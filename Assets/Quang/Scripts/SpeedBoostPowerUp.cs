using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpeedBoostPowerUp : MonoBehaviour
{
    //upon being in contact with another object, if player grant them the ability to speed up with
    //when a key is hit, if enemy, boost them up immediately
    // Start is called before the first frame update
    //private float playerSpeed = gameObject.GetComponent<PlayerController> speed;
    //static private float enemySpeed = EnemyPatrol.GetComponent(speed);
    private float speedTimer = 0.0f;
    
    void Start()
    {
        
    }
    // Update is called once per frame
    void Update()
    {
        
    }
    public void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            if (Input.GetKeyDown("space"))
            {
                //playerSpeed = playerSpeed * 2.0f;
                Destroy(gameObject);
            }
        }
   /*     else if(other.gameObject.tag == "Enemy")
        {
            while (speedTimer <3.0f)
            {
                EnemyPatrol.speed = EnemyPatrol.speed * 2.0f;
                speedTimer=speedTimer + Time.deltaTime;
            }
            if (speedTimer == 3.0f)
            {
                speedTimer = 0.0f;
            }
        }*/
    }
}
