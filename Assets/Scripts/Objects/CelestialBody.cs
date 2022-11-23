using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Components;

public class CelestialBody : MonoBehaviour
{
    /// <summary>
    /// Radius
    /// </summary>
    public float Radius = 1;
    /// <summary>
    /// Units scale
    /// </summary>
    public double Scale = 1d;

    public Components.Vector3d Position;

    public Bounds CombinedRenderBounds { get; private set; }

    private SpaceSimulation _simulation;
    

    public const float MIN_SIZE = 0.1f;

    private void Awake()
    {
        gameObject.layer = LayerMask.NameToLayer("CelestialBodies");
    }

    private void Start()
    {
        _simulation = FindObjectOfType<SpaceSimulation>();

        ScaleMesh();
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

    private void Update()
    {
        PositionUpdate();
    }

    public Vector3d GetTranslatedWorldPosition()
    {
        return Position + _simulation.TranslateVector;
    }
}
