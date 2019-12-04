using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Realtime;

public class PlayerListing : MonoBehaviour
{
    [SerializeField]
    private Text _text;

    private int playerNumber;

    public Player Player {get; private set;}

    public bool Ready = false;

    public void SetPlayerInfo(Player player)
    {   
       //int result = -1;
       Player = player;

       playerNumber = player.ActorNumber;

       print("Actor Number " + playerNumber);

       /*if(player.CustomProperties.ContainsKey("RandomNumber"))
            result = (int) player.CustomProperties["RandomNumber"];*/
       
       _text.text = /*result.ToString() + ", " +*/ player.NickName;
    }

    public int getActorNumber()
    {
        return playerNumber;
    }

}
