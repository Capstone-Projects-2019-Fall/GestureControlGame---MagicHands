using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RingSpawner : MonoBehaviour
{
    static public int counter;
    public Transform[] RingSpawnPoint;
    public GameObject RingObject;
    // Start is called before the first frame update
    void Start()
    {
        Spawner();

    }

    // Update is called once per frame
    void Update()
    {
        /*GameObject[] bettercounter = GameObject.FindGameObjectsWithTag("PowerUp");

        int newcounter = bettercounter.Length;
        Debug.Log("this is counter " + newcounter);
        if (newcounter == 0)
        {
            Spawner();
        }*/
    }
    public void Spawner()
    {
        for (int i = 0; i < RingSpawnPoint.Length; i++)
        {
            Instantiate(RingObject, new Vector3(RingSpawnPoint[i].transform.position.x, RingSpawnPoint[i].transform.position.y+2.0f,
                RingSpawnPoint[i].transform.position.z), RingSpawnPoint[i].transform.rotation);
            //counter++;
        }
    }
}
