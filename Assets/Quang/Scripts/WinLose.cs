using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class WinLose : MonoBehaviour
{
    public static bool isWin;
    
    public GameObject winImage;
    public GameObject loseImage;
    // Start is called before the first frame update
    void Start()
    {
        isWin = false;
    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log(isWin);
        if (isWin == true)
        {
            winImage.SetActive(true);
            loseImage.SetActive(false);

        }
        else
        {
            winImage.SetActive(false);
            loseImage.SetActive(true);
        }
        if (Input.GetMouseButtonDown(0))
        {
            SceneManager.LoadScene("Menu2");
        }
    }
}
