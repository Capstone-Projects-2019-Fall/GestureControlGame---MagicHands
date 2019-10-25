﻿using UnityEngine;
using EZCameraShake;
using System.Collections;
using System;



public class PlayerController : MonoBehaviour
{
//this is test message
    private Rigidbody rb;
    public float speed;
    public float rotationSpeed;
    private int count;
    //Controller controller;
    //public GameObject gameManager;
    public GameObject player;
    public static bool speedBoostState = false;
    //GameManager gameManagerCode;
    Vector3 PrevPos; 
    Vector3 NewPos; 
    Vector3 ObjVelocity;
    float speedMultiplier;
    public float hp;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        count = 0;
        PrevPos = transform.position;
        NewPos = transform.position;
        speedMultiplier = 1f;
        //gameManagerCode = gameManager.GetComponent<GameManager>();
    }
    void FixedUpdate()
    {
        float xMove = Input.GetAxis("Horizontal");
        float zMove = Input.GetAxis("Vertical");
        
        Vector3 move = new Vector3(xMove, 0.0f, zMove);
        //Vector3 move = new Vector3(rightForce, 0.0f, upForce).normalized;

        rb.AddForce(move * speed);

    }

    void Update()
    {
        if (!GameManager.started)
        {
            return;
        }
        Vector3 rotate = GameManager.controller.GetRotation();
        float speedMag = GameManager.controller.GetSpeed();

        rotate = rotate * Time.deltaTime * rotationSpeed;
        
        player.transform.Rotate(-rotate.y, rotate.x, rotate.z, Space.Self);
        player.transform.position += speedMultiplier * speed * player.transform.forward * Time.deltaTime * speedMag;
        

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
        if (Input.GetKeyDown("p"))
        {
            speedMultiplier = 0f;
        }
        if (Input.GetKeyUp("p"))
        {
            speedMultiplier = 1f;
        }
        NewPos = transform.position;  // each frame track the new position
        //Debug.Log("delta time:" + Time.deltaTime);
        //Debug.Log("fixed delta time:" + Time.fixedDeltaTime);
        ObjVelocity = (NewPos - PrevPos) / Time.deltaTime;  // velocity = dist/time
        PrevPos = NewPos;  // update position for next frame calculation
        Debug.Log("velocity "+ Math.Round(ObjVelocity.magnitude, 0));
    
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
    public float GetHealth()
    {
        return hp;
    }
    public float GetSpeed()
    {
        return speed;
    }

    public void SetHealth(int hpnew)
    {
        hp = hpnew;
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
