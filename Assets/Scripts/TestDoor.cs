using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestDoor : Chargeable
{
    public float openDistance = 10f;
    public bool ignoreArrows = false;
    public int chargesNeededToOpen = 1;

    private Vector3 openPosition;
    private Vector3 closedPosition;

    private DoorState state;

    private List<Charge> charges;

    private Coroutine curCoroutine = null;

    private void Awake()
    {
        charges = new List<Charge>();
        closedPosition = transform.position;
        openPosition = closedPosition + new Vector3(0f, openDistance, 0f);

    }

    private void Update()
    {
        Debug.DrawLine(openPosition, closedPosition, Color.red);

        if (charges.Count >= chargesNeededToOpen && state != DoorState.OPEN && state != DoorState.OPENING)
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

    public override void AddCharge(Charge charge)
    {
        if (!ignoreArrows)
        { 
            charges.Add(charge);
        }
        else if (!(charge is Arrow))
        {
            charges.Add(charge);
        }
    }

    public override void RemoveCharge(Charge charge)
    {
        charges.Remove(charge);
    }

    private enum DoorState 
    {
        CLOSED,
        OPEN,
        CLOSING,
        OPENING
    }
}
