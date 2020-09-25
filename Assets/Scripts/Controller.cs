using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// represents the player who is controlling the characters in the game

[RequireComponent(typeof(Camera))]
public class Controller : MonoBehaviour
{
    [Range(1f, 10f)] public float mouseSensitivity = 3f;

    public static Controller instance;
    public ControllableObject currentlyControlling = null;
    private float cameraMoveSpeed = 0.01f;

    private void Awake()
    {
        instance = this;

        if (currentlyControlling != null)
        {
            AttachTo(currentlyControlling);
        }
    }

    private void Update()
    {
        HandleMouseInput();
        UpdateCameraPos();
    }

    public void AttachTo(ControllableObject obj)
    {
        if (obj != null)
        {
            cameraMoveSpeed = 0.25f;
            Invoke("ResetSpeed", 0.5f);
            currentlyControlling.DetachController();
            currentlyControlling = obj;
            obj.AttachController(this);
        }
    }

    public void ResetSpeed()
    {
        cameraMoveSpeed = 0.01f;
    }

    private void UpdateCameraPos()
    {
        // Updates the camera's position every frame to smoothly follow the player around.

        Vector3 cameraPos = transform.position;

        float velocityChange = Mathf.Max(currentlyControlling.getVelocity().magnitude * Time.deltaTime, cameraMoveSpeed);

        cameraPos.x = Mathf.MoveTowards(
            cameraPos.x, 
            currentlyControlling.getPosition().x + currentlyControlling.Properties.CameraOffset.x, 
            velocityChange);
        
        cameraPos.y = Mathf.MoveTowards(
            cameraPos.y, 
            currentlyControlling.getPosition().y + currentlyControlling.Properties.CameraOffset.y, 
            velocityChange);
        
        cameraPos.z = Mathf.MoveTowards(
            cameraPos.z, 
            currentlyControlling.getPosition().z + currentlyControlling.Properties.CameraOffset.z, 
            velocityChange);

        transform.position = cameraPos;
    }
    private void HandleMouseInput()
    {
        // calculate new rotation from mouse input and sensitivity
        float newRotationY = transform.localEulerAngles.y + Input.GetAxis("Mouse X") * mouseSensitivity;
        float newRotationX = transform.localEulerAngles.x - Input.GetAxis("Mouse Y") * mouseSensitivity;

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
        currentlyControlling.transform.localEulerAngles = new Vector3(0f, newRotationY, 0f);
        transform.localEulerAngles = new Vector3(newRotationX, newRotationY, 0f);
    }
}
