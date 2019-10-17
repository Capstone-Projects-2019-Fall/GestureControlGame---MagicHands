using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ControlSelectionMenu : MonoBehaviour
{
    public void UseMouse()
    {
        GameManager.UpdateController(motionControl: false);
        GameManager.UpdateInMenu(isInMenu: false);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void UsePredefinedMotionControl()
    {
        GameManager.UpdateController(motionControl: true);
        GameManager.UpdateInMenu(isInMenu: false);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }
}
