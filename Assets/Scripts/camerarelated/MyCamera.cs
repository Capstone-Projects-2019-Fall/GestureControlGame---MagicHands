using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyCamera : MonoBehaviour
{
    private PhotonView PV;
    public Vector3 cameraOffSet;
    GameObject player;

    private Transform target;

    // Start is called before the first frame update
    void Start()
    {
        PV = GetComponent<PhotonView>();
        cameraOffSet = new Vector3(0, 2, -5);
        
        
    }
    public void SetTarget(Transform target)
    {
        this.target = target;
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = target.transform.position + cameraOffSet;
    }
}
