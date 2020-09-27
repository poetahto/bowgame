using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;

// Also called a button. Accepts charges, and can distribute Current to 
// linked objects in order to power them.

public class Button : MonoBehaviour, IChargeableObject
{
    // TODO: is there a better way to handle the inverse targets? code smell

    [Header("Acceptor Settings")]
    [SerializeField] private ChargeType[] acceptedChargeTypes = null;
    [SerializeField] private GameObject[] targets = null;
    [SerializeField] private GameObject[] inverseTargets = null;
    [SerializeField] private int maxCharges = 1;

    private Current current;
    private List<Charge> charges;

    private void Awake()
    {
        current = new Current();
        charges = new List<Charge>();
    }

    private void Start()
    {
        foreach (var target in inverseTargets)
        {
            target.GetComponent<IChargeableObject>()?.AddCharge(current);
        }
    }

    public void AddCharge(Charge charge)
    {
        if (charges.Count < maxCharges && acceptedChargeTypes.Contains(charge.Type()))
        {
            DebugUtil.AddMessage(gameObject, charge.GetType().ToString());
            charges.Add(charge);

            foreach (var target in inverseTargets)
            {
                target.GetComponent<IChargeableObject>()?.RemoveCharge(current);
            }
            foreach (var target in targets)
            {
                target.GetComponent<IChargeableObject>()?.AddCharge(current);
            }
        }
    }

    public void RemoveCharge(Charge charge)
    {
        DebugUtil.ClearMessage(gameObject, charge.GetType().ToString());
        charges.Remove(charge);

        foreach (var target in inverseTargets)
        {
            target.GetComponent<IChargeableObject>()?.AddCharge(current);
        }
        foreach (var target in targets)
        {
            target.GetComponent<IChargeableObject>()?.RemoveCharge(current);
        }
    }

    public ChargeType[] AcceptedChargeTypes()
    {
        return acceptedChargeTypes;
    }
}
