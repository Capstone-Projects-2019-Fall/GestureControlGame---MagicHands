using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Teleport : MonoBehaviour
{
    public Transform teleporTarget;
    public GameObject player;
    public void OnTriggerEnter(Collider other)
    {
        player.transform.position = teleporTarget.transform.position;
    }
}
