using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class SpaceSimulation : MonoBehaviour
{
    private const float MIN_SCALE = 0.0000001f, MAX_SCALE = 100;

    [Header("General")]
    public bool Paused = false;
    public double DistanceScale = 0.1;
    public double GravConst = 6.674e-20d;
    // Private
    private Spaceship[] _spaceships;
    private ControlStation[] _controlStation;
    private Coroutine _simulationUpdate;

    [Header("Controls")]
    public float RotationSpeed = 20f;
    public float ZoomSpeed = 5f;
    // Private
    private CelestialBody _referenceObject;
    private bool _leftClicked, _rightClicked;
    private Camera _camera;
    private Vector2 _rotate;
    private float _zoom;
    private int _currentTimeScaleIndex = 1;
    private double[] _timeScales = new double[] { 0, 1, 2, 5, 10 }; //, 20, 50, 100, 1000, 10_000, 50_000, 100_000 }; // TODO: Currently capped to 10 to prevent unexpected and undefined behavior

    // Properties
    public double TimeScale
    {
        get => _timeScales[_currentTimeScaleIndex];
        set
        {
            int closest = 0;
            double close = double.MaxValue;
            for (int i = 0; i < _timeScales.Length; i++)
                if (System.Math.Abs(_timeScales[i] - value) < close)
                {
                    closest = i;
                    close = System.Math.Abs(_timeScales[i] - value);
                }
            _currentTimeScaleIndex = closest;
        }
    }
    public double AbsoluteTime { get; private set; }
    public CelestialBody ReferenceObject
    {
        get { return _referenceObject; }
        set
        {
            _referenceObject = value;
        }
    }
    public SimpleKeplerOrbits.Vector3d TranslateVector
    {
        get
        {
            return _referenceObject != null ? -_referenceObject.GetWorldPosition() : SimpleKeplerOrbits.Vector3d.zero;
        }
    }

    public bool PointerOverUi { get; set; }

    // Unity Methods

    private void Awake()
    {
        ReferenceObject = transform.Find("Earth").GetComponent<CelestialBody>();
        _camera = Camera.main;
    }

    private void Start()
    {        
        UpdateSpaceships();
        UpdateControlStations();

        _simulationUpdate = StartCoroutine(SimulationUpdate());
    }

    private void LateUpdate()
    {
        // Rotation
        _camera.transform.RotateAround(transform.position, _camera.transform.right, -_rotate.y * RotationSpeed * Time.deltaTime);
        _camera.transform.RotateAround(transform.position, _camera.transform.up, _rotate.x * RotationSpeed * Time.deltaTime);

        // Zoom
        if (_zoom != 0)
        {
            float zoom = Time.deltaTime * ZoomSpeed * _zoom;
            //// First do scaling
            //if (false && ((transform.localScale.x > MIN_SCALE && zoom < 0) || (transform.localScale.x < MAX_SCALE && zoom > 0)))
            //{
            float scale = transform.localScale.x + transform.localScale.x * zoom;
            scale = Mathf.Clamp(scale, MIN_SCALE, MAX_SCALE);
            transform.localScale = Vector3.one * scale;
            //}
            //// Then do camera movement
            //else
            //{
            //    float maxCameraDistance = Camera.main.farClipPlane / 2;
            //    // Distance thanks to: http://answers.unity.com/answers/1190942/view.html
            //    float minCameraDistance = Mathf.Max(ReferenceObject.CombinedRenderBounds.size.x, ReferenceObject.CombinedRenderBounds.size.y, ReferenceObject.CombinedRenderBounds.size.z) * ReferenceObject.transform.lossyScale.x;
            //    minCameraDistance /= 2.0f * Mathf.Tan(0.5f * _camera.fieldOfView * Mathf.Deg2Rad);
            //    float cameraDistance = (_camera.transform.position - ReferenceObject.transform.position).magnitude;
            //    cameraDistance -= zoom * 10000;
            //    cameraDistance = Mathf.Clamp(cameraDistance, minCameraDistance, maxCameraDistance);
            //    _camera.transform.position = ReferenceObject.transform.position + -_camera.transform.forward * cameraDistance;
            //}
        }
    }

    private void OnDisable()
    {
        StopCoroutine(_simulationUpdate);
        _simulationUpdate = null;
    }

    // Simulation Methods

    private IEnumerator SimulationUpdate()
    {
        yield return new WaitForFixedUpdate();
        for (; ; )
        {
            double deltaTime = (double)Time.fixedDeltaTime * TimeScale;
            AbsoluteTime += deltaTime;
            foreach (CelestialBody celestialBody in GetComponentsInChildren<CelestialBody>())
                celestialBody.SimulationUpdate(deltaTime);
            foreach (Spaceship spaceship in _spaceships)
                spaceship.SimulationUpdate(deltaTime);
            foreach (ControlStation controlStation in _controlStation)
                controlStation.SimulationUpdate(deltaTime);
            if (TimeScale <= 0)
                yield return new WaitWhile(() => TimeScale <= 0);
            if (Paused)
                yield return new WaitWhile(() => Paused);
            yield return new WaitForFixedUpdate();
        }
    }

    public void UpdateSpaceships()
    {
        _spaceships = Spaceship.GetAllSpaceships(false, false);
    }

    public void UpdateControlStations()
    {
        _controlStation = GetComponentsInChildren<ControlStation>();
    }

    // Control Methods
    public void LeftClick(InputAction.CallbackContext context)
    {
        if (context.started && !PointerOverUi)
            _leftClicked = true;
        else if (context.canceled)
        {
            _leftClicked = false;
            _rotate = Vector2.zero;
        }
    }

    public void RightClick(InputAction.CallbackContext context)
    {
        if (context.started && !PointerOverUi)
            _rightClicked = true;
        else if (context.canceled)
        {
            _rightClicked = false;
            _zoom = 0;
        }
    }

    public void Move(InputAction.CallbackContext context)
    {
        _rotate = context.ReadValue<Vector2>();
    }

    public void Look(InputAction.CallbackContext context)
    {
        if (_leftClicked)
        {
            // Rotate camera
            _rotate = context.ReadValue<Vector2>();
        }
        else if (_rightClicked)
        {
            // Zoom camera
            _zoom = context.ReadValue<Vector2>().y;
        }
    }

    public void Scroll(InputAction.CallbackContext context)
    {
        if (PointerOverUi) return;

        _zoom = context.ReadValue<Vector2>().y * 0.1f;
        if (_zoom < 0)
            _zoom = -10;
        else if (_zoom > 0)
            _zoom = 10;
    }

    public void IncreaseTimeScale(InputAction.CallbackContext context)
    {
        if (context.canceled && _currentTimeScaleIndex + 1 < _timeScales.Length)
        {
            _currentTimeScaleIndex++;
            FindObjectOfType<WorldMapController>(true).SetTimeScaleText(TimeScale);
        }
    }

    public void DecreaseTimeScale(InputAction.CallbackContext context)
    {
        if (context.canceled && _currentTimeScaleIndex > 0)
        {
            _currentTimeScaleIndex--;
            FindObjectOfType<WorldMapController>(true).SetTimeScaleText(TimeScale);
        }
    }
}
