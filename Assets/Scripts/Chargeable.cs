using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEditor;
using UnityEngine;

public enum ChargeType
{ 
    Arrow,
    Current
}

public abstract class Chargeable : MonoBehaviour
{
    [Header("Chargeable Settings")]
    [SerializeField] private ChargeType[] acceptedChargeTypes = new ChargeType[0];

    private Type[] chargeTypes = new Type[] { typeof(Arrow), typeof(Current) };
    protected List<Charge> charges = new List<Charge>();

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
        foreach (ChargeType type in acceptedChargeTypes)
        {
            if (charge.GetType() == chargeTypes[(int)type])
            {
                charges.Add(charge);
            }
        }
    }

    public virtual void RemoveCharge(Charge charge)
    {
        charges.Remove(charge);
    }
}
