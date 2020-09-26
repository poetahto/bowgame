using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class ChargedHuman : ControllableObject, IChargeableObject
{
    private ControllableProperties _properties;
    public override ControllableProperties Properties { get { return _properties; } }
    private List<Charge> charges = new List<Charge>();
    private ControllableObject originalController = null;

    public ChargeType[] AcceptedChargeTypes()
    {
        return new ChargeType[] { ChargeType.Arrow };
    }

    private void LateUpdate()
    {
        if (charges.Count > 0 && Controller.instance.currentlyControlling != this)
        {
            originalController = Controller.instance.currentlyControlling;
            Controller.instance.AttachTo(this);
        }
        else if (charges.Count <= 0 && Controller.instance.currentlyControlling == this)
        {
            DetachController();
            Controller.instance.AttachTo(originalController);
        }
    }

    public void AddCharge(Charge charge)
    {
        if (charge.Type() == ChargeType.Arrow)
        {
            DebugUtil.AddMessage(gameObject, charge.GetType().ToString());
            charges.Add(charge);
        }
    }

    public void RemoveCharge(Charge charge)
    {
        DebugUtil.ClearMessage(gameObject, charge.GetType().ToString());
        charges.Remove(charge);
    }

    private void Awake()
    {
        _properties.JumpHeight = 1f;
        _properties.MaxSpeed = 4f;
        _properties.GroundAcceleration = 26f;
        _properties.AirAcceleration = 4f;
        _properties.MaxAirJumps = 0f;
        _properties.MaxGroundAngle = 30f;
        _properties.CameraOffset = new Vector3(0f, 0.5f, 0f);
    }
}