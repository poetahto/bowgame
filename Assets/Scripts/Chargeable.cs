using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface Chargeable
{
    void OnHit();

    void AddCharge(Arrow arrow);

    void RemoveCharge(Arrow arrow);
}
