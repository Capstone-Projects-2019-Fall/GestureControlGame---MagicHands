using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GameManager
{
    public static bool useMotionControl = false;
    public static ControllerQuang controller = new MouseControl();
    public static bool InMenu = false;
    public static bool started = false;

    public static void UpdateInMenu(bool isInMenu)
    {
        InMenu = isInMenu;
        controller.InMenu = InMenu;
    }

    public static void UpdateController(bool motionControl)
    {
        useMotionControl = motionControl;
        if (useMotionControl)
        {
            controller = new MotionControl();
        }
        else
        {
            controller = new MouseControl();
        }
    }
}
