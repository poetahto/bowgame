using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct ControllableProperties
{
    public float JumpHeight;
    public float MaxSpeed;
    public float GroundAcceleration;
    public float AirAcceleration;
    public float MaxAirJumps;
}

public class Human : ControllableObject
{
    public override ControllableProperties Properties { 
        get 
        {
            ControllableProperties prop;
            prop.JumpHeight = 1f;
            prop.MaxSpeed = 4f;
            prop.GroundAcceleration = 26f;
            prop.AirAcceleration = 4f;
            prop.MaxAirJumps = 0f;

            return prop; 
        } 
    }
}
