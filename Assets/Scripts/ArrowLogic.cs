using UnityEngine;

public class ArrowLogic : MonoBehaviour
{
    public float stickValue = .2f;
    bool stopped = false;
    bool active = true;

    Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        if (!stopped)
        {
            //transform.LookAt(rb.velocity);
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Player") return;

        if (collision.gameObject.tag == "sticky" && !stopped && active)
        {
            bool isHit = false;

            for (int i = 0; i < collision.contactCount; i++)
            {
                Vector3 normal = transform.forward + collision.GetContact(i).normal;
                isHit |= normal.magnitude < 1.2f;
            }

            if (isHit)
            {
                collision.gameObject.GetComponent<ShootableObject>()?.OnHit(this);

                LeanTween.move(gameObject, transform.position + transform.forward.normalized * stickValue, 0.05f);
                rb.constraints = RigidbodyConstraints.FreezeAll;
                Destroy(rb);
                stopped = true;
            }
            
        }
        active = false;
    }
}
