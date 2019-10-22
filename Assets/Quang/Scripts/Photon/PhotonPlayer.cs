﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System;
using System.IO;

public class PhotonPlayer : MonoBehaviour
{
    private PhotonView PV;
    public GameObject myAvatar;
    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("within player.Start");

        PV = GetComponent<PhotonView>();
        //int spawnSelector = UnityEngine.Random.Range(0, GameSetup.GS.spawnPoints.Length);
        if (PV.IsMine)
        {
            myAvatar = PhotonNetwork.Instantiate(Path.Combine("Prefabs", "Player"),
                transform.position, Quaternion.identity, 0);//GameSetup.GS.spawnPoints[0].position, GameSetup.GS.spawnPoints[0].rotation
        }
        
    }

   

   
}