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

    private void FixedUpdate()
    {
        if (Vector3.Distance(transform.position, positions[current]) > 1f)
        {
            //Vector3 pos = Vector3.MoveTowards(transform.position, nodes[current].position, speed * Time.deltaTime);
            //GetComponent<Rigidbody>().MovePosition(pos);
            //ApplyRotate();
            //target.position = nodes[current].position + positionCorrection;
            target.position = positions[current];
            //PikcUpPower();
            TrackThePath();
        }
        else
        {
            current = (current + 1) % positions.Count;
        }
    }

    private void ApplyRotate()
    {
        float speedMag = 1f;
        Vector3 rotate;
        Vector3 relativeVector = transform.InverseTransformPoint(target.position);
        rotate.x = (relativeVector.x / relativeVector.magnitude) * rotationSpeed;
        rotate.y = (relativeVector.y / relativeVector.magnitude) * rotationSpeed;
        rotate.z = 0f;

        player.transform.Rotate(-rotate.y * Time.deltaTime * rotationSpeed, rotate.x*Time.deltaTime*rotationSpeed, rotate.z*Time.deltaTime*rotationSpeed, Space.Self);
        player.transform.position += speed * player.transform.forward * Time.deltaTime * speedMag * 1f;
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
        else if(transform.position == powerUp.position)
        {
            StartCoroutine(SpeedBoost());
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
