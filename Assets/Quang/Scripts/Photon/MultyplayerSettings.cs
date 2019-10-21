using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MultyplayerSettings : MonoBehaviour
{

    public static MultyplayerSettings multyPlayerSettings;

    public bool delayStart;
    public int maxPlayers;

    public int menuScene;
    public int multyPlayerScene;

    private void Awake()
    {
        if (MultyplayerSettings.multyPlayerSettings == null)
        {
            MultyplayerSettings.multyPlayerSettings = this;
        }
        else
        {
            if (MultyplayerSettings.multyPlayerSettings != this)
            {
                Destroy(this.gameObject);
            }

        }
        DontDestroyOnLoad(this.gameObject);
    }


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
