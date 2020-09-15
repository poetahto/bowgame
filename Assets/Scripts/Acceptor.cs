using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class Acceptor : Chargeable
{
    [SerializeField] private Chargeable[] targets = null;
    [SerializeField] private Chargeable[] inverseTargets = null;

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
        foreach (var target in inverseTargets)
        {
            target.RemoveCharge(current);
        }
        foreach (var target in targets)
        {
            target.AddCharge(current);
        }
    }

    public override void RemoveCharge(Charge charge)
    {
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
