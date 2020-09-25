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
    [Header("GameObject References")]

    [SerializeField] private Transform cameraTransform = null;
    [SerializeField] private Transform characterTransform = null;

    [Header("Movement Properties")]

    [Range(1f, 10f)]
    [SerializeField] private float mouseSensitivity = 3f;

    [Range(0f, 90f)]
    [SerializeField] private float maxGroundAngle = 10f;

    public abstract ControllableProperties Properties { get; }

    private bool OnGround => groundContactCount > 0;
    private bool desiredJump;
    private float minNormalY;
    private int usedJumps;
    private int groundContactCount;
    private Rigidbody body, connectedBody, previousConnectedBody;
    private Vector3 velocity, desiredVelocity, connectionVelocity;
    private Vector3 connectionWorldPosition, connectionLocalPosition;
    private Vector3 contactNormal;

    void Awake()
    {
        body = GetComponent<Rigidbody>();

        // Calculate the minNormalY everytime we run the game, both in editor and build
        OnValidate();
    }

    void Update()
    {
        /*
            process keyboard inputs that are only for testing
        */
        HandleDebugInput();

        /*
            uses mouse input to update the rotations of the player rigidbody
            and the player camera
        */
        HandleMouseInput();

        /*
            uses keyboard input to update our desiredVelocity 
            (doesn't actually move player)
        */
        HandleKeyboardInput();

        /*
            allows camera to smoothly follow the character since fixed physics 
            updates cause the rigidbody to stutter as it moves
        */
        UpdateCameraPos();
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
            GameManager.LoadLevel(Scene.IntroLevel);
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            Time.timeScale = Time.timeScale == 0.25f ? 1f : 0.25f;
        }
    }

    private void HandleMouseInput()
    {
        // calculate new rotation from mouse input and sensitivity
        float newRotationY = characterTransform.localEulerAngles.y + Input.GetAxis("Mouse X") * mouseSensitivity;
        float newRotationX = cameraTransform.localEulerAngles.x - Input.GetAxis("Mouse Y") * mouseSensitivity;

        // clamp X rotation to between 0-90 and 270-360 because euler angles wrap at 0
        // TODO is there a better way to do this?
        if (newRotationX > 90 && newRotationX < 270)
        {
            if (newRotationX < 180)
            {
                newRotationX = 90;
            }
            else if (newRotationX > 180)
            {
                newRotationX = 270;
            }
        }

        // update local rotation with the values we calculated, no z because we dont want to pitch or roll
        characterTransform.localEulerAngles = new Vector3(0f, newRotationY, 0f);
        cameraTransform.localEulerAngles = new Vector3(newRotationX, newRotationY, 0f);
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

    private void UpdateCameraPos()
    {
        // Updates the camera's position every frame to smoothly follow the player around.
        // I did this because standard parenting of the camera to the player caused jerky 
        // movements, and increasing the physics tick rate really slows stuff down.

        Vector3 cameraPos = cameraTransform.position;

        float velocityChange = Mathf.Max(velocity.magnitude * Time.deltaTime, 0.01f);

        cameraPos.x = Mathf.MoveTowards(cameraPos.x, characterTransform.position.x, velocityChange);
        cameraPos.y = Mathf.MoveTowards(cameraPos.y, characterTransform.position.y + 0.5f, velocityChange);
        cameraPos.z = Mathf.MoveTowards(cameraPos.z, characterTransform.position.z, velocityChange);

        cameraTransform.position = cameraPos;
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

    private void OnValidate()
    {
        // When we check our collision with the ground, the Y value of the normal must
        // be greater than this value for it to count as ground (refreshing jump, ect).
        // The reason this works is dot product, 1 * 1 * cos(ground angle).

        minNormalY = Mathf.Cos(maxGroundAngle * Mathf.Deg2Rad);
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
}
