using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using UnityEngine;
using UnityEngine.SceneManagement;



public class ControlSelectionMenu : MonoBehaviour
{
    public GameObject ControlSelectionMenuObject;
    public GameObject MechMenu;
    public GameObject BulletMenu;
    string sceneToLoad = "Menu2";
    int port = 5065;
    public void Start()
    {
        Debug.Log("started");
        Debug.Log(Application.dataPath);
    }

    public void UseMouseControl()
    {
        GameManager.UpdateController(false, false);
        GameManager.UpdateInMenu(isInMenu: false);
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneToLoad);
    }

    public void UsePredefinedMotionControl()
    {
        GameManager.client = GetFreeClient();
        GameManager.port = port;
        GameManager.UpdateController(true, false);
        GameManager.UpdateInMenu(isInMenu: false);

        // Create a process
        System.Diagnostics.Process process = new System.Diagnostics.Process();

        // Set the StartInfo of process
        string exeDir = Application.dataPath + "/Quang/python_scripts/executable/dist";
        process.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Normal;
        process.StartInfo.FileName = exeDir + "/quang_tracker.exe";
        Debug.Log(exeDir);
        string argument = "-F \"" + exeDir + "/haarcascade_frontalface_default.xml\" -H 7 -C 0" + " --port " + port;
        Debug.Log(argument);
        process.StartInfo.Arguments = argument;

        // Start the process
        process.Start();
        SceneManager.LoadScene("Menu2");
    }

    public void UseCustomMotionControl()
    {
        GameManager.client = GetFreeClient();
        GameManager.port = port;
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
        string argument = "-F \"" + exeDir + "/haarcascade_frontalface_default.xml\" -H 7 -C 1 -L 0 -O \""+ Application.dataPath + "/Quang/python_scripts\"" + " --port " + port;
        Debug.Log(argument);
        process.StartInfo.Arguments = argument;

        // Start the process
        process.Start();
        SceneManager.LoadScene(sceneToLoad);
    }

    public void UseSavedCustomMotionControl()
    {
        GameManager.client = GetFreeClient();
        GameManager.port = port;
        GameManager.UpdateController(true, true);
        GameManager.UpdateInMenu(isInMenu: false);

        // Create a process
        System.Diagnostics.Process process = new System.Diagnostics.Process();

        // Set the StartInfo of process
        string exeDir = Application.dataPath + "/Quang/python_scripts/executable/dist";
        process.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Normal;
        process.StartInfo.FileName = exeDir + "/quang_tracker.exe";
        Debug.Log(exeDir);
        string argument = "-F \"" + exeDir + "/haarcascade_frontalface_default.xml\" -H 7 -C 1 -L 1 -O \"" + Application.dataPath + "/Quang/python_scripts\"" + " --port " + port;
        Debug.Log(argument);
        process.StartInfo.Arguments = argument;

        // Start the process
        process.Start();
        SceneManager.LoadScene(sceneToLoad);
    }
    public void OpenMechMenu()
    {
        ControlSelectionMenuObject.SetActive(false);
        MechMenu.SetActive(true);
        BulletMenu.SetActive(false);
    }
    public void OpenBuletMenu()
    {
        ControlSelectionMenuObject.SetActive(false);
        MechMenu.SetActive(false);
        BulletMenu.SetActive(true);
    }
    public void ReturnSelectionMenu()
    {
        ControlSelectionMenuObject.SetActive(true);
        MechMenu.SetActive(false);
        BulletMenu.SetActive(false);
    }

    UdpClient GetFreeClient()
    {
        UdpClient client;
        bool clientFound = false;
        while (!clientFound)
        {
            try
            {
                client = new UdpClient(port);
                Debug.Log("using port " + port);
                clientFound = true;
                return client;
            }
            catch (Exception e)
            {
                Debug.Log("port " + port + " not available");
                port += 1;
            }
        }
        return null;
    }
}
