using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bow : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            var arrow = Instantiate(arrowPrefab);
            var arrowRigidbody = arrow.GetComponent<Rigidbody>();

            arrow.transform.SetParent(arrowGroup);
            arrow.transform.position = characterTransform.position + (transform.forward.normalized * 0.5f) + (transform.up * 0.5f);
            arrow.transform.rotation = cameraTransform.rotation;
            arrowRigidbody.velocity = (cameraTransform.forward.normalized * 10);

            arrows.Add(arrow);
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }

        if (Input.GetKeyDown(KeyCode.Return))
        {
            foreach (GameObject arrow in arrows)
            {
                Destroy(arrow);
            }
        }
    }


}
