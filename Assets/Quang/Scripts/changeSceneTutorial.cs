using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class changeSceneTutorial : MonoBehaviour
{
    public GameObject secondImage;
    public GameObject firstImage;
    // Start is called before the first frame update
    void Start()
    {
        secondImage.SetActive(false);
        firstImage.SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0)) {
            if (firstImage.activeSelf)
            {
                secondImage.SetActive(true);
                firstImage.SetActive(false);
            }
            else {
                secondImage.SetActive(false);
                firstImage.SetActive(true);
            }
        }
           

    }
}
