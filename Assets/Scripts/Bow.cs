using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Bow : MonoBehaviour
{
    [Header("GameObject References")]
    [SerializeField] private Transform arrowGroup = null;
    [SerializeField] private Transform cameraTransform = null;
    [SerializeField] private Transform characterTransform = null;
    [SerializeField] private GameObject arrowPrefab = null;
    [SerializeField] private RawImage chargeBar = null;

    [Header("Bow Properties")]
    public int speedLevel0 = 4;
    public int speedLevel1 = 16;
    public int speedLevel2 = 24;
    public int speedLevel3 = 42;

    public int speedLevel1Mills = 300;
    public int speedLevel2Mills = 600;
    public int speedLevel3Mills = 1200;

    public Color colorLevel0;
    public Color colorLevel1;
    public Color colorLevel2;
    public Color colorLevel3;

    private List<GameObject> arrows;
    private LTDescr anim;

    private float chargeStart = -1;

    void Start()
    {
        chargeBar.color = colorLevel0;
    }

    void Awake()
    {
        arrows = new List<GameObject>();
    }

    void Update()
    {
        if (chargeStart > 0) 
        {
           float chargeTimed = Time.time * 1000 - chargeStart * 1000;
            if (chargeTimed > speedLevel3Mills)
            {
                chargeBar.color = colorLevel3;
            }
            else if (chargeTimed > speedLevel2Mills)
            {
                chargeBar.color = colorLevel2;
            }
            else if (chargeTimed > speedLevel1Mills)
            {
                chargeBar.color = colorLevel1;
            }
        }

        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            chargeStart = Time.time;
            if (anim == null)
            {
                anim = LeanTween.scaleX(chargeBar.gameObject, 5f, (speedLevel1Mills + speedLevel2Mills + speedLevel3Mills) / 1000);
            }
        }

        if (Input.GetKeyUp(KeyCode.Mouse0))
        {
            LeanTween.cancel(anim.id);
            chargeBar.transform.localScale = new Vector3(0, 2f, 1f);
            chargeBar.color = colorLevel0;
            anim = null;

            float chargeTime = Time.time * 1000 - chargeStart * 1000;
            int speed = speedLevel0;
            if (chargeTime > speedLevel3Mills)
            {
                //Debug.Log("fired 3");
                speed = speedLevel3;
            } else if (chargeTime > speedLevel2Mills)
            {
                //Debug.Log("fired 2");
                speed = speedLevel2;
            } else if (chargeTime > speedLevel1Mills)
            {
                //Debug.Log("fired 1");
                speed = speedLevel1;
            }
            var arrow = Instantiate(arrowPrefab);
            var arrowRigidbody = arrow.GetComponent<Rigidbody>();

            chargeStart = -1; //DEBUG

            arrow.transform.SetParent(arrowGroup);
            arrow.transform.position = characterTransform.position + (transform.forward.normalized * 0.5f) + (transform.up * 0.5f);
            arrow.transform.rotation = cameraTransform.rotation;
            arrowRigidbody.velocity = (cameraTransform.forward.normalized * speed);

            arrows.Add(arrow);
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }

        if (Input.GetKeyDown(KeyCode.Return))
        {
            foreach (GameObject arrow in arrows)
            {
                Destroy(arrow);
            }
        }

    }


}
