using UnityEngine;

[RequireComponent(typeof(Rigidbody))] 
public class AarrowLogic : MonoBehaviour
{
    public float stickValue = .2f;
    bool stoped = false;

    Rigidbody rigidbody;

    // Start is called before the first frame update
    void Start()
    {
        rigidbody = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!stoped)
        {
            transform.LookAt(rigidbody.velocity);
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag != "Player")
        {
            transform.position += transform.forward.normalized * stickValue;
            rigidbody.isKinematic = true;
            stoped = true;

        }
    }
}
