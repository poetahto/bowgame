using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Really basic script for removing arrows when a player collides.
// add a bit of abstraction to this class later (like a trigger wrapper class)

public class ArrowRemover : MonoBehaviour
{
    private void OnTriggerExit(Collider collider)
    {
        //Checks to see if the object leaving is the player
        if (collider.gameObject.tag.Equals("Player"))
        {
            //clears the aarows
            collider.gameObject.GetComponent<Bow>().CollectArrows();
        }
    }
}
