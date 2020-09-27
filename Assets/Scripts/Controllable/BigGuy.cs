using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BigGuy : ControllableObject
{
    private ControllableProperties _properties;
    public override ControllableProperties Properties { get { return _properties; } }

    private void Awake()
    {
        _properties.JumpHeight = 0f;
        _properties.MaxSpeed = 2f;
        _properties.GroundAcceleration = 16f;
        _properties.AirAcceleration = 2f;
        _properties.MaxAirJumps = -1f;
        _properties.MaxGroundAngle = 30f;
        _properties.CameraOffset = new Vector3(0f, 1.8f, 0f);
    }
}