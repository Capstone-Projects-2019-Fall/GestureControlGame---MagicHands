using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;

public class CreateRoomMenu : MonoBehaviourPunCallbacks
{
    [SerializeField]
    private Text _roomName;
    private RoomsCanvases _roomCanvases;

    public void FirstInitialize(RoomsCanvases canvases)
    {
        _roomCanvases = canvases;
    }
    
    public void OnClick_CreateRoom()
    {
        if(!PhotonNetwork.IsConnected)
            return;
            
        RoomOptions options = new RoomOptions();
        options.MaxPlayers = 4;
        PhotonNetwork.JoinOrCreateRoom(_roomName.text, options, TypedLobby.Default);
        
    }
    
    public void OnClick_Leave()
    {
        SceneManager.LoadScene("Menu2");
        

    }

    public override void OnCreatedRoom()
    {
        Debug.Log("Created Room succesfully.", this);
        _roomCanvases.CurrentRoomCanvas.Show();
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.Log("Room creation failed:" + message, this);
    }


}
