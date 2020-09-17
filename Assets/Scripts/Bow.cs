using System.Collections;
using System.Collections.Generic;
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

    public int speedLevel0 = 4;
    public int speedLevel1 = 16;
    public int speedLevel2 = 24;
    public int speedLevel3 = 42;

    public float speedLevel1Mills = 0.3f;
    public float speedLevel2Mills = 0.6f;
    public float speedLevel3Mills = 1.2f;

    public Color colorLevel0;
    public Color colorLevel1;
    public Color colorLevel2;
    public Color colorLevel3;

    private Color[] colors;
    private int[] arrowSpeeds;

    public int MaxArrows { get; set; }
    public int CurrentArrows { get; set; }
    public short ChargeLevel { get; set; }

    private List<GameObject> arrows;
    private LTDescr chargingAnimation;
    private GameObject arrowGroup = null;
    private Vector3 originalBarScale;

    private bool charging = false;

    private float chargeStart = -1;

    void Awake()
    {
        colors = new Color[] { colorLevel0, colorLevel1, colorLevel2, colorLevel3 };
        arrowSpeeds = new int[] { speedLevel0, speedLevel1, speedLevel2, speedLevel3 };

        arrowGroup = new GameObject("Arrow Group");
        arrows = new List<GameObject>();
        ChargeLevel = 0;
        MaxArrows = maxArrows;
        CurrentArrows = MaxArrows;
        chargeBar.color = colorLevel0;
        originalBarScale = chargeBar.transform.localScale;
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

    void Update()
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
        
        arrowRigidbody.velocity = (cameraTransform.forward.normalized * arrowSpeeds[ChargeLevel]);
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
                5,   // our target scale
                speedLevel1Mills + speedLevel2Mills + speedLevel3Mills); // how long it will take to scale

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
            Destroy(arrow);
        }

        // Refund all collected arrows
        CurrentArrows = MaxArrows;
    }

    private void UpdateChargeLevel()
    {
        float chargeTime = Time.time - chargeStart;

        if (chargeTime > speedLevel3Mills)
        {
            ChargeLevel = 3;
        }
        else if (chargeTime > speedLevel2Mills)
        {
            ChargeLevel = 2;
        }
        else if (chargeTime > speedLevel1Mills)
        {
            ChargeLevel = 1;
        }
        else
        {
            ChargeLevel = 0;
        }
    }

    private void UpdateChargeBar()
    {
        chargeBar.color = colors[ChargeLevel];
    }
}
