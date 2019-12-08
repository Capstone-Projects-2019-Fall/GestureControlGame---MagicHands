using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MechMenuOpenAndBack : MonoBehaviour
{
    public GameObject ControlSelectionMenu;
    public GameObject MechMenu;


    // Update is called once per frame
    public void OpenMechMenu()
    {
        ControlSelectionMenu.SetActive(false);
            MechMenu.SetActive(true);
    }
    void OnMouseDown()
    {
        Debug.Log("clicked");
        if (ControlSelectionMenu.activeSelf)
        {
            ControlSelectionMenu.SetActive(false);
            MechMenu.SetActive(true);
        }
        else
        {
            ControlSelectionMenu.SetActive(true);
            MechMenu.SetActive(false);
        }
    }
}
