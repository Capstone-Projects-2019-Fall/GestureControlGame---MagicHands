using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{

    public void Multyplayer()
    {
        Debug.Log("Multyplayer pressed");
        

    }

    public void SinglePlayer()
    {
        Debug.Log("Single player pressed");
        SceneManager.LoadScene("Main");

    }


    public void ExitGame()
    {
        Debug.Log("Exit pressed");
        Application.Quit();

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
