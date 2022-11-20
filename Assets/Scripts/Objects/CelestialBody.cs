using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// TODO: Use job system to make computing positions multithreaded
public class CelestialBody : MonoBehaviour
{
    /// <summary>
    /// Eccentricity
    /// </summary>
    [Tooltip("Eccentricity")]
    public double EC;
    /// <summary>
    /// SemiMajorAxis
    /// </summary>
    [Tooltip("SemiMajorAxis")]
    public double A;
    /// <summary>
    /// MeanAnomalyDeg
    /// </summary>
    [Tooltip("MeanAnomalyDeg")]
    public double MA;
    /// <summary>
    /// InclinationDeg
    /// </summary>
    [Tooltip("InclinationDeg")]
    public double IN;
    /// <summary>
    /// ArgOfPerifocusDeg
    /// </summary>
    [Tooltip("ArgOfPerifocusDeg")]
    public double W;
    /// <summary>
    /// AscendingNodeDeg
    /// </summary>
    [Tooltip("AscendingNodeDeg")]
    public double OM;
    /// <summary>
    /// AttractorMass
    /// </summary>
    [Tooltip("AttractorMass")]
    public double AttractorMass;
    /// <summary>
    /// Mass
    /// </summary>
    public float Mass;
    /// <summary>
    /// Radius
    /// </summary>
    public float Radius = 1;
    /// <summary>
    /// Attractor
    /// </summary>
    public Transform Attractor;
    /// <summary>
    /// Units scale
    /// </summary>
    public double Scale = 1d;

    public Bounds CombinedRenderBounds { get; private set; }

    public SimpleKeplerOrbits.KeplerOrbitData KeplerOrbitData = new SimpleKeplerOrbits.KeplerOrbitData();
    private SpaceSimulation _simulation;
    private Spaceship _spaceship = null;
    private CelestialBody[] _siblingBodies;
    

    public const float MIN_SIZE = 0.1f;

    private void Awake()
    {
        gameObject.layer = LayerMask.NameToLayer("CelestialBodies");
        _siblingBodies = new CelestialBody[0];
    }

    private void Start()
    {
        _simulation = FindObjectOfType<SpaceSimulation>();
        _spaceship = GetComponent<Spaceship>();

        Init();
        ScaleMesh();
    }

    public void Init()
    {
        if (A > 0 && !KeplerOrbitData.IsValidOrbit)
        {
            KeplerOrbitData = new SimpleKeplerOrbits.KeplerOrbitData(
                eccentricity: EC,
                semiMajorAxis: A * 1.495978707e8d,
                meanAnomalyDeg: MA,
                inclinationDeg: IN,
                argOfPerifocusDeg: W,
                ascendingNodeDeg: OM,
                attractorMass: AttractorMass,
                gConst: FindObjectOfType<SpaceSimulation>().GravConst);
            KeplerOrbitData.CalculateOrbitStateFromOrbitalElements();
        }
    }

    private void ScaleMesh()
    {
        // Temporarily move it out of any parents so that their scales are ignored
        Transform parent = transform.parent;
        transform.SetParent(null, false);

        // Following was taken from https://stackoverflow.com/a/11960936

        // First find a center for your bounds.
        Vector3 center = Vector3.zero;

        List<Renderer> renderers = new List<Renderer>();
        GetComponentsInChildren(true, renderers);
        foreach (Renderer renderer in renderers)
        {
            center += renderer.bounds.center;
        }
        center /= renderers.Count; // Center is average center of children

        //Now you have a center, calculate the bounds by creating a zero sized 'Bounds', 
        Bounds bounds = new Bounds(center, Vector3.zero);

        foreach (Renderer renderer in renderers)
        {
            bounds.Encapsulate(renderer.bounds);
        }
        CombinedRenderBounds = bounds;

        float renderDiameter = Mathf.Max(bounds.size.x, bounds.size.y, bounds.size.z);
        transform.localScale *= Mathf.Max((float)Scale * Radius * 2, MIN_SIZE) / renderDiameter;

        transform.SetParent(parent, false);
    }

    public void PositionUpdate()
    {
        // Set own position
        var posD = GetTranslatedWorldPosition();
        Vector3 pos = new Vector3((float)posD.x, (float)posD.y, (float)posD.z);
        transform.localPosition = pos;
    }

    public void SimulationUpdate(double deltaTime)
    {
        if (KeplerOrbitData != null)
            KeplerOrbitData.UpdateOrbitDataByTime(deltaTime);

        if (_spaceship != null && _spaceship.Mass > 0)
        {
            // Check if too far away from current attractor
            double f = (KeplerOrbitData.GravConst * KeplerOrbitData.AttractorMass * _spaceship.Mass) / System.Math.Pow(KeplerOrbitData.Position.magnitude, 2);
            if (f < 0.01)
                SetNewAttractor(Attractor.GetComponent<CelestialBody>().Attractor.GetComponent<CelestialBody>());
            else {
                // Check for capture by other celestial body
                foreach (CelestialBody sibling in _siblingBodies)
                {
                    // Check if orbit would be 
                    f = (KeplerOrbitData.GravConst * sibling.Mass * _spaceship.Mass) / System.Math.Pow((GetWorldPosition() - sibling.GetWorldPosition()).magnitude, 2);
                    if (f < 0.01)
                        SetNewAttractor(sibling);
                }
            }
        }
    }

    private void SetNewAttractor(CelestialBody attractor)
    {
        Debug.Log(name + ": Switch attractor from " + Attractor.name + " to " + attractor.name);
        SimpleKeplerOrbits.Vector3d vel = KeplerOrbitData.Velocity;
        KeplerOrbitData = new SimpleKeplerOrbits.KeplerOrbitData(
            GetWorldPosition() - attractor.GetWorldPosition(),
            vel,
            attractor.Mass,
            KeplerOrbitData.GravConst);
        Attractor = attractor.transform;
    }

    private void Update()
    {
        PositionUpdate();
    }

    public SimpleKeplerOrbits.Vector3d GetWorldPosition()
    {
        CelestialBody attractor;
        if (Attractor.TryGetComponent(out attractor))
        {
            return attractor.GetWorldPosition() + KeplerOrbitData.Position * Scale;
        }
        else
        {
            Vector3 parPos = Attractor.position;
            SimpleKeplerOrbits.Vector3d parPosD = new SimpleKeplerOrbits.Vector3d(parPos.x, parPos.y, parPos.z);
            var ownPos = KeplerOrbitData != null ? KeplerOrbitData.Position : SimpleKeplerOrbits.Vector3d.zero;
            return parPosD + ownPos * Scale;
        }
    }

    public SimpleKeplerOrbits.Vector3d GetTranslatedWorldPosition()
    {
        return GetWorldPosition() + _simulation.TranslateVector;
    }
}
