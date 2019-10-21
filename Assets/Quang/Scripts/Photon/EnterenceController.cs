using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnterenceController : MonoBehaviourPunCallbacks
 
{
    int randomRoomNumber;
    public static EnterenceController enterenceController;
    [SerializeField]
    private GameObject startButton; //button used for creating and joining a game.
    [SerializeField]
    private GameObject cancelButton; //button used to stop searching for a game to join.
    [SerializeField]
    private int RoomSize; //Manual set the number of player in the room at one time.



    private void Awake()
    {
        enterenceController = this;
    }

    void Start()
    {
        PhotonNetwork.ConnectUsingSettings(); //Connects to Photon master servers

    }
  
    public override void OnConnectedToMaster() //Callback function for when the first connection is established successfully.
    {
        Debug.Log("We are now connected to the " + PhotonNetwork.CloudRegion + " server");
        PhotonNetwork.AutomaticallySyncScene = true; //Makes it so whatever scene the master client has loaded is the scene all other clients will load
        startButton.SetActive(true);
    }
    public void OnStartClicked() 
    {
        startButton.SetActive(false);
        cancelButton.SetActive(true);
        PhotonNetwork.JoinRandomRoom(); //First tries to join an existing room
        Debug.Log("Start pressed");
    }
    public override void OnJoinRandomFailed(short returnCode, string message) //Callback function for if we fail to join a rooom
    {
        Debug.Log("Failed to join a room #"+ randomRoomNumber);
        CreateRoom();
    }
    void CreateRoom() //trying to create our own room
    {
        Debug.Log("Creating room now ");
        randomRoomNumber = Random.Range(0, 10000); //creating a random name for the room
        RoomOptions roomOps = new RoomOptions() { IsVisible = true, IsOpen = true, MaxPlayers = (byte)MultyplayerSettings.multyPlayerSettings.maxPlayers };
        PhotonNetwork.CreateRoom("Room" + randomRoomNumber, roomOps); //attempting to create a new room
        Debug.Log("Room #"+randomRoomNumber+" created");
    }

   /* public override void OnJoinedRoom()
    {
        Debug.Log("Room joined");
        base.OnJoinedRoom();
    }*/


    public override void OnCreateRoomFailed(short returnCode, string message) //callback function for if we fail to create a room. Most likely fail because room name was taken.
    {
        Debug.Log("Failed to create room... trying again");
        CreateRoom(); //Retrying to create a new room with a different name.
    }
    public void OnCancelClicked() //Paired to the cancel button. Used to stop looking for a room to join.
    {
        Debug.Log("Cancel clicked");
        cancelButton.SetActive(false);
        startButton.SetActive(true);
        PhotonNetwork.LeaveRoom(); 
    }
}
