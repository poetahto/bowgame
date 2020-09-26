using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Currently, the easiest way to restrict player progress.
// Will move between two positions, defined by a height above its local position 
// or by another Transform's position.

// TODO improve pathing options, door will crush players when opening upwards

[ExecuteInEditMode]
public class Door : ChargeableObject
{
    [Header("Door Settings")]
    [SerializeField] private float moveSpeed = 1f;
    [SerializeField] private float openDistance = 10f;
    [SerializeField] private int chargesNeededToOpen = 1;
    [SerializeField] private Transform targetPosition = null;
    [SerializeField] private Transform visualizer = null;

    private bool Obstructed => obstructor != null;
    private Collider obstructor;
    private Vector3 visualizerOriginalScale;
    private Vector3 openPosition, closedPosition;
    private Rigidbody rb;
    private DoorState state;

    // Why use corourtines instead of an animation controller for handling the 
    // door-movement process? Because I hate animation controllers.
    private Coroutine curCoroutine;

    private void Awake()
    {
        // Save the visualizer's original scale to help with the scaling process
        if (visualizer != null) visualizerOriginalScale = visualizer.localScale;

        rb = GetComponent<Rigidbody>();
        
        closedPosition = transform.position;
        openPosition = targetPosition == null ? closedPosition + new Vector3(0f, openDistance, 0f) : targetPosition.position;
    }

    private void OnValidate()
    {
        closedPosition = transform.position;
        openPosition = targetPosition == null ? closedPosition + new Vector3(0f, openDistance, 0f) : targetPosition.position;
    }

    private void Update()
    {
        /// --- DEBUG STUFF ---
        Color lineColor = Color.white;

        if (state == DoorState.Opening) lineColor = Color.green;
        if (state == DoorState.Closing) lineColor = Color.red;

        Debug.DrawLine(openPosition, closedPosition, lineColor);
        /// --- DEBUG STUFF ---

        // Update the door's state based on it's current charges.
        if (charges.Count >= chargesNeededToOpen && state != DoorState.Open && state != DoorState.Opening)
        {
            if (curCoroutine != null) StopCoroutine(curCoroutine);
            curCoroutine = StartCoroutine(OpenDoor());
        }
        else if (charges.Count <= 0 && state != DoorState.Closed && state != DoorState.Closing)
        {
            if (curCoroutine != null) StopCoroutine(curCoroutine);
            curCoroutine = StartCoroutine(CloseDoor());
        }
    }

    public override void AddCharge(Charge charge)
    {
        base.AddCharge(charge);
        UpdateVisualizerScale();
    }

    public override void RemoveCharge(Charge charge)
    {
        base.RemoveCharge(charge);
        UpdateVisualizerScale();
    }

    private IEnumerator OpenDoor()
    {
        state = DoorState.Opening;
        
        yield return MoveDoorToPosition(openPosition);

        state = DoorState.Open;
    }

    private IEnumerator CloseDoor()
    {
        state = DoorState.Closing;

        yield return MoveDoorToPosition(closedPosition);

        state = DoorState.Closed;
    }

    private IEnumerator MoveDoorToPosition(Vector3 target)
    {
        while (transform.position != target)
        {
            if (!Obstructed)
            {
                rb.MovePosition(
                    Vector3.MoveTowards(transform.position, target, Time.fixedDeltaTime * moveSpeed));
            }
            yield return new WaitForEndOfFrame();
        }
    }

    private void UpdateVisualizerScale()
    {
        // The target scale of the visualizer is inversely proportional to the percent-unlocked
        // the door is. In other words, more charges -> smaller size of the visualizer

        if (visualizer != null)
        {
            Vector3 targetScale = visualizerOriginalScale * 
                (1f - Mathf.Min((float) charges.Count / chargesNeededToOpen, 1f));

            AnimationUtil.ChangeSize(visualizer.gameObject, 
                targetScale.x, targetScale.y, visualizerOriginalScale.z, 0.15f);
        }
    }

    private void OnCollisionEnter(Collision col)
    {
        if (col.GetContact(0).normal.y > 0.9f)
        { 
            obstructor = col.collider;
        }
    }

    private void OnCollisionExit(Collision col)
    {
        obstructor = null;
    }

    private enum DoorState 
    {
        Closed,
        Open,
        Closing,
        Opening
    }
}
