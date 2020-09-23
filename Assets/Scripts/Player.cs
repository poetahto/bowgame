using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Player : MonoBehaviour
{
    [Header("GameObject References")]

    [SerializeField] private Transform cameraTransform = null;
    [SerializeField] private Transform characterTransform = null;

    [Header("Movement Properties")]

    //[Range(1f, 10f)]
    [SerializeField] private float jumpHeight = 2f;

    //[Range(1f, 10f)]
    [SerializeField] private float mouseSensitivity = 3f;

    //[Range(1f, 100f)]
    [SerializeField] private float maxSpeed = 4f;

    //[Range(1f, 100f)]
    [SerializeField] private float acceleration = 26f, airAcceleration = 1f;

    [Range(0f, 90f)]
    [SerializeField] private float maxGroundAngle = 10f;

    [Range(0, 5)]
    [SerializeField] private float maxAirJumps = 0;

    private bool OnGround => groundContactCount > 0;
    private bool desiredJump;
    private int jumpPhase;
    private int groundContactCount;
    private float minGroundDotProduct;
    private Rigidbody body, connectedBody, previousConnectedBody;
    private Vector3 velocity, desiredVelocity, connectionVelocity;
    private Vector3 contactNormal;
    private Vector3 connectionWorldPosition, connectionLocalPosition;


    void Awake()
    {
        body = GetComponent<Rigidbody>();
        OnValidate();

        // TODO move this code to a more intuitive location with more initialization stuff
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        Application.targetFrameRate = -1;
        QualitySettings.vSyncCount = 0;
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
        //TODO move the spawning and destroying of arrows into a factory class
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            GameManager.RestartLevel(Scene.IntroLevel);
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
        desiredVelocity = (transform.right * playerInput.x + transform.forward * playerInput.y) * maxSpeed;
    }

    private void UpdateVelocity()
    {
        UpdateState();

        AdjustVelocity();

        if (desiredJump)
        {
            desiredJump = false;
            Jump();
        }

        // change our player's position according to their velocity
        body.velocity = velocity;
        ClearState();
    }

    private void Jump()
    {
        if (OnGround || jumpPhase < maxAirJumps)
        {
            if (velocity.y < 0)
            {
                velocity.y = 0;
            }
            jumpPhase++;
            float jumpSpeed = Mathf.Sqrt(-2f * Physics.gravity.y * jumpHeight);
            float alignedSpeed = Vector3.Dot(velocity, contactNormal);
            if (alignedSpeed > 0f)
            {
                jumpSpeed = Mathf.Max(jumpSpeed - alignedSpeed, 0f);
            }
            velocity += contactNormal * jumpSpeed;
        }
    }

    private void UpdateCameraPos()
    {
        Vector3 cameraPos = cameraTransform.position;
        float velocityChange = velocity.magnitude * Time.deltaTime;

        // make the camera follow the players body around
        cameraPos.x = Mathf.MoveTowards(cameraPos.x, characterTransform.position.x, velocityChange);
        cameraPos.y = Mathf.MoveTowards(cameraPos.y, characterTransform.position.y + 0.5f, velocityChange);
        cameraPos.z = Mathf.MoveTowards(cameraPos.z, characterTransform.position.z, velocityChange);

        cameraTransform.position = cameraPos;
    }

    private void OnCollisionEnter(Collision collision)
    {
        EvaluateCollision(collision);
    }

    private void OnCollisionStay(Collision collision)
    {
        EvaluateCollision(collision);
    }

    private void EvaluateCollision(Collision collision)
    { 
        for (int i = 0; i < collision.contactCount; i++)
        {
            Vector3 normal = collision.GetContact(i).normal;
            if (normal.y >= minGroundDotProduct)
            {
                groundContactCount += 1;
                contactNormal += normal;
                connectedBody = collision.rigidbody;
            }
        }
    }

    private void UpdateState() 
    {
        velocity = body.velocity;
        if (OnGround)
        {
            jumpPhase = 0;
            if (groundContactCount > 1)
            { 
                contactNormal.Normalize();
            }
        }
        else
        {
            contactNormal = Vector3.up;
        }

        if (connectedBody) 
        {
            if (connectedBody.isKinematic || connectedBody.mass >= body.mass)
            { 
                UpdateConnectionState();
            }
        }
    }

    private void UpdateConnectionState()
    {
        if (connectedBody == previousConnectedBody)
        {
            Vector3 connectionMovement =
            connectedBody.transform.TransformPoint(connectionLocalPosition) - connectionWorldPosition;
            connectionVelocity = connectionMovement / Time.deltaTime;
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

    private void OnValidate()
    {
        minGroundDotProduct = Mathf.Cos(maxGroundAngle* Mathf.Deg2Rad);
    }

    private Vector3 ProjectOnContactPlane(Vector3 vector)
    {
        return vector - contactNormal * Vector3.Dot(vector, contactNormal);
    }

    private void AdjustVelocity()
    {
        Vector3 xAxis = ProjectOnContactPlane(Vector3.right).normalized;
        Vector3 zAxis = ProjectOnContactPlane(Vector3.forward).normalized;

        Vector3 relativeVelocity = velocity - connectionVelocity;
        float currentX = Vector3.Dot(relativeVelocity, xAxis);
        float currentZ = Vector3.Dot(relativeVelocity, zAxis);

        float accel = OnGround ? acceleration : airAcceleration;
        float maxSpeedChange = accel * Time.deltaTime;

        float newX = Mathf.MoveTowards(currentX, desiredVelocity.x, maxSpeedChange);
        float newZ = Mathf.MoveTowards(currentZ, desiredVelocity.z, maxSpeedChange);

        velocity += xAxis * (newX - currentX) + zAxis * (newZ - currentZ);
    }
}
