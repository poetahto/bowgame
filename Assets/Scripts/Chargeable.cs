using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Chargeable : MonoBehaviour
{
    public abstract void AddCharge(Charge charge);

    public abstract void RemoveCharge(Charge charge);
}
