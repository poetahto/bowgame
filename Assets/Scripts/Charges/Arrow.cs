﻿using UnityEngine;

public class Arrow : MonoBehaviour, Charge
{
    // how far the arrow will lodge itself into objects
    [SerializeField] private float stickDepth = .2f;

    // how lenient we should be when deciding whether the arrow should stick or bounce
    // - higher numbers are more lenient 
    [SerializeField] private float stickLeniency = 1.2f;

    // whether the arrow can currently get stuck in something
    private bool active = true;

    // the object this arrow is currently charging
    private IChargeableObject curCharging = null;

    // this arrows rigidbody
    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        if (active)
        {
            /// --- DEBUG STUFF ---
            Vector3 start = transform.position + transform.forward.normalized * 0.25f;
            Vector3 target = transform.position + rb.velocity.normalized;
            Debug.DrawLine(start, target);
            /// --- DEBUG STUFF ---

            // makes the arrow face the direction it is moving
            transform.forward = rb.velocity.normalized;
        }
    }

    private void OnDestroy()
    {
        // if we are powering an object, unpower it when this is destroyed
        curCharging?.RemoveCharge(this);
    }

    private void OnCollisionEnter(Collision collision)
    {
        // the object this arrow collided with
        GameObject hitObject = collision.gameObject;

        // make sure the arrow never collides with the player
        if (hitObject.transform.position == Controller.instance.currentlyControlling.transform.position)
        {
            return;
        }

        /*
        if (hitObject.layer == 8)
        {
            ControllableObject obj = hitObject.transform.GetComponent<ControllableObject>();
            Controller.instance.AttachTo(obj);
        }
        */

        if (hitObject.CompareTag("Sticky") && active)
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
                curCharging = hitObject.GetComponent<IChargeableObject>();
                curCharging?.AddCharge(this);

                // sink the arrow a little deeper into the object we hit
                LeanTween.move(gameObject, transform.position + transform.forward.normalized * stickDepth, 0.05f);

                // track the position of the object we hit
                transform.SetParent(hitObject.transform);
                if (collision.rigidbody != null) collision.rigidbody.velocity = Vector3.zero;
                // freeze the arrow in place
                Destroy(rb);
            }
        }
        // after this arrow has collided with something, it can no longer get stuck
        active = false;
    }

    public ChargeType Type()
    {
        return ChargeType.Arrow;
    }
}
