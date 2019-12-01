using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LobbyController : MonoBehaviourPunCallbacks,  IInRoomCallbacks

{
    int roomNumber;
    public static LobbyController lobbyController;
    //[SerializeField]
    //private GameObject startButton; //button used for creating and joining a game.
    [SerializeField]
    private Text _roomName;
    [SerializeField]
    private GameObject cancelButton; //button used to stop searching for a game to join.
    [SerializeField]
    private byte RoomSize; //Manual set the number of player in the room at one time.
   
    
    [SerializeField]
    private GameObject createServer;
    [SerializeField]
    private GameObject joinServer;

    [SerializeField]
    private InputField sizeRoom;

    private bool isCreated;
    





    public void OnCreateServer()
    {
        
        RoomSize = 2;
        int myInt = int.Parse(sizeRoom.text);
        byte myByte = (byte)myInt;
        RoomSize = (byte)myByte;
        
        MultyplayerSettings.multyplayerSettings.maxPlayers = myByte;
        Debug.Log("Creating room now, size "+ RoomSize);
        CreateRoom();

    }

    public void OnJoinServer()
    {

        Debug.Log("joining room");
        PhotonNetwork.JoinRandomRoom(); //Tries to join an existing room
                                        //PhotonNetwork.JoinRoom()
     
        


    }





    private void Awake()
    {
        lobbyController = this;
        //isCreated = false;
       
    }

    void Start()
    {
        Debug.Log("within multyplayer menu");
        PhotonNetwork.ConnectUsingSettings(); //Connects to Photon master servers


    }

    public override void OnConnectedToMaster() //Callback function for when the first connection is established successfully.
    {
        Debug.Log("We are now connected to the " + PhotonNetwork.CloudRegion + " server");
        //string str= PhotonNetwork.roo
        PhotonNetwork.AutomaticallySyncScene = true; //Makes it so whatever scene the master client has loaded is the scene all other clients will load
        //startButton.SetActive(true);

        createServer.SetActive(true);
        joinServer.SetActive(true);
    }
   
    public override void OnJoinRandomFailed(short returnCode, string message) //Callback function for if we fail to join a rooom
    {
        Debug.Log("Failed to join a room");
        //OnJoinServer();
    }
    void CreateRoom() //trying to create our own room
    {
        
        roomNumber = Random.Range(51, 10000); //creating a random name for the room
        RoomOptions roomOps = new RoomOptions() { IsVisible = true, IsOpen = true, MaxPlayers = (byte)RoomSize };// (byte)MultyplayerSettings.multyPlayerSettings.maxPlayers };
        PhotonNetwork.CreateRoom("Room" + roomNumber, roomOps); //attempting to create a new room
        Debug.Log("Room #" + roomNumber + " created");
    }

    


    public override void OnCreateRoomFailed(short returnCode, string message) //callback function for if we fail to create a room. Most likely fail because room name was taken.
    {
        Debug.Log("Failed to create room... trying again");
        CreateRoom(); //Retrying to create a new room with a different name.
    }
    public void OnCancelClicked() //Paired to the cancel button. Used to stop looking for a room to join.
    {
        Debug.Log("Cancel clicked");
        if(PhotonNetwork.InRoom)
            PhotonNetwork.LeaveRoom();
        //cancelButton.SetActive(false);
        //startButton.SetActive(true);
        Destroy(lobbyController);
       
        isCreated = false;
        PhotonNetwork.Disconnect();
    }

    /*
    void Update()
    {
        if(!isCreated)
        {
            sizeRoom.text = "";
            isCreated = true;
           
           

        }

    }
    */
}
