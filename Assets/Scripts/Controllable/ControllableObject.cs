using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Represents the player that will move around and interact with the 
// game world. A pretty hefty class, might need to be abstracted if
// possible. 

// Best way to learn about this class is to jump right into
// the Update and FixedUpdate methods, most of the stuff there
// is explained with comments.


[RequireComponent(typeof(Rigidbody))]
public abstract class ControllableObject : MonoBehaviour
{
    public Controller Controller { get; set; }
    public Rigidbody body;

    public abstract ControllableProperties Properties { get; }

    private bool OnGround => groundContactCount > 0;
    private bool desiredJump;
    private float minNormalY;
    private int usedJumps;
    private int groundContactCount;
    private Rigidbody connectedBody, previousConnectedBody;
    private Vector3 velocity, desiredVelocity, connectionVelocity;
    private Vector3 connectionWorldPosition, connectionLocalPosition;
    private Vector3 contactNormal;

    public void Start()
    {
        minNormalY = Mathf.Cos(Properties.MaxGroundAngle * Mathf.Deg2Rad);
    }

    void Update()
    {
        if (Controller != null)
        {
            /*
                uses keyboard input to update our desiredVelocity 
                (doesn't actually move player)
            */
            HandleKeyboardInput();
            /*
                process keyboard inputs that are only for testing
            */
            HandleDebugInput();
        }
    }

    void FixedUpdate()
    {
        /*
            accelerate towards the desiredVelocity by a fixed amount every fixed
            update (50 times per second), operates independent of framerate
        */
        UpdateVelocity();    
    }

    private void HandleDebugInput()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            GameManager.LoadLevel(Scene.Test);
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            RaycastHit hit;

            if (Physics.Raycast(Controller.instance.transform.position, Controller.instance.transform.TransformDirection(Vector3.forward), out hit, 1 << 8))
            {
                ControllableObject obj = hit.transform.GetComponent<ControllableObject>();
                Controller.AttachTo(obj);
            }
        }
    }

    private void HandleKeyboardInput()
    {
        // store the player's movement inputs
        Vector2 playerInput;
        playerInput.x = Input.GetAxisRaw("Horizontal");
        playerInput.y = Input.GetAxisRaw("Vertical");

        // normalize movement to stop strafe speed bonus
        playerInput = Vector2.ClampMagnitude(playerInput, 1f);

        // if they player pressed jump, make sure desiredJump is set to true
        desiredJump |= Input.GetButtonDown("Jump");

        // convert player's input into a 3D vector that represents a target velocity
        desiredVelocity = (transform.right * playerInput.x + transform.forward * playerInput.y) * Properties.MaxSpeed;
    }

    private void UpdateVelocity()
    {
        // Ensure that our locally stored variables are updated and reflect the player state
        UpdateState();

        // Update our local velocity variable according to the players most recent inputs
        AdjustVelocity();

        // If the player wants to jump, try to jump
        if (desiredJump)
        {
            desiredJump = false;
            Jump();
        }

        // Update rigidbody velocity to match our local velocity variable
        body.velocity = velocity;

        // Reset player state so it can be re-calculated in the next physics tick
        ClearState();
    }

    private void AdjustVelocity()
    {
        // Project the x and z axis along the floor we are standing on.
        // This ensures smooth movement up / down slopes.
        Vector3 xAxis = ProjectOnContactPlane(Vector3.right).normalized;
        Vector3 zAxis = ProjectOnContactPlane(Vector3.forward).normalized;

        // Make sure our velocity will be affected by any object we are standing on
        Vector3 relativeVelocity = velocity - connectionVelocity;

        // Adjusts our acceleration based on whether we are grounded or not
        float accel = OnGround ? Properties.GroundAcceleration : Properties.AirAcceleration;
        float maxSpeedChange = accel * Time.deltaTime;

        // Represents the x and z components of our current velocity
        float currentX = Vector3.Dot(relativeVelocity, xAxis);
        float currentZ = Vector3.Dot(relativeVelocity, zAxis);
        
        // Represents the x and z components of what will be the players new velocity
        float newX = Mathf.MoveTowards(currentX, desiredVelocity.x, maxSpeedChange);
        float newZ = Mathf.MoveTowards(currentZ, desiredVelocity.z, maxSpeedChange);

        // Add the change in velocity that we calculated for both the x and z axis to our internal
        // representation of the velocity (it will be applied to the player by the end of this frame)
        velocity += xAxis * (newX - currentX) + zAxis * (newZ - currentZ);
    }

    private void Jump()
    {
        if (Properties.MaxAirJumps < 0)
        {
            return;
        }

        if (OnGround || usedJumps < Properties.MaxAirJumps)
        {
            // QOL for making double jumps work against gravity
            if (velocity.y < 0) velocity.y = 0;

            // Some physics equation or something for realistic jump speed?? It works but
            // we don't really care about scientific accuracy here, so maybe change later
            float jumpSpeed = Mathf.Sqrt(-2f * Physics.gravity.y * Properties.JumpHeight);

            // Represents the player's current speed in the y direction away from the surface they are standing on
            float alignedSpeed = Vector3.Dot(velocity, contactNormal);
            
            // If the player already has some upward velocity...
            if (alignedSpeed > 0f)
            {
                // ... decrease our jump speed such that it cannot exceed our desired speed
                jumpSpeed = Mathf.Max(jumpSpeed - alignedSpeed, 0f);
            }

            // Add our jump speed to our velocity, scaled away from our contact normal
            velocity += contactNormal * jumpSpeed;
            usedJumps++;
        }
    }

    private void UpdateState() 
    {
        // Update our cached local velocity variable to match the true rigidbody velocity
        velocity = body.velocity;
        
        if (OnGround)
        {
            // Refresh jumps
            usedJumps = 0;

            // Make sure our avarage contact normal is normalized. Since we added all our 
            // contact normals together, magnitude would be pretty big if we didn't do this.
            if (groundContactCount > 1) contactNormal.Normalize();
        }
        else
        {
            // If we are not standing on the floor, default the contact normal to point straight up.
            contactNormal = Vector3.up;
        }

        // If we are standing on another rigidbody...
        if (connectedBody) 
        {
            // ... and if the rigidbody should actually be able to move us...
            if (connectedBody.isKinematic || connectedBody.mass >= body.mass)
            { 
                // ... update our cached information about the rigidbody we are standing on.
                UpdateConnectionState();
            }
        }
    }

    private void UpdateConnectionState()
    {
        // Figure out the connected object's velocity from its change in position. 
        // Takes orbital velocity into account, allowing rotation to be tracked.

        if (connectedBody == previousConnectedBody)
        {
            Vector3 changeInPosition =
                connectedBody.transform.TransformPoint(connectionLocalPosition) - connectionWorldPosition;
            connectionVelocity = changeInPosition / Time.deltaTime;
        }

        connectionWorldPosition = body.position;
        connectionLocalPosition = connectedBody.transform.InverseTransformPoint(
            connectionWorldPosition
        );
    }

    private void ClearState()
    {
        groundContactCount = 0;
        contactNormal = connectionVelocity = Vector3.zero;
        previousConnectedBody = connectedBody;
        connectedBody = null;
    }

    private void EvaluateCollision(Collision collision)
    {
        // Loop through every point at which we collided during this physics tick
        for (int i = 0; i < collision.contactCount; i++)
        {
            Vector3 normal = collision.GetContact(i).normal;

            // Essentially, if the normal of our ground collision is upright
            // enough, count it as a contact with the ground

            if (normal.y >= minNormalY)
            {
                groundContactCount += 1;
                contactNormal += normal;
                connectedBody = collision.rigidbody;
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        EvaluateCollision(collision);
    }

    private void OnCollisionStay(Collision collision)
    {
        EvaluateCollision(collision);
    }

    private Vector3 ProjectOnContactPlane(Vector3 vector)
    {
        // Returns the input vector, but it is adjusted to follow the slope of the 
        // floor that the player is standing on.
        Vector3 result = vector - (contactNormal * Vector3.Dot(vector, contactNormal));

        // Uncommenting the line below can help visualise whats happening (make sure gizmos are on!)
        /// Debug.DrawLine(transform.position, transform.position + result, Color.blue);

        return result;
    }

    public Vector3 getVelocity()
    {
        return body.velocity;
    }

    public Vector3 getPosition()
    {
        return transform.position;
    }

    public void DetachController()
    {
        Controller = null;
        desiredVelocity = Vector3.zero;
    }

    public void AttachController(Controller controller)
    {
        Controller = controller;
    }
}
