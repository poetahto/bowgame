using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEditor;
using UnityEngine;

// The different ways an object can be powered, enums look nice in editor
public enum ChargeType
{ 
    Arrow,
    Current
}

// Represents an object that can accept and store charges. Maybe incorperate events
// for stuff like charges being added to ease some code smell and make things easy to extend
public abstract class ChargeableObject : MonoBehaviour
{
    [Header("Chargeable Settings")]
    // The types of charges this object can accept (use case: make doors ignore arrows)
    [SerializeField] private ChargeType[] acceptedChargeTypes = new ChargeType[1] { ChargeType.Arrow };
    
    // stores all of the charges this object currently has (arrows, current ect)
    protected List<Charge> charges = new List<Charge>();

    private void Start()
    {
        DebugUtil.AddMessage(gameObject, gameObject.name);
    }

    public virtual void AddCharge(Charge charge)
    {
        if (acceptedChargeTypes.Contains(charge.Type()))
        {
            DebugUtil.AddMessage(gameObject, charge.Type().ToString());
            charges.Add(charge);
        }
    }

    public virtual void RemoveCharge(Charge charge)
    {
        DebugUtil.ClearMessage(gameObject, charge.Type().ToString());
        charges.Remove(charge);
    }
}
