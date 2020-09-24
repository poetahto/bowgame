using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// An object that can be used as a power source for other objects
public interface Charge 
{
    ChargeType Type();
}
