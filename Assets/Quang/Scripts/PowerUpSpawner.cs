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
        if (counter == 0)
        {
            Spawner();
        }
    }
    public void Spawner()
    {
        for (int i = 0; i < SpeedBoostSpawnPoint.Length; i++)
        {
            Instantiate(SpeedBoostPowerUpObject, SpeedBoostSpawnPoint[i].transform.position, Quaternion.identity);
            counter++;
        }
    }
}
