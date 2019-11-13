using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MultyplayerSettings : MonoBehaviour
{

    public static MultyplayerSettings multyplayerSettings;

    public bool delayStart;
    public int maxPlayers;

    public int menuScene;
    public int multyplayerScene;

    private void Awake()
    {
        if (MultyplayerSettings.multyplayerSettings == null)
        {
            MultyplayerSettings.multyplayerSettings = this;
        }
        else
        {
            if (MultyplayerSettings.multyplayerSettings != this)
            {
                Destroy(this.gameObject);
            }

        }
        DontDestroyOnLoad(this.gameObject);
    }

}
