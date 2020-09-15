using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArrowRemover : MonoBehaviour
{
    // Start is called before the first frame update
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
