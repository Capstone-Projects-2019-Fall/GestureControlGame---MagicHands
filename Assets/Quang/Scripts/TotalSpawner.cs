using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TotalSpawner : MonoBehaviour
{
    static public int counter;
    public Transform[] RingSpawnPoint;
    public GameObject RingObject;
    public Transform[] HazardSpawnPoint;
    public GameObject HazardObject;

    // Start is called before the first frame update
    void Start()
    {
        RingSpawner();
        //HazardSpawner();

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
    public void RingSpawner()
    {
        for (int i = 0; i < RingSpawnPoint.Length; i++)
        {
            Instantiate(RingObject, new Vector3(RingSpawnPoint[i].transform.position.x, RingSpawnPoint[i].transform.position.y+2.0f,
                RingSpawnPoint[i].transform.position.z), RingSpawnPoint[i].transform.rotation);
            //counter++;
        }
    }
    /*public void HazardSpawner()
    {
        for (int i = 0; i < HazardSpawnPoint.Length; i++)
        {
            Instantiate(HazardObject, HazardSpawnPoint[i].transform.position, HazardSpawnPoint[i].transform.rotation);
            //counter++;
        }
    }*/
}
