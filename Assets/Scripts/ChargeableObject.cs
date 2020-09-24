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
    [SerializeField] private ChargeType[] acceptedChargeTypes = new ChargeType[0];
    
    // stores all of the charges this object currently has (arrows, current ect)
    protected List<Charge> charges = new List<Charge>();

    // TODO honestly, come up with a big singleton class for debug helper methods
    // they dont really belong here or in the Acceptor class
#if (UNITY_EDITOR)
    private void OnDrawGizmos()
    {
        String message = String.Format("{0}\nCharges: {1}", 
            gameObject.name,
            charges.Count);

        Handles.Label(transform.position, message);
    }
#endif

    public virtual void AddCharge(Charge charge)
    {
        if (acceptedChargeTypes.Contains(charge.Type()))
        {
            charges.Add(charge);
        }
    }

    public virtual void RemoveCharge(Charge charge)
    {
        charges.Remove(charge);
    }
}
