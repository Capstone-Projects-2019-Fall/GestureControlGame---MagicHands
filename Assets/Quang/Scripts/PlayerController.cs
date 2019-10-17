// wrr world


using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using EZCameraShake;



public class PlayerController : MonoBehaviour
{
//this is test message
    private Rigidbody rb;
    public float speed;
    public float rotationSpeed;

    private int count;
    Thread receiveThread;
    UdpClient client;
    int port;
    float upForce;
    float rightForce;
    Vector3 controlVec;
    Vector3 leftVec;
    Vector3 rightVec;
    public static bool speedBoostState = false;

    public float Timer = 0.0f;

    public GameObject player;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        count = 0;
        port = 5065;
        InitUDP();
        leftVec = new Vector3(0f, 0f, 0f);
        rightVec = new Vector3(0f, 0f, 0f);
    }

    // 3. InitUDP
    private void InitUDP()
    {
        print("UDP Initialized");
        receiveThread = new Thread(new ThreadStart(ReceiveData));
        receiveThread.IsBackground = true;
        receiveThread.Start();
    }

    // 4. Receive Data
    private void ReceiveData()
    {
        client = new UdpClient(port);
        while (true)
        {
            try
            {
                IPEndPoint anyIP = new IPEndPoint(IPAddress.Parse("0.0.0.0"), port);
                byte[] data = client.Receive(ref anyIP);
                string text = Encoding.UTF8.GetString(data);
                print(text);
                UpdateLeftRightVec(text);
            }
            catch (Exception e)
            {
                print(e.ToString());
            }
        }
    }

    void UpdateLeftRightVec(string signal)
    {
        var cords = signal.Split(',');
        
        var left = cords[0].Split(' ');
        var right = cords[1].Split(' ');
        try
        {
            leftVec = new Vector3(float.Parse(left[0]), float.Parse(left[1]), 0f);
            rightVec = new Vector3(float.Parse(right[0]), float.Parse(right[1]), 0f);

            float mag = (leftVec - rightVec).magnitude;
            Vector3 vec = (leftVec + rightVec) / 2;
            vec.z = mag;
        }
        catch (Exception e)
        {
            print(e.ToString());
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

    Vector3 GetRotation()
    {
        Vector3 vec = (leftVec + rightVec) / 2;
        Vector3 leftRight = rightVec - leftVec;
        float angle;
        if (leftRight.magnitude == 0f)
        {
            angle = 0f;
        }
        else {
            angle = Vector3.SignedAngle(new Vector3(1f, 0f, 0f), leftRight, new Vector3(0f, 0f, 1f));
        }
        vec.z = angle / 360;
        return vec;
    }

    float GetSpeed()
    {
        return (leftVec - rightVec).magnitude;
    }

    void Update()
    {
        // mouse control
        Vector3 rotate = Input.mousePosition;
        float speedMag = 1f;
        rotate.x = (rotate.x - Screen.width / 2) / Screen.width;
        rotate.y = (rotate.y - Screen.height / 2) / Screen.height;
        rotate.z = 0f;
    


        // hand control
        // Vector3 rotate = GetRotation();
        // float speedMag = GetSpeed();



        player.transform.Rotate(-rotate.y * Time.deltaTime * rotationSpeed, rotate.x*Time.deltaTime*rotationSpeed, rotate.z*Time.deltaTime*rotationSpeed, Space.Self);
        player.transform.position += speed * player.transform.forward * Time.deltaTime * speedMag * 1f;

        if (Input.GetKeyDown("v"))
        {
            CameraShaker.Instance.ShakeOnce(10f, 10f, .5f, 1.5f);
        }
        if (Input.GetKeyDown("space"))
        {
            if (speedBoostState == true)
            {
                StartCoroutine(SpeedBoost());
            }
        }

    }
    IEnumerator SpeedBoost()
    {
        
        float oldspeed = speed;
        float newspeed = speed * 2;
        speed = newspeed;
        yield return new WaitForSeconds(3);
        speed = oldspeed;
        speedBoostState=false;

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
