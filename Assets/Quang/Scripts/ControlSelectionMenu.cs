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
        GameManager.UpdateController(false, false);
        GameManager.UpdateInMenu(isInMenu: false);
        UnityEngine.SceneManagement.SceneManager.LoadScene("main");
    }

    public void UsePredefinedMotionControl()
    {
        GameManager.UpdateController(true, false);
        GameManager.UpdateInMenu(isInMenu: false);

        // Create a process
        System.Diagnostics.Process process = new System.Diagnostics.Process();

        // Set the StartInfo of process
        string exeDir = Application.dataPath + "/Quang/python_scripts/executable/dist";
        process.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Normal;
        process.StartInfo.FileName = exeDir + "/quang_tracker.exe";
        Debug.Log(exeDir);
        string argument = "-F \"" + exeDir + "/haarcascade_frontalface_default.xml\" -H 7 -C 0";
        Debug.Log(argument);
        process.StartInfo.Arguments = argument;

        // Start the process
        process.Start();
        SceneManager.LoadScene("main");
    }

    public void UseCustomMotionControl()
    {
        Debug.Log("use custom motion control");
        GameManager.UpdateController(true, true);
        GameManager.UpdateInMenu(isInMenu: false);

        // Create a process
        System.Diagnostics.Process process = new System.Diagnostics.Process();

        // Set the StartInfo of process
        string exeDir = Application.dataPath + "/Quang/python_scripts/executable/dist";
        process.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Normal;
        process.StartInfo.FileName = exeDir + "/quang_tracker.exe";
        Debug.Log(exeDir);
        string argument = "-F \"" + exeDir + "/haarcascade_frontalface_default.xml\" -H 7 -C 1 -L 1 -O \""+ Application.dataPath + "/Quang/python_scripts\"";
        Debug.Log(argument);
        process.StartInfo.Arguments = argument;

        // Start the process
        process.Start();
        SceneManager.LoadScene("main");
    }

    public void UseSavedCustomMotionControl()
    {
        GameManager.UpdateController(true, true);
        GameManager.UpdateInMenu(isInMenu: false);

        // Create a process
        System.Diagnostics.Process process = new System.Diagnostics.Process();

        // Set the StartInfo of process
        string exeDir = Application.dataPath + "/Quang/python_scripts/executable/dist";
        process.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Normal;
        process.StartInfo.FileName = exeDir + "/quang_tracker.exe";
        Debug.Log(exeDir);
        string argument = "-F \"" + exeDir + "/haarcascade_frontalface_default.xml\" -H 7 -C 1 -L 1 -O \"" + Application.dataPath + "/Quang/python_scripts\"";
        Debug.Log(argument);
        process.StartInfo.Arguments = argument;

        // Start the process
        process.Start();
        SceneManager.LoadScene("main");
    }
}
