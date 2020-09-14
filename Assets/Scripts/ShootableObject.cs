using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ShootableObject
{
    void OnHit(ArrowLogic arrow);
}
