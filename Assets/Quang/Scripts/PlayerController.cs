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
    public bool useMotionControl;

    public GameObject player;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        count = 0;
        //useMotionControl = true;
        controller = initializeController(useMotionControl);
    }

    private Controller initializeController(bool useMotionControl)
    {
        if (useMotionControl)
        {
            return new MotionControl();
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
        Vector3 rotate = controller.GetRotation();
        float speedMag = controller.GetSpeed();

        rotate = rotate * Time.deltaTime * rotationSpeed;
        
        player.transform.Rotate(-rotate.y, rotate.x, rotate.z, Space.Self);
        player.transform.position += speed * player.transform.forward * Time.deltaTime * speedMag;

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
