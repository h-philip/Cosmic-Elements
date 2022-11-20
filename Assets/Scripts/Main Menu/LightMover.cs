using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightMover : MonoBehaviour
{
    public Transform MoveAround;
    public float MoveSpeed = 1;

    private void Update()
    {
        transform.RotateAround(MoveAround.transform.position, Vector3.down, Time.deltaTime * MoveSpeed);
    }
}
