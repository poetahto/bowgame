using UnityEngine;

[RequireComponent(typeof(Rigidbody))] 
public class ArrowLogic : MonoBehaviour
{
    public float stickValue = .2f;
    bool stopped = false;

    Rigidbody rb;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!stopped)
        {
            transform.LookAt(rb.velocity);
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        Debug.Log("collided");
        if (collision.gameObject.GetComponent<ShootableObject>() != null)
        {
            collision.gameObject.GetComponent<ShootableObject>().OnHit(this);
        }
            
        if (collision.gameObject.tag != "Player" && !stopped)
        {
            
            transform.position += transform.forward.normalized * stickValue;
            rb.constraints = RigidbodyConstraints.FreezeAll;
            stopped = true;

        }
    }
}
