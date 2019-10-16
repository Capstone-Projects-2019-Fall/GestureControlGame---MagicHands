using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public Controller controller;
    public bool useMotionControl;
    public bool InMenu;
    // Start is called before the first frame update
    void Start()
    {
        controller = InitializeController(useMotionControl);
    }

    // Update is called once per frame
    void Update()
    {
        controller.InMenu = InMenu;
    }

    public Controller GetController()
    {
        return controller;
    }

    private Controller InitializeController(bool useMotionControl)
    {
        if (useMotionControl)
        {
            return new MotionControl();
        }
        else
        {
            return new MouseControl();
        }
    }
}
