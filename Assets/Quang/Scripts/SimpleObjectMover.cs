using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class SimpleObjectMover : MonoBehaviourPun
{
    [SerializeField]
    private float _moveSpeed;
    void Update()
    {
        if(base.photonView.IsMine)
        {
            float x = Input.GetAxisRaw("Horizontal");
            float y = Input.GetAxisRaw("Vertical");
            transform.position += (new Vector3(x, y, 0f) * _moveSpeed);
        }
    }
}
