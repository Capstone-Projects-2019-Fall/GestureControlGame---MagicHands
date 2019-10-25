using Photon.Pun;
using UnityEngine;
using Photon.Realtime;
using UnityEngine.SceneManagement;
using System.IO;

public class RoomController : MonoBehaviourPunCallbacks, IInRoomCallbacks
{
    //room info
    public static RoomController room;
    private PhotonView PV;

    public bool isGameLoaded;
    public int currentScene;

    static Player[] playerList;
    public int playersInRoom;
    public int myNumberInRoom;

    public int playerInGame;
    //delays
    private bool readyToCount;
    private bool readyToStart;
    public float startingTime;
    private float lessThanMaxPlayers;
    private float atMaxPlayers;
    private float timeToStart;



    public GameObject myAvatar;


    private void Awake()
    {
        //set up singleton
        if (RoomController.room == null)
        {
            RoomController.room = this;
        }
        else
        {
            if (RoomController.room != this)
            {
                Destroy(RoomController.room.gameObject);
                RoomController.room = this;
            }


        }
        DontDestroyOnLoad(this.gameObject);
    }
    public override void OnEnable()
    {
        base.OnEnable();
        PhotonNetwork.AddCallbackTarget(this);
        SceneManager.sceneLoaded += OnSceneFinishedLoading;
    }
    public override void OnDisable()
    {
        base.OnDisable();
        PhotonNetwork.RemoveCallbackTarget(this);
        SceneManager.sceneLoaded -= OnSceneFinishedLoading;
    }

    void Start()
    {
        PV = GetComponent<PhotonView>();
        readyToCount = false;
        readyToStart = false;
        lessThanMaxPlayers = startingTime;
        atMaxPlayers = 3;
        timeToStart = startingTime;
    }


    void Update()
    {
        if (MultyplayerSettings.multyplayerSettings.delayStart)
        {
           
            if (!isGameLoaded)
            {
                if (readyToStart)
                {

                    atMaxPlayers -= Time.deltaTime;
                    lessThanMaxPlayers = atMaxPlayers;
                    timeToStart = atMaxPlayers;
                }
                else
                {
                    /*if (readyToCount)
                    {
                        lessThanMaxPlayers -= Time.deltaTime;
                        timeToStart = lessThanMaxPlayers;
                    }*/
                }
                //Debug.Log("Time to start: " + timeToStart);
                if (timeToStart <= 0)
                {
                    StartGame();
                }
            }
        }

    }
    public override void OnJoinedRoom() //Callback function for when we successfully create or join a room.
    {
        base.OnJoinedRoom();
        Debug.Log("Room joined");
        playerList = PhotonNetwork.PlayerList;
        playersInRoom = PhotonNetwork.PlayerList.Length;
        myNumberInRoom = playersInRoom;
        PhotonNetwork.NickName = myNumberInRoom.ToString();
        if (MultyplayerSettings.multyplayerSettings.delayStart)
        {
            Debug.Log("Players in room (" + playersInRoom + ":" + MultyplayerSettings.multyplayerSettings.maxPlayers + ")");

            if (playersInRoom == MultyplayerSettings.multyplayerSettings.maxPlayers)
            {
                readyToStart = true;
                if (!PhotonNetwork.IsMasterClient)
                    return;
                PhotonNetwork.CurrentRoom.IsOpen = false;
            }
        }
        else
        {
            StartGame();
        }

    }
    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
    {
        base.OnPlayerEnteredRoom(newPlayer);
        Debug.Log("New player has entered room");
        playerList = PhotonNetwork.PlayerList;
        playersInRoom++;
        if (MultyplayerSettings.multyplayerSettings.delayStart)
        {
            Debug.Log("Players in room (" + playersInRoom + ":" + MultyplayerSettings.multyplayerSettings.maxPlayers + ")");

            if (playersInRoom == MultyplayerSettings.multyplayerSettings.maxPlayers)
            {
                readyToStart = true;
                if (!PhotonNetwork.IsMasterClient)
                    return;
                PhotonNetwork.CurrentRoom.IsOpen = false;
            }



        }
    }


    void StartGame() //Function for loading into the multiplayer scene.
    {
        Debug.Log("Starting Game");
        isGameLoaded = true;

        if (!PhotonNetwork.IsMasterClient)
        {
            return;
        }
        if (MultyplayerSettings.multyplayerSettings.delayStart)
        {
            PhotonNetwork.CurrentRoom.IsOpen = false;


        }
        PhotonNetwork.LoadLevel(MultyplayerSettings.multyplayerSettings.multyplayerScene); //because of AutoSyncScene all photonPlayers who join the room will also be loaded into the multiplayer scene.
    }
  /*  void RestartTimer()
    {
        //Debug.Log("Time to start: " + timeToStart);
        readyToCount = false;
        readyToStart = false;
        lessThanMaxPlayers = startingTime;
        atMaxPlayers = 6;
        timeToStart = startingTime;

    }*/
    void OnSceneFinishedLoading(Scene scene, LoadSceneMode mode)
    {

        currentScene = scene.buildIndex;
        if (currentScene == MultyplayerSettings.multyplayerSettings.multyplayerScene)
        {
            isGameLoaded = true;
            if (MultyplayerSettings.multyplayerSettings.delayStart)
            {
                PV.RPC("RPC_LoadedGameScene", RpcTarget.MasterClient);
            }
            else
            {
                RPC_CreatePlayer();
            }
        }


    }
    [PunRPC]
    private void RPC_LoadedGameScene()
    {
        playerInGame++;
        if (playerInGame == PhotonNetwork.PlayerList.Length)
        {
            PV.RPC("RPC_CreatePlayer", RpcTarget.All);

        }
    }
    [PunRPC]
    private void RPC_CreatePlayer()
    {
        int spawnSelector = UnityEngine.Random.Range(0, GameSetup.GS.spawnPoints.Length);
        //PhotonNetwork.Instantiate(Path.Combine("Prefab", "TempEnemy"), transform.position, Quaternion.identity, 0);
        //PhotonNetwork.Instantiate("Assets/Prefab/TempEnemy", transform.position, Quaternion.identity, 0);
        PhotonNetwork.Instantiate(Path.Combine("Prefabs", "NetworkPlayer"), transform.position, Quaternion.identity, 0);
        
    }


}
