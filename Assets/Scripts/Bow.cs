using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

public class Bow : MonoBehaviour
{
    [Header("GameObject References")]

    [SerializeField] private Transform cameraTransform = null;
    [SerializeField] private Transform characterTransform = null;
    [SerializeField] private GameObject arrowPrefab = null;
    [SerializeField] private RawImage chargeBar = null;
    [SerializeField] private int maxArrows = 6;

    [Header("Bow Properties")]

    public Color[] colors = new Color[] { Color.green, Color.yellow, Color.red, Color.magenta};
    public int[] arrowSpeeds = new int[] { 2, 8, 12, 21 };
    public float[] chargeTimes = new float[] { 0f, 0.3f, 0.6f, 1.2f };

    public int MaxArrows { get { return maxArrows; } set { maxArrows = value;  } }
    public int CurrentArrows { get; set; }
    public int ChargeLevel { get; set; }

    private List<GameObject> arrows;
    private LTDescr chargingAnimation;
    private GameObject arrowGroup = null;
    private Vector3 originalBarScale;

    private bool charging = false;
    private float totalChargeTime = 0;
    private float chargeStart = -1;

    void Awake()
    {
        foreach (float time in chargeTimes)
        {
            totalChargeTime += time;
        }

        arrowGroup = new GameObject("Arrow Group");
        arrows = new List<GameObject>();
        ChargeLevel = 0;
        MaxArrows = maxArrows;
        CurrentArrows = MaxArrows;
        chargeBar.color = colors[0];
        originalBarScale = chargeBar.transform.localScale;
    }

    void Update()
    {
        if (Controller.instance.currentlyControlling.GetComponent<Bow>() != null)
        {
            UpdateChargeLevel();

            UpdateChargeBar();

            if (Input.GetButtonDown("Fire1") && CurrentArrows > 0)
            {
                StartCharging();
            }

            if (Input.GetButtonUp("Fire1") && CurrentArrows > 0)
            {
                ReleaseArrow();
            }
        }

        if (Input.GetButtonDown("Fire2"))
        {
            CollectArrows();
        }
    }

    private void ReleaseArrow()
    {
        StopCharging();

        // initialize arrow and its velocity, position, rotation, and parent
        // TODO change to an object pooling method later?!
        GameObject arrow = Instantiate(arrowPrefab);
        Rigidbody arrowRigidbody = arrow.GetComponent<Rigidbody>();
        arrowRigidbody.velocity = (cameraTransform.forward.normalized * (arrowSpeeds[ChargeLevel]));
        arrow.transform.position = characterTransform.position + (transform.forward.normalized * 0.5f) + (transform.up * 0.5f);
        arrow.transform.rotation = cameraTransform.rotation;
        arrow.transform.SetParent(arrowGroup.transform);

        // cache the arrow we just fired to a list, so we can destroy it later
        arrows.Add(arrow);

        // Subtract the arrow we just shot from our total
        CurrentArrows--;
    }

    private void StartCharging()
    {
        if (!charging)
        {
            charging = true;

            chargingAnimation = LeanTween.scaleX(
                chargeBar.gameObject,   // the object we are scaling
                5,                      // our target scale
                totalChargeTime);       // how long it will take to scale

            chargeStart = Time.time;
        }
    }

    private void StopCharging()
    {
        if (charging)
        {
            charging = false;

            // stop the charging animation if it hasn't already finished
            LeanTween.cancel(chargingAnimation.id);

            // set the scale to 0
            var newScale = originalBarScale;
            newScale.x = 0f;
            chargeBar.transform.localScale = newScale;

            // reset the color to its default
            chargeBar.color = colors[0];
        }
    }

    public void CollectArrows()
    {
        // destroy every arrow we've shot
        foreach (GameObject arrow in arrows)
        {
            LeanTween.scale(arrow, Vector3.zero, 0.3f).setEaseInOutQuad().setOnComplete(()=> {
                arrows.Remove(arrow);
                Destroy(arrow);
            });
        }

        // Refund all collected arrows
        CurrentArrows = MaxArrows;
    }

    private void UpdateChargeLevel()
    {
        float chargeTime = Time.time - chargeStart;

        for (int i = 0; i < chargeTimes.Length; i++)
        {
            if (chargeTime > chargeTimes[i])
            {
                ChargeLevel = i;
            }
        }
    }

    private void UpdateChargeBar()
    {
        chargeBar.color = colors[ChargeLevel];
    }

    private void OnGUI()
    {
        // prints out the number of remaining arrows: replace with gui element later
        int w = Screen.width, h = Screen.height;

        GUIStyle style = new GUIStyle();

        Rect rect = new Rect(0, 0, w, h * 2 / 100);
        style.alignment = TextAnchor.UpperLeft;
        style.fontSize = h * 2 / 100;
        style.normal.textColor = Color.white;
        string text = "Remaining arrows: " + CurrentArrows;
        GUI.Label(rect, text, style);
    }
}
