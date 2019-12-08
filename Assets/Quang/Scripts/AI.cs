using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AI : MonoBehaviour
{

    public Transform path;
    public Transform target;
    public float MaxSpeed;
    public float speed = 0;
    public float acc;
    public float rotationSpeed;
    public float agressive;
    public bool speedBoostState = false;
    public bool speedBoostInProgress = false;

    private List<Transform> nodes;
    List<Vector3> positions;
    private int current = 0;


    private Rigidbody rb;
    public GameObject player;
    private int hp;

    private float speedModifyer = 0.6f;
    

    void Start()
    {
        hp = 100;

        Vector3 positionCorrection = new Vector3(0f, -1.5f, 0f);

        Transform[] pathTransform = path.GetComponentsInChildren<Transform>();
        nodes = new List<Transform>();
        positions = new List<Vector3>();

        rb = GetComponent<Rigidbody>();

        for (int i = 0; i < pathTransform.Length; i++)
        {
            if (pathTransform[i] != path.transform)
            {
                nodes.Add(pathTransform[i]);
                positions.Add(pathTransform[i].position + positionCorrection);
                Debug.Log("target Position: " + (pathTransform[i].position + positionCorrection));
            }
        }
    }

    private void Update()
    {
        if (hp < 1)
        {
            destroyed();
        }
        if (Vector3.Distance(transform.position, positions[current]) > 1.5f)
        {
            //Vector3 pos = Vector3.MoveTowards(transform.position, nodes[current].position, speed * Time.deltaTime);
            //GetComponent<Rigidbody>().MovePosition(pos);
            //ApplyRotate();
            //target.position = nodes[current].position + positionCorrection;
            target.position = positions[current];
            PikcUpPower();
            TrackThePath();
        }
        else
        {
            current = (current + 1) % positions.Count;
        }

        if (speedBoostState == true && speedBoostInProgress == false)
        {
            StartCoroutine(SpeedBoost());
            speedBoostInProgress = true;
        }
    }

    private void TrackThePath()
    {
        float speedMag = 1f;
        float breakAngle = 60f;

        speedModifyer = 1f;
        Vector3 rotate;
        //Vector3 relativeVector = transform.InverseTransformPoint(target.position);
        Vector3 diff = target.position - transform.position;
        // Debug.Log(diff);

        float angle = Vector3.Angle(diff, transform.forward);

        if((angle > 60.0f) && (speed > (MaxSpeed * 0.6)))
        {
            speed -= acc;
        } else if(rb.velocity.magnitude < MaxSpeed)
        {
            speed += acc;
        }

        //transform.Rotate(-rotate.y * Time.deltaTime, rotate.x * Time.deltaTime, rotate.z*Time.deltaTime, Space.Self);

        if(rb.velocity.magnitude < MaxSpeed)
        {
            speed += acc;
        }

        speed = MaxSpeed;
        
        if(angle > 60.0f)
        {
            speedModifyer = 0.45f; 
        }

        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(diff.normalized), rotationSpeed * Time.deltaTime);
        //rb.velocity = transform.forward * speed;
        transform.position += MaxSpeed * player.transform.forward * Time.deltaTime * speedMag * speedModifyer;
        
    }

    private void PikcUpPower()
    {
        GameObject ClosestPowerUp = FindClosestPowerUp();
        if (ClosestPowerUp == null) return;
        Transform powerUp = ClosestPowerUp.transform;
   
        if(Vector3.Distance(powerUp.position, transform.position) < (Vector3.Distance(positions[current], transform.position))*agressive)
        {
            target.position = powerUp.position;
        }
    }

    private GameObject FindClosestPowerUp()
    {
        GameObject[] gos;
        gos = GameObject.FindGameObjectsWithTag("PowerUp");
        GameObject closest = null;
        float distance = Mathf.Infinity;
        Vector3 position = transform.position;
        foreach (GameObject go in gos)
        {
            Vector3 diff = go.transform.position - position;
            float curDistance = diff.sqrMagnitude;
            if (curDistance < distance)
            {
                closest = go;
                distance = curDistance;
            }
        }
        return closest;
    }

    private void destroyed()
    {
        Destroy(this.gameObject);
    }

    IEnumerator SpeedBoost()
    {
    
        Debug.Log("Power up boject picked up");
        float oldspeed = MaxSpeed;
        float newspeed = MaxSpeed * 2;
        MaxSpeed = newspeed;
        yield return new WaitForSeconds(3);
        MaxSpeed = oldspeed;
        speedBoostState = false;

    }

}
