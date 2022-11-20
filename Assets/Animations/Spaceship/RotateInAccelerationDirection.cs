using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateInAccelerationDirection : MonoBehaviour
{
    private Spaceship _spaceship;
    private Vector3 _acceleration;
    private Components.Vector3 _accelerationD;
    private Quaternion _rotation;

    // Start is called before the first frame update
    void Start()
    {
        _spaceship = GetComponent<Spaceship>();
        if (_spaceship == null)
            _spaceship = GetComponentInChildren<Spaceship>();
        if (_spaceship == null)
            _spaceship = transform.parent.GetComponent<Spaceship>();
    }

    // Update is called once per frame
    void Update()
    {
        _accelerationD = _spaceship.Thrust.normalized;
        _acceleration.x = (float)_accelerationD.x;
        _acceleration.y = (float)_accelerationD.y;
        _acceleration.z = (float)_accelerationD.z;
        if (_acceleration == Vector3.zero)
            return;
        _rotation.SetLookRotation(_acceleration);
        transform.rotation = Quaternion.Lerp(transform.rotation, _rotation, 1f * Time.deltaTime);
    }
}
