using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpeedBoostPowerUp : MonoBehaviour
{
    

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
            PlayerController.speedBoostState=true;
            //if put Destroy(gameObject) outside of these tag condition, cause too many problems
            Destroy(gameObject);

        }
        else if(other.gameObject.tag == "Enemy")
        {
            EnemyPatrol.speedBoostState = true;
            Destroy(gameObject);
        }
        /*if (PowerUpSpawner.counter > 0)
        {
            PowerUpSpawner.counter--;
        }*/


    }
}
