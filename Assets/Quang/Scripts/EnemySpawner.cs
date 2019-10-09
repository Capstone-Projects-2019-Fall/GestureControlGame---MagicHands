using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public Transform EnemySpawnPoint;
    public GameObject enemy;
    public static int enemyCount;
    public void OnTriggerEnter(Collider other)
    {
        while (enemyCount < 10)
        {
            Instantiate(enemy, EnemySpawnPoint.transform.position, Quaternion.identity);
            enemyCount++;
        }
        enemyCount = selfDestruct.enemyCountDestroy;
    }
}
