using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;



public class ControlSelectionMenu : MonoBehaviour
{

    public void Start()
    {
        Debug.Log("started");
        Debug.Log(Application.dataPath);
    }

    public void UseMouseControl()
    {
        GameManager.UpdateController(motionControl: false);
        GameManager.UpdateInMenu(isInMenu: false);
        SceneManager.LoadScene("main");
    }

    public void UsePredefinedMotionControl()
    {
        GameManager.UpdateController(motionControl: true);
        GameManager.UpdateInMenu(isInMenu: false);

        // Create a process
        System.Diagnostics.Process process = new System.Diagnostics.Process();

        // Set the StartInfo of process
        string exeDir = Application.dataPath + "\\Quang\\python_scripts\\executable\\dist";
        process.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Normal;
        process.StartInfo.FileName = exeDir + "\\quang_tracker.exe";
        process.StartInfo.Arguments = "-F " + exeDir + "\\haarcascade_frontalface_default.xml";

        // Start the process
        process.Start();
        while (true)
        {
            if (GameManager.controller.GetSpeed() > 0f)
            {
                SceneManager.LoadScene("main");
                break;
            }
        }
    }

    public void UseCustomMotionControl()
    {
        
    }
}
