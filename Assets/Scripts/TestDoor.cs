using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestDoor : Chargeable
{
    [Header("Door Settings")]
    [SerializeField] private float moveSpeed = 1f;
    [SerializeField] private float openDistance = 10f;
    [SerializeField] private int chargesNeededToOpen = 1;

    private Vector3 openPosition;
    private Vector3 closedPosition;

    private DoorState state;

    private Coroutine curCoroutine = null;

    private void Awake()
    {
        closedPosition = transform.position;
        openPosition = closedPosition + new Vector3(0f, openDistance, 0f);
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
            transform.position = Vector3.MoveTowards(transform.position, target, Time.deltaTime * moveSpeed);
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
            transform.position = Vector3.MoveTowards(transform.position, target, Time.deltaTime * moveSpeed);
            yield return new WaitForEndOfFrame();
        }

        state = DoorState.Closed;
    }

    private enum DoorState 
    {
        Closed,
        Open,
        Closing,
        Opening
    }
}
