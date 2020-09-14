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

    public Color[] colors;
    public int[] arrowSpeeds;

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
        MaxArrows = 10;
        CurrentArrows = MaxArrows;
        chargeBar.color = colorLevel0;
        originalBarScale = chargeBar.transform.localScale;
    }

    void Update()
    {
        UpdateChargeLevel();

        UpdateChargeBar();

        if (Input.GetButtonDown("Fire1"))
        {
            StartCharging();
        }

        if (Input.GetButtonUp("Fire1"))
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

        int speed = arrowSpeeds[ChargeLevel];
        var arrow = Instantiate(arrowPrefab);
        var arrowRigidbody = arrow.GetComponent<Rigidbody>();

        chargeStart = -1; //DEBUG

        arrow.transform.SetParent(arrowGroup.transform);
        arrow.transform.position = characterTransform.position + (transform.forward.normalized * 0.5f) + (transform.up * 0.5f);
        arrow.transform.rotation = cameraTransform.rotation;
        arrowRigidbody.velocity = (cameraTransform.forward.normalized * speed);

        arrows.Add(arrow);
    }

    private void StartCharging()
    {
        chargeStart = Time.time;

        if (!charging)
        {
            chargingAnimation = LeanTween.scaleX(
                chargeBar.gameObject,   // the object we are scaling
                5,   // our target scale
                speedLevel1Mills + speedLevel2Mills + speedLevel3Mills); // how long it will take to scale

            charging = true;
        }
    }

    private void StopCharging()
    {
        // stop the charging animation if it hasn't already finished
        LeanTween.cancel(chargingAnimation.id);
        var newScale = originalBarScale;
        newScale.x = 0f;
        chargeBar.transform.localScale = newScale;
        chargeBar.color = colors[0];
        charging = false;
    }

    private void CollectArrows()
    {
        foreach (GameObject arrow in arrows)
        {
            Destroy(arrow);
        }
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
