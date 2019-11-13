using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class simpleMoves : MonoBehaviour
{
    private PhotonView PV;
    private CharacterController myCC;
    public float moveS;
    public float rotS;

    // Start is called before the first frame update
    void Start()
    {
        PV = GetComponent<PhotonView>();
        myCC = GetComponent<CharacterController>();
        
    }

    void Update()
    {
        if (PV.IsMine)//the most stupid error
        {
            basics();
            //rotation();
        }

    }


    void basics()
    {
        if (Input.GetKey(KeyCode.W))
        {
            myCC.Move(transform.forward * Time.deltaTime * moveS);
        }
        if (Input.GetKey(KeyCode.D))
        {
            myCC.Move(transform.right * Time.deltaTime * moveS);
        }
        if (Input.GetKey(KeyCode.A))
        {
            myCC.Move(-transform.right * Time.deltaTime * moveS);

        }
        if (Input.GetKey(KeyCode.S))
        {
            myCC.Move(-transform.forward * Time.deltaTime * moveS);
        }
    }
    void rotation()
    {
        float mouseX = Input.GetAxis("MouseX") * Time.deltaTime * rotS;
        transform.Rotate(new Vector3(0, mouseX, 0));
    }

    // Update is called once per frame
    
}
