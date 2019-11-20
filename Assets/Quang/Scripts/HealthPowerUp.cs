using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthPowerUp : MonoBehaviour
{
    public GameObject player;
    public void OnTriggerEnter(Collider other)
    {

        if (other.gameObject.tag == "Player")
        {
            player.GetComponent<PlayerController>().SetHealth(-50.0f);
            //if put Destroy(gameObject) outside of these tag condition, cause too many problems
            Destroy(gameObject);
        }
    }
}
