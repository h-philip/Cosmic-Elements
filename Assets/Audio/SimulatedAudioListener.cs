using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This script moves the transform back and forth to compensate for the camera not actually moving away from the gameobjects but them only getting scaled.
/// </summary>
public class SimulatedAudioListener : MonoBehaviour
{
    public float ScaleMultiplier = 1.0f;

    private SpaceSimulation _simulation;
    private float _initialDistance;

    // Start is called before the first frame update
    private void Start()
    {
        _simulation = FindObjectOfType<SpaceSimulation>();
        _initialDistance = Camera.main.transform.position.magnitude;
    }

    // Update is called once per frame
    private void Update()
    {
        // Smaller scale -> further away
        // Bigger scale -> closer

        float distance = ScaleMultiplier / _simulation.transform.localScale.x + _initialDistance;
        transform.position = (Camera.main.transform.position - _simulation.ReferenceObject.transform.position).normalized * distance;
    }
}
