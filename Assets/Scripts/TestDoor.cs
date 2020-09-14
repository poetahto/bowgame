using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestDoor : MonoBehaviour, ShootableObject
{
    public bool open = false;
    private float percentComplete = 0f;
    public float openDistance = 10f;

    public void OnHit(Arrow arrow)
    {
        StartCoroutine(OpenDoor());
    }

    private IEnumerator OpenDoor()
    {
        percentComplete = 0f;
        Vector3 initial = transform.position;
        float direction = open ? -openDistance : openDistance;
        Vector3 target = initial + new Vector3(0f, direction, 0f);


        while (transform.position != target)
        {
            percentComplete += Time.deltaTime;
            transform.position = Vector3.Lerp(initial, target, percentComplete);
            yield return new WaitForEndOfFrame();
        }

        open = !open;
    }
}
