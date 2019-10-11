using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AI : MonoBehaviour
{

    public Transform path;
    public float speed;
    public float rotationSpeed;

    private List<Transform> nodes;
    private int current = 0;

    private Rigidbody rb;
    public GameObject player;

    void Start()
    {
        Transform[] pathTransform = path.GetComponentsInChildren<Transform>();
        nodes = new List<Transform>();

        for (int i = 0; i < pathTransform.Length; i++)
        {
            if (pathTransform[i] != path.transform)
            {
                nodes.Add(pathTransform[i]);
            }
        }
    }

    private void FixedUpdate()
    {
        if (transform.position != nodes[current].position)
        {
            //Vector3 pos = Vector3.MoveTowards(transform.position, nodes[current].position, speed * Time.deltaTime);
            //GetComponent<Rigidbody>().MovePosition(pos);
            ApplyRotate();
        }
        else
        {
            current = (current + 1) % nodes.Count;
        }
    }

    private void ApplyRotate()
    {
        float speedMag = 1f;
        Vector3 rotate;
        Vector3 relativeVector = transform.InverseTransformPoint(nodes[current].position);
        rotate.x = (relativeVector.x / relativeVector.magnitude) * rotationSpeed;
        rotate.y = (relativeVector.y / relativeVector.magnitude) * rotationSpeed;
        rotate.z = 0f;

        player.transform.Rotate(-rotate.y * Time.deltaTime * rotationSpeed, rotate.x*Time.deltaTime*rotationSpeed, rotate.z*Time.deltaTime*rotationSpeed, Space.Self);
        player.transform.position += speed * player.transform.forward * Time.deltaTime * speedMag * 1f;
    }

}
