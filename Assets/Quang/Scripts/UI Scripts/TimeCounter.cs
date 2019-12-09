using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeCounter : MonoBehaviour
{
    // Start is called before the first frame update

    public float lastTime;
    public static TimeCounter timeCounter;
    void Start()
    {
        
    }

    private void Awake()
    {
        if (TimeCounter.timeCounter == null)
            timeCounter = this;
        DontDestroyOnLoad(this.gameObject);
        
    }

    

    // Update is called once per frame
    void Update()
    {
        
    }
}
