﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartButton : MonoBehaviour
{
   
    public GameObject PauseAtStart;
    // Start is called before the first frame update
    void Start()
    {
        PauseAtStart.SetActive(true);
        Time.timeScale = 0f;
    }

    public void ButtonPressed()
    {
        GameManager.started = true;
        PauseAtStart.SetActive(false);
        Time.timeScale = 1f;
    }
}
