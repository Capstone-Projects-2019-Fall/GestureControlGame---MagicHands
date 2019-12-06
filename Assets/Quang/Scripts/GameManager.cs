using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

public static class GameManager
{
    public static bool useMotionControl = false;
    public static ControllerQuang controller = new MouseControl();
    public static bool UseIntuitiveController = false;
    public static bool InMenu = false;
    public static bool started = false;
    public static bool customMotionControl = false;
    static Thread thread;
    public static UdpClient client;
    public static int port;

    public static void UpdateInMenu(bool isInMenu)
    {
        InMenu = isInMenu;
        controller.InMenu = InMenu;
    }

    public static void UpdateController(bool motionControl, bool customMotionControl)
    {
        controller.Destroy();

        useMotionControl = motionControl;
        GameManager.customMotionControl = customMotionControl;

        thread = new Thread(new ThreadStart(ThreadJob));
        thread.IsBackground = true;
        thread.Start();
    

        

        //if (useMotionControl)
        //{
        //    if (UseIntuitiveController) controller = new MotionControlIntuitive();
        //    else
        //    {
        //        if (customMotionControl)
        //        {
        //            controller = new CustomMotionController();
        //        }
        //        else
        //        {
        //            controller = new MotionControl();
        //        }
        //    }
        //}
        //else
        //{
        //    controller = new MouseControl();
        //}
    }

    static void ThreadJob()
    {
        //while (!controller.GetPortDestroyed())
        //{
        //    Debug.Log("port not yet destroyed");
        //}
        if (useMotionControl)
        {
            if (UseIntuitiveController) controller = new MotionControlIntuitive();
            else
            {
                if (customMotionControl)
                {
                    controller = new CustomMotionController();
                }
                else
                {
                    controller = new MotionControl();
                }
            }
        }
        else
        {
            controller = new MouseControl();
        }
    }
}
