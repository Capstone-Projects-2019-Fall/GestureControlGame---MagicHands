using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletSelection : MonoBehaviour
{
    public static BulletSelection Instance2;
    public static bool bulletChoice1; //Egg
    public static bool bulletChoice2; //Plasma
    public static bool bulletChoice3; //Flame
    // Start is called before the first frame update
    void Awake()
    {
        bulletChoice1 = false;
        bulletChoice2 = false;
        bulletChoice3 = false;
        if (Instance2 == null)
        {
            DontDestroyOnLoad(gameObject);
            Instance2 = this;
        }
        else if (Instance2 != this)
        {
            Destroy(gameObject);
        }
    }
    /*public void Start()
    {
        bulletChoice1 = false;
        bulletChoice2 = false;
        bulletChoice3 = false;
    }*/

    public void Egg()
    {
        bulletChoice1 = true;
        bulletChoice2 = false;
        bulletChoice3 = false;
    }
    public void Plasma()
    {
        bulletChoice2 = true;
        bulletChoice1 = false;
        bulletChoice3 = false;
    }
    public void Flame()
    {
        bulletChoice3 = true;
        bulletChoice2 = false;
        bulletChoice1 = false;
    }
    // Update is called once per frame
    void Update()
    {
        Debug.Log("1: " + bulletChoice1 + ",2: " + bulletChoice2 + ",3: " + bulletChoice3);
    }
}
