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
    [SerializeField]
    private Text _roomSize;
    private RoomsCanvases _roomCanvases;
    [SerializeField]
    private InputField sizeInputField;

    public void FirstInitialize(RoomsCanvases canvases)
    {
        _roomCanvases = canvases;
    }
    
    public void OnClick_CreateRoom()
    {
        if(!PhotonNetwork.IsConnected)
            return;

        TimeCounter.timeCounter.lastTime = Time.realtimeSinceStartup;
        RoomOptions options = new RoomOptions();
        options.MaxPlayers = (byte) int.Parse(Mathf.Clamp(float.Parse(_roomSize.text), 2, 8).ToString());
        PhotonNetwork.JoinOrCreateRoom(_roomName.text, options, TypedLobby.Default);
        
    }

    
    public void OnClick_Leave()
    {   
        PhotonNetwork.Disconnect();
        SceneManager.LoadScene("Menu2");

    }

    public void OnClick_SetRoomSize()
    {
        sizeInputField.text =  Mathf.Clamp(float.Parse(_roomSize.text), 2, 8).ToString();

        print(_roomSize.text);
    }

    public override void OnCreatedRoom()
    {
        Debug.Log("time spent for creating room: " + (Time.realtimeSinceStartup - TimeCounter.timeCounter.lastTime) + " seconds");
        Debug.Log("Created Room succesfully.", this);
        _roomCanvases.CurrentRoomCanvas.Show();
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.Log("Room creation failed:" + message, this);
    }


}
