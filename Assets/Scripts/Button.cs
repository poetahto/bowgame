using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;

// Also called a button. Accepts charges, and can distribute Current to 
// linked objects in order to power them.

public class Button : ChargeableObject
{
    // TODO: is there a better way to handle the inverse targets? code smell

    [Header("Acceptor Settings")]
    [SerializeField] private ChargeableObject[] targets = null;
    [SerializeField] private ChargeableObject[] inverseTargets = null;
    [SerializeField] private int maxCharges = 1;

    private Current current;

    private void Awake()
    {
        current = new Current();
    }

    private void Start()
    {
        foreach (var target in inverseTargets)
        { 
            target.AddCharge(current);
        }
    }

    public override void AddCharge(Charge charge)
    {
        if (charges.Count < maxCharges)
        {
            base.AddCharge(charge);

            foreach (var target in inverseTargets)
            {
                target.RemoveCharge(current);
            }
            foreach (var target in targets)
            {
                target.AddCharge(current);
            }
        }
    }

    public override void RemoveCharge(Charge charge)
    {
        base.RemoveCharge(charge);

        foreach (var target in inverseTargets)
        {
            target.AddCharge(current);
        }
        foreach (var target in targets)
        {
            target.RemoveCharge(current);
        }
    }
}
