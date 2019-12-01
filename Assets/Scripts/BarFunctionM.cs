using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BarFunctionM : MonoBehaviour
{
    
    public Image speed;
    public Image hp;
    public GameObject powerUp;
    public GameObject player;
    private float currentSpeed;
    private float currentHP;
    private float maxHP;
    private PhotonView PV;
    // Start is called before the first frame update
    void Awake()
    {
        PV = GetComponent<PhotonView>();
        speed.fillAmount = 0.5f;
        hp.fillAmount = 1.0f;
        powerUp.SetActive(false);
        if (PV.IsMine)
        {
            
            currentSpeed = GetComponent<PlayerControllerM>().GetSpeed();
            maxHP = GetComponent<PlayerControllerM>().GetHealth();
            currentHP = GetComponent<PlayerControllerM>().GetHealth();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (PV.IsMine)
        {
            SpeedCheck();
            HealthCheck();
            PowerUpCheck();
        }
    }
    public void SpeedCheck()
    {
        float newSpeed = GetComponent<PlayerControllerM>().GetSpeed();
        if (currentSpeed < newSpeed)
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
        float newHP = GetComponent<PlayerControllerM>().GetHealth();

        if (currentHP != newHP)
        {

            currentHP = newHP;
            Debug.Log(currentHP / maxHP);
            hp.fillAmount = currentHP / maxHP;

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
