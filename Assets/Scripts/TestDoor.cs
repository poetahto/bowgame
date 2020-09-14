using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestDoor : MonoBehaviour, Chargeable
{
    public float openDistance = 10f;

    private Vector3 openPosition;
    private Vector3 closedPosition;

    private DoorState state;

    private List<Arrow> charges;

    private Coroutine curCoroutine = null;

    private void Start()
    {
        charges = new List<Arrow>();
        closedPosition = transform.position;
        openPosition = closedPosition + new Vector3(0f, openDistance, 0f);
    }

    private void Update()
    {
        Debug.DrawLine(openPosition, closedPosition, Color.red);

        if (charges.Count > 1 && state != DoorState.OPEN && state != DoorState.OPENING)
        {
            if (curCoroutine != null) StopCoroutine(curCoroutine);
            curCoroutine = StartCoroutine(OpenDoor());
        }
        else if (charges.Count <= 0 && state != DoorState.CLOSED && state != DoorState.CLOSING)
        {
            if (curCoroutine != null) StopCoroutine(curCoroutine);
            curCoroutine = StartCoroutine(CloseDoor());
        }
    }

    private IEnumerator OpenDoor()
    {
        var percentComplete = 0f;
        Vector3 target = openPosition;

        state = DoorState.OPENING;

        while (transform.position != target)
        {
            percentComplete += Time.deltaTime;
            transform.position = Vector3.MoveTowards(transform.position, target, Time.deltaTime);
            yield return new WaitForEndOfFrame();
        }

        state = DoorState.OPEN;
    }

    private IEnumerator CloseDoor()
    {
        var percentComplete = 0f;
        Vector3 target = closedPosition;

        state = DoorState.CLOSING;

        while (transform.position != target)
        {
            percentComplete += Time.deltaTime;
            transform.position = Vector3.MoveTowards(transform.position, target, Time.deltaTime);
            yield return new WaitForEndOfFrame();
        }

        state = DoorState.CLOSED;
    }

    public void AddCharge(Arrow arrow)
    {
        charges.Add(arrow);
    }

    public void RemoveCharge(Arrow arrow)
    {
        charges.Remove(arrow);
    }

    public void OnHit() { }

    private enum DoorState 
    {
        CLOSED,
        OPEN,
        CLOSING,
        OPENING
    }
}
