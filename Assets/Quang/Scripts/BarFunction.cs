using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BarFunction : MonoBehaviour
{
    public Image speed;
    public Image hp;
    public GameObject powerUp;
    public GameObject player;
    private float currentSpeed;
    private float currentHP;
    private float maxHP;
    // Start is called before the first frame update
    void Start()
    {
        speed.fillAmount = 0.5f;
        hp.fillAmount = 1.0f;
        powerUp.SetActive(false);
        currentSpeed = player.GetComponent<PlayerControllerM>().GetSpeed();
        maxHP = player.GetComponent<PlayerControllerM>().GetHealth();
        currentHP = player.GetComponent < PlayerControllerM>().GetHealth();
    }

    // Update is called once per frame
    void Update()
    {
        
        SpeedCheck();
        HealthCheck();
        PowerUpCheck();
    }
    public void SpeedCheck()
    {
        float newSpeed=player.GetComponent<PlayerControllerM>().GetSpeed();
        if (currentSpeed< newSpeed)
        {
            speed.fillAmount = 1.0f;
        }
        else
        {
            speed.fillAmount = 0.5f;
        }
    }
    public void HealthCheck()
    {
        float newHP= player.GetComponent<PlayerControllerM>().GetHealth();
        
        if (currentHP != newHP)
        {
            
            currentHP = newHP;
            Debug.Log(currentHP / maxHP);
            hp.fillAmount = currentHP/maxHP;
            
        }

    }
    public void PowerUpCheck()
    {
        if (PlayerControllerM.speedBoostState == true)
        {
            powerUp.SetActive(true);
        }
        else
        {
            powerUp.SetActive(false);
        }
    }
}
