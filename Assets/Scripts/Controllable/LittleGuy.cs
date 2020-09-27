using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LittleGuy : ControllableObject
{
    private ControllableProperties _properties;
    public override ControllableProperties Properties { get { return _properties; } }

    private void Awake()
    {
        _properties.JumpHeight = 1f;
        _properties.MaxSpeed = 5f;
        _properties.GroundAcceleration = 26f;
        _properties.AirAcceleration = 8f;
        _properties.MaxAirJumps = 2f;
        _properties.MaxGroundAngle = 30f;
        _properties.CameraOffset = new Vector3(0f, 0f, 0f);
    }
}