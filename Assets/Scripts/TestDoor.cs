using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestDoor : Chargeable
{
    [Header("Door Settings")]
    [SerializeField] private float moveSpeed = 1f;
    [SerializeField] private float openDistance = 10f;
    [SerializeField] private int chargesNeededToOpen = 1;
    [SerializeField] private Transform targetPosition = null;

    private Vector3 openPosition;
    private Vector3 closedPosition;
    private Rigidbody rb;
    private bool obstructed = false;

    private DoorState state;

    private Coroutine curCoroutine = null;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        closedPosition = transform.position;
        openPosition = targetPosition == null ? closedPosition + new Vector3(0f, openDistance, 0f) : targetPosition.position;
    }

    private void Update()
    {
        Color lineColor = Color.clear;
        if (state == DoorState.Opening) lineColor = Color.green;
        if (state == DoorState.Closing) lineColor = Color.red;

        Debug.DrawLine(openPosition, closedPosition, lineColor);

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

    private IEnumerator OpenDoor()
    {
        state = DoorState.Opening;
        
        Vector3 target = openPosition;

        while (transform.position != target)
        {
            if (!obstructed)
            {
                rb.MovePosition(Vector3.MoveTowards(transform.position, target, Time.fixedDeltaTime * moveSpeed));
            }

            yield return new WaitForEndOfFrame();
        }

        state = DoorState.Open;
    }

    private IEnumerator CloseDoor()
    {
        state = DoorState.Closing;

        Vector3 target = closedPosition;

        while (transform.position != target)
        {
            if (!obstructed)
            { 
                rb.MovePosition(Vector3.MoveTowards(transform.position, target, Time.fixedDeltaTime * moveSpeed));
            }
            yield return new WaitForEndOfFrame();
        }

        state = DoorState.Closed;
    }

    private void OnCollisionEnter(Collision col)
    {
        if (col.gameObject.tag == "Player" && col.GetContact(0).normal.y > 0.9f)
        { 
            obstructed = true;
        }
    }

    private void OnCollisionExit(Collision col)
    {
        if (col.gameObject.tag == "Player")
        {
            obstructed = false;
        }
    }

    private enum DoorState 
    {
        Closed,
        Open,
        Closing,
        Opening
    }
}
