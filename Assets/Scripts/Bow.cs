using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bow : MonoBehaviour
{
    [Header("GameObject References")]
    [SerializeField] private Transform arrowGroup = null;
    [SerializeField] private Transform cameraTransform = null;
    [SerializeField] private Transform characterTransform = null;
    [SerializeField] private GameObject arrowPrefab = null;

    [Header("Bow Properties")]
    public int speedLevel0 = 4;
    public int speedLevel1 = 8;
    public int speedLevel2 = 16;
    public int speedLevel3 = 24;

    public int speedLevel1Mills = 200;
    public int speedLevel2Mills = 600;
    public int speedLevel3Mills = 1000;

    private List<GameObject> arrows;

    private float chargeStart = -1;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    void Awake()
    {
        arrows = new List<GameObject>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            chargeStart = Time.time;
        }

        if (Input.GetKeyUp(KeyCode.Mouse0))
        {
            float chargeTime = Time.time*1000 - chargeStart*1000;
            int speed = speedLevel0;
            if (chargeTime > speedLevel3Mills)
            {
                Debug.Log("sl3");
                speed = speedLevel3;
            } else if (chargeTime > speedLevel2Mills)
            {
                Debug.Log("sl2");
                speed = speedLevel2;
            } else if (chargeTime > speedLevel1Mills)
            {
                Debug.Log("sl1");
                speed = speedLevel1;
            }
            var arrow = Instantiate(arrowPrefab);
            var arrowRigidbody = arrow.GetComponent<Rigidbody>();

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
