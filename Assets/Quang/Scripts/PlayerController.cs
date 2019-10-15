using UnityEngine;
using EZCameraShake;



public class PlayerController : MonoBehaviour
{
//this is test message
    private Rigidbody rb;
    public float speed;
    public float rotationSpeed;
    private int count;
    Controller controller;
    bool useMotionControl;

    public GameObject player;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        count = 0;
        useMotionControl = false;

        controller = initializeController(useMotionControl);
    }

    private Controller initializeController(bool useMotionControl)
    {
        if (useMotionControl)
        {
            return null;
        }
        else
        {
            return new MouseControl();
        }
    }

    void FixedUpdateQuang()
    {
        float xMove = Input.GetAxis("Horizontal");
        float zMove = Input.GetAxis("Vertical");

        Vector3 move = new Vector3(xMove, 0.0f, zMove);
        //Vector3 move = new Vector3(rightForce, 0.0f, upForce).normalized;

        rb.AddForce(move * speed);
    }

    void Update()
    {
        // mouse control
        //Vector3 rotate = Input.mousePosition;
        //float speedMag = 1f;
        //rotate.x = (rotate.x - Screen.width / 2) / Screen.width;
        //rotate.y = (rotate.y - Screen.height / 2) / Screen.height;
        //rotate.z = 0f;

        Vector3 rotate = controller.GetRotation();
        float speedMag = controller.GetSpeed();

        // hand control
        // Vector3 rotate = GetRotation();
        // float speedMag = GetSpeed();


        
        player.transform.Rotate(-rotate.y * Time.deltaTime * rotationSpeed, rotate.x * Time.deltaTime * rotationSpeed, rotate.z * Time.deltaTime * rotationSpeed, Space.Self);
        player.transform.position += speed * player.transform.forward * Time.deltaTime * speedMag * 1f;

        if (Input.GetKeyDown("v"))
        {
            CameraShaker.Instance.ShakeOnce(10f, 10f, .5f, 1.5f);
        }

    }

    void SpeedBoost(bool isSpeedBoost)
    {
        if (isSpeedBoost == true)
        {
            speed = speed * 2;

        }
    }
   /* void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("pickUp"))
        {
            other.gameObject.SetActive(false);
            count++;
        }
    }*/
}
