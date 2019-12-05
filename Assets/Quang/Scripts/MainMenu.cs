using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{

    public static string photonNickname;

    
    public Text nickname;

    void Start()
    {
  
    }

    public void Multyplayer()
    {
        Debug.Log("Multyplayer pressed");
        SceneManager.LoadScene("LobbyScene");
        
    }

    public void SinglePlayer()
    {
        Debug.Log("Single player pressed");
        SceneManager.LoadScene("main");

    }

    public void Tutorial()
    {
        Debug.Log("Tutorial pressed");
        SceneManager.LoadScene("Tutorial");
    }

    public void Settings()
    {
        Debug.Log("Settings pressed");
        SceneManager.LoadScene("MainMenu");
    }

    public void ExitGame()
    {
        Debug.Log("Exit pressed");
        Application.Quit();

    }
    public void SetUsername()
    {
        photonNickname = nickname.text.ToString();
        print(photonNickname);
    }

    // Start is called before the first frame update


    // Update is called once per frame
    void Update()
    {
        
    }
}
