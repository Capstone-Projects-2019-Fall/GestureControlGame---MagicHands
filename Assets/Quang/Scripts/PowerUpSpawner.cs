using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerUpSpawner : MonoBehaviour
{
    static public int counter;
    public Transform[] SpeedBoostSpawnPoint;
    public GameObject SpeedBoostPowerUpObject;
    // Start is called before the first frame update
    void Start()
    {
        Spawner();
        
    }

    // Update is called once per frame
    void Update()
    {
        GameObject[] bettercounter = GameObject.FindGameObjectsWithTag("PowerUp");

        int newcounter = bettercounter.Length;
        //Debug.Log("this is counter " + newcounter);
        if (newcounter == 0)
        {
            Spawner();
        }
    }
    public void Spawner()
    {
        for (int i = 0; i < SpeedBoostSpawnPoint.Length; i++)
        {
            //Debug.Log("this is i " + i);
            Instantiate(SpeedBoostPowerUpObject, SpeedBoostSpawnPoint[i].transform.position, Quaternion.identity);
            //counter++;
        }
    }
}
