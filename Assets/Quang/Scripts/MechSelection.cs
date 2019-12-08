using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MechSelection : MonoBehaviour
{
    public static MechSelection Instance;
    public static bool mechChoice1; //airplane
    public static bool mechChoice2; //stealth bomber
    public static bool mechChoice3; //mech
    // Start is called before the first frame update
    void Awake()
    {
        mechChoice1 = false;
        mechChoice2 = false;
        mechChoice3 = false;
        if (Instance == null)
        {
            DontDestroyOnLoad(gameObject);
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }
    /*public void Start()
    {
        mechChoice1 = false;
        mechChoice2 = false;
        mechChoice3 = false;
    }*/

    public void AirPlane()
    {
        mechChoice1 = true;
        mechChoice2 = false;
        mechChoice3 = false;
    }
    public void StealthBomber()
    {
        mechChoice2 = true;
        mechChoice1 = false;
        mechChoice3 = false;
    }
    public void Mecha()
    {
        mechChoice3 = true;
        mechChoice2 = false;
        mechChoice1 = false;
    }
    // Update is called once per frame
    void Update()
    {
        Debug.Log("1: " + mechChoice1 + ",2: " + mechChoice2 + ",3: " + mechChoice3);
    }
}
