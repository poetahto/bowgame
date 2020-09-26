using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PressurePlate : Button
{
    private Current charge = new Current();

    private void OnCollisionEnter(Collision collision)
    {
        ControllableObject obj = collision.gameObject.GetComponent<ControllableObject>();
        if (obj != null)
        {
            AddCharge(charge);
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        ControllableObject obj = collision.gameObject.GetComponent<ControllableObject>();
        if (obj != null)
        {
            RemoveCharge(charge);
        }
    }
}
