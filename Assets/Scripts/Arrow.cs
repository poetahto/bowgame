using UnityEngine;

public class Arrow : MonoBehaviour
{
    // how far the arrow will lodge itself into objects
    [SerializeField] private float stickDepth = .2f;

    // how lenient we should be when deciding whether the arrow should stick or bounce
    // - higher numbers are more lenient 
    [SerializeField] private float stickLeniency = 1.2f;

    // whether the arrow can currently get stuck in something
    private bool active = true;
    private Chargeable curCharging = null;

    // this arrows rigidbody
    private new Rigidbody rigidbody;

    private void Awake()
    {
        rigidbody = GetComponent<Rigidbody>();
    }

    private void OnDestroy()
    {
        curCharging?.RemoveCharge(this);
    }

    private void OnCollisionEnter(Collision collision)
    {
        // the object this arrow collided with
        GameObject hitObject = collision.gameObject;

        // make sure the arrow never collides with the player
        if (hitObject.tag == "Player") return;

        if (hitObject.tag == "Sticky" && active)
        {
            bool shouldStick = false;

            // iterate through every point that this arrow collided with
            for (int i = 0; i < collision.contactCount; i++)
            {
                // if the arrow hit the surface head on, then it should stick
                Vector3 normal = transform.forward + collision.GetContact(i).normal;
                shouldStick |= normal.magnitude < stickLeniency;
            }

            if (shouldStick)
            {
                // if the object we hit has a component that impliments ShootableObject, call it's OnHit method
                curCharging = hitObject.GetComponent<Chargeable>();
                curCharging?.OnHit();
                curCharging?.AddCharge(this);

                // sink the arrow a little deeper into the object we hit
                LeanTween.move(gameObject, transform.position + transform.forward.normalized * stickDepth, 0.02f);

                // track the position of the object we hit
                transform.SetParent(hitObject.transform);

                // freeze the arrow in place
                Destroy(rigidbody);
            }
        }
        // after this arrow has collided with something, it can no longer get stuck
        active = false;
    }
}
