using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LapSystem : MonoBehaviour
{
    int counter;
    bool isWait;
    // Start is called before the first frame update
    void Start()
    {
        counter = 0;
        isWait = false;
    }

    // Update is called once per frame
    void Update()
    {

        /*psedocode
         * need some sort of ring counter that will count each time pass through a ring
         * 
         * */
        if (counter == 3)
        {
            QuitGame();
        }
    }
    public void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            if (isWait == false)
            {
                counter++;
                StartCoroutine(WaitTime());
            }
        }

    }
    public void QuitGame()
    {
        WinLose.isWin = true;
        Time.timeScale = 1f;
            
            SceneManager.LoadScene("WinLose");
        
    }
    IEnumerator WaitTime()
    {
        isWait = true;
        yield return new WaitForSeconds(3);
        isWait = false;
        

    }
}
