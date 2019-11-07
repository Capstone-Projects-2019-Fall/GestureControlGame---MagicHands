using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GameManager
{
    public static bool useMotionControl = false;
    public static ControllerQuang controller = new MouseControl();
    public static bool UseIntuitiveController = false;
    public static bool InMenu = false;
    public static bool started = false;
    public static bool customMotionControl = false;

    public static void UpdateInMenu(bool isInMenu)
    {
        InMenu = isInMenu;
        controller.InMenu = InMenu;
    }

    public static void UpdateController(bool motionControl, bool customMotionControl)
    {
        useMotionControl = motionControl;
        GameManager.customMotionControl = customMotionControl;

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
