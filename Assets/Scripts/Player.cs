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

    [Range(0, 5)]
    [SerializeField] private float maxAirJumps = 0;

    private int jumpPhase;
    private bool onGround;
    private bool desiredJump;
    private Vector3 desiredVelocity;
    private Vector3 velocity;
    private Rigidbody body;

    void Awake()
    {
        body = GetComponent<Rigidbody>();

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

        // represents the most we will be able to change our velocity during this tick
        float accel = onGround ? acceleration : airAcceleration;
        float velocityChange = accel * Time.deltaTime;

        // increment the velocity towards our desired velocity - acceleration effect
        velocity.x = Mathf.MoveTowards(velocity.x, desiredVelocity.x, velocityChange);
        velocity.z = Mathf.MoveTowards(velocity.z, desiredVelocity.z, velocityChange);

        if (desiredJump)
        {
            desiredJump = false;
            Jump();
        }

        // change our player's position according to their velocity
        body.velocity = velocity;
        onGround = false;
    }

    private void Jump()
    {
        if (onGround || jumpPhase < maxAirJumps)
        {
            if (velocity.y < 0)
            {
                velocity.y = 0;
            }
            jumpPhase++;
            float jumpSpeed = Mathf.Sqrt(-2f * Physics.gravity.y * jumpHeight);
            if (velocity.y > 0f)
            {
                jumpSpeed = Mathf.Max(jumpSpeed - velocity.y, 0f);
            }
            velocity.y += jumpSpeed;
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
            onGround |= normal.y >= 0.9f;
        }
    }

    private void UpdateState() 
    {
        velocity = body.velocity;
        if (onGround)
        {
            jumpPhase = 0;
        }
    }
}
