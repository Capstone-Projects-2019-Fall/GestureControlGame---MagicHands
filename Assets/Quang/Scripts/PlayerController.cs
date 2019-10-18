using UnityEngine;
using EZCameraShake;
using Photon.Pun;

public class PlayerController : MonoBehaviour
{
    //this is test message
    private PhotonView PV;
    private Rigidbody rb;
    public float speed;
    public float rotationSpeed;
    private int count;
    //Controller controller;
    //public GameObject gameManager;
    public GameObject player;
    //GameManager gameManagerCode;

    void Start()
    {
       
        rb = GetComponent<Rigidbody>();
        count = 0;
        //gameManagerCode = gameManager.GetComponent<GameManager>();
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
        Vector3 rotate = GameManager.controller.GetRotation();
        float speedMag = GameManager.controller.GetSpeed();

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
