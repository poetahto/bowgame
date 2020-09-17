﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestDoor : Chargeable
{
    [Header("Door Settings")]
    [SerializeField] private float moveSpeed = 1f;
    [SerializeField] private float openDistance = 10f;
    [SerializeField] private int chargesNeededToOpen = 1;
    [SerializeField] private Transform targetPosition = null;
    [SerializeField] private Transform visualizer = null;

    private Vector3 visualizerOriginalScale = Vector3.one;
    private Vector3 openPosition;
    private Vector3 closedPosition;
    private Rigidbody rb;
    private bool obstructed => obstructor != null;
    private Collider obstructor = null;

    private DoorState state;

    private Coroutine curCoroutine = null;

    private void Awake()
    {
        if (visualizer != null) visualizerOriginalScale = visualizer.localScale;
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

    public override void AddCharge(Charge charge)
    {
        base.AddCharge(charge);

        if (visualizer != null)
        {
            Vector3 targetScale = visualizerOriginalScale * (1f - Mathf.Min((float)charges.Count / (float)chargesNeededToOpen, 1f));
            targetScale.z = visualizerOriginalScale.z;
            ChangeSize(targetScale);
        }
    }

    public override void RemoveCharge(Charge charge)
    {
        base.RemoveCharge(charge);

        if (visualizer != null)
        {
            Vector3 targetScale = visualizerOriginalScale * (1f - Mathf.Min((float) charges.Count / (float) chargesNeededToOpen, 1f));
            targetScale.z = visualizerOriginalScale.z;
            ChangeSize(targetScale);
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
        if (col.GetContact(0).normal.y > 0.9f)
        { 
            obstructor = col.collider;
        }
    }

    private void OnCollisionExit(Collision col)
    {
        obstructor = null;
    }

    private void ChangeSize(Vector3 targetScale)
    {
        LeanTween.cancel(visualizer.gameObject);
        LeanTween.scaleX(visualizer.gameObject, targetScale.x, 0.15f);
        LeanTween.scaleY(visualizer.gameObject, targetScale.y, 0.15f);
        LeanTween.scaleZ(visualizer.gameObject, targetScale.z, 0.15f);
    }

    private enum DoorState 
    {
        Closed,
        Open,
        Closing,
        Opening
    }
}
