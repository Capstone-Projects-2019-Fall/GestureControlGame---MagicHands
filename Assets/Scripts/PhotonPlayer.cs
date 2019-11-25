﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System;
using System.IO;

public class PhotonPlayer : MonoBehaviourPunCallbacks
{
    private PhotonView PV;
    public GameObject myAvatar;
    public GameObject myCamera;
    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("within PPlayer.Start");

        PV = GetComponent<PhotonView>();
        int spawnSelector = UnityEngine.Random.Range(0, GameSetup.GS.spawnPoints.Length);
        if (PV.IsMine)
        {
            Debug.Log("within PPlayer.Start ismine");
            myAvatar = PhotonNetwork.Instantiate(Path.Combine("Prefabs", "Player2"),
                GameSetup.GS.spawnPoints[spawnSelector].position, GameSetup.GS.spawnPoints[spawnSelector].rotation, 0); //*/ transform.position, Quaternion.identity, 0);//
          
            
           



            //Camera.main.transform.position = new Vector3(0, 2, -5);
            Camera.main.transform.position = myAvatar.transform.position - myAvatar.transform.forward * 5 +
                myAvatar.transform.up * 2;
            Camera.main.transform.rotation = Quaternion.identity;
            // Camera.main.transform.localEulerAngles = new Vector3(10, 0, 0);

            Camera.main.transform.LookAt(myAvatar.transform.position);

            
            Camera.main.transform.parent = myAvatar.transform;
            
           // myAvatar.GetComponent<Camera>().enabled = true;            
           // myAvatar.GetComponent<Camera>().transform.parent = myAvatar.transform;

           // myAvatar.GetComponent<MyCamera>().SetTarget(myAvatar.transform);
            //myAvatar.transform.Find("Camera").gameObject.SetActive(true);*/

        }
        

    }




}
