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
public interface IChargeableObject
{
    ChargeType[] AcceptedChargeTypes();
    void AddCharge(Charge charge);
    void RemoveCharge(Charge charge);
}
