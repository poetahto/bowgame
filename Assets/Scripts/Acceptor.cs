using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;

public class Acceptor : Chargeable
{
    // TODO: is there a better way to handle the inverse targets? code smell

    [Header("Acceptor Settings")]
    [SerializeField] private Chargeable[] targets = null;
    [SerializeField] private Chargeable[] inverseTargets = null;
    [SerializeField] private int maxCharges = 1;

    private Current current;

    private void Awake()
    {
        current = new Current();
    }

#if (UNITY_EDITOR)
    private void OnDrawGizmos()
    {
        String message = String.Format("{0}\nCharges: {1}/{2}",
            gameObject.name,
            charges.Count,
            maxCharges);

        Handles.Label(transform.position, message);

        Handles.color = Color.clear;
        foreach (var target in inverseTargets)
        {
            if (charges.Count == 0) Handles.color = Color.green;
            Handles.DrawLine(target.transform.position, transform.position);
        }

        Handles.color = Color.clear;
        foreach (var target in targets)
        {
            if (charges.Count > 0) Handles.color = Color.green;
            Handles.DrawLine(target.transform.position, transform.position);
        }
    }
#endif

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
