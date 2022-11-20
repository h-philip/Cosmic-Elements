using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlanetRotator : MonoBehaviour
{
    public float RotationSpeed = 1;

    void Update()
    {
        Vector3 euler = transform.rotation.eulerAngles;
        euler.y -= Time.deltaTime * RotationSpeed;
        transform.rotation = Quaternion.Euler(euler);
    }
}
