using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class selfDestruct : MonoBehaviour
{
    private float timer;
    public static int enemyCountDestroy;
    // Start is called before the first frame update
    void Start()
    {
        timer = 3.0f;
    }

    // Update is called once per frame
    void Update()
    {
        Destroy(gameObject, timer);
        enemyCountDestroy = 0;
    }
}
