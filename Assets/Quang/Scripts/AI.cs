using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AI : MonoBehaviour
{

    public Transform path;
    public Transform powerUp;
    public Transform target;
    public float speed;
    public float rotationSpeed;
    public static bool speedBoostState = false;
    Vector3 positionCorrection = new Vector3(0f, -2f, 0f);

    private List<Transform> nodes;
    List<Vector3> positions;
    private int current = 0;


    private Rigidbody rb;
    public GameObject player;

    void Start()
    {
        Transform[] pathTransform = path.GetComponentsInChildren<Transform>();
        nodes = new List<Transform>();
        positions = new List<Vector3>();

        for (int i = 0; i < pathTransform.Length; i++)
        {
            if (pathTransform[i] != path.transform)
            {
                nodes.Add(pathTransform[i]);
                positions.Add(pathTransform[i].position + positionCorrection);
            }
        }
    }

    private void Update()
    {
        if (Vector3.Distance(transform.position, positions[current]) > 1f)
        {
            //Vector3 pos = Vector3.MoveTowards(transform.position, nodes[current].position, speed * Time.deltaTime);
            //GetComponent<Rigidbody>().MovePosition(pos);
            //ApplyRotate();
            //target.position = nodes[current].position + positionCorrection;
            target.position = positions[current];
            //PikcUpPower();
            ApplyRotate();
        }
        else
        {
            current = (current + 1) % positions.Count;
        }
        if (speedBoostState == true)
        {
            StartCoroutine(SpeedBoost());
        }
    }

    private void ApplyRotate()
    {
        float speedMag = 1f;
        Vector3 rotate;
        Vector3 relativeVector = transform.InverseTransformPoint(target.position);

        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(target.position - transform.position), rotationSpeed * Time.deltaTime);


        rotate.x = (-transform.eulerAngles.x + relativeVector.x / relativeVector.magnitude) * rotationSpeed;
        rotate.y = (-transform.eulerAngles.y + relativeVector.y / relativeVector.magnitude) * rotationSpeed;
        rotate.z = (-transform.eulerAngles.z) * rotationSpeed;

        //transform.Rotate(-rotate.y * Time.deltaTime, rotate.x * Time.deltaTime, rotate.z*Time.deltaTime, Space.Self);
        transform.position += speed * player.transform.forward * Time.deltaTime * speedMag * 1f;
    }

    private void TrackThePath()
    {
        Vector3 targetDir = target.position - transform.position;
        Vector3 tarDir = Vector3.RotateTowards(transform.forward, targetDir, speed * Time.deltaTime, rotationSpeed);
        transform.forward = targetDir;
        transform.position = Vector3.MoveTowards(transform.position, target.position, speed * Time.deltaTime);

    }

    private void PikcUpPower()
    {
        if(Vector3.Distance(powerUp.position, transform.position) < Vector3.Distance(positions[current], transform.position))
        {
            target.position = powerUp.position;
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
