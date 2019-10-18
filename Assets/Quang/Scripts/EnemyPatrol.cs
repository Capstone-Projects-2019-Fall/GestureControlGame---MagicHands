using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyPatrol : MonoBehaviour
{
    public float speed;
    private float waitTime;
    public float startWaitTime;
    public Transform[] moveSpots;
    private int circularMove;
    private int counter=0;
    static public bool speedBoostState = false;
    // Start is called before the first frame update
    void Start()
    {
        waitTime = startWaitTime;
        circularMove = 0;
    }

    // Update is called once per frame
    void Update()
    {
        
                transform.position = Vector3.MoveTowards(transform.position, moveSpots[circularMove].position,
                    speed * Time.deltaTime);
        if (speedBoostState == true)
        {
            StartCoroutine(SpeedBoost());
        }

        if (Vector3.Distance(transform.position, moveSpots[circularMove].position) < 0.2f)
        {
                if (waitTime <= 0)
                {
                    circularMove++;
                    if (circularMove == moveSpots.Length)
                    {
                        circularMove = 0;
                    }// Random.Range(0, moveSpots.Length);
                    waitTime = startWaitTime;
                }
                else
                {
                    waitTime -= Time.deltaTime;
                }
            
        }
    }
    IEnumerator SpeedBoost()
    {

        float oldspeed = speed;
        float newspeed = speed * 2;
        speed = newspeed;
        yield return new WaitForSeconds(3);
        speed = oldspeed;
        speedBoostState = false;

    }
}
