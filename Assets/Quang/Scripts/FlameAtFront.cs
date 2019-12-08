using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlameAtFront : MonoBehaviour
{
    public GameObject firepoint;
    // Update is called once per frame
    void Update()
    {
        gameObject.transform.position = firepoint.transform.position;
    }
}
