using System.Collections.Generic;
using System.Linq;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Windows.Speech;


public class VoiceController : MonoBehaviour
{
    // Voice command vars
    private Dictionary<string, Action> keyActs = new Dictionary<string, Action>();
    
    private KeywordRecognizer recognizer;
    // Var needed for color manipulation
   
    public float speed;
    
    private MeshRenderer cubeRend;

    private Rigidbody rb;


    void Start()
    {
        rb = GetComponent<Rigidbody>();

        cubeRend = GetComponent<MeshRenderer>();
        
        //Voice commands for changing color
        keyActs.Add("red", Red);
        keyActs.Add("green", Green);
        keyActs.Add("blue", Blue);
        keyActs.Add("white", White);
        keyActs.Add("zoom", Zoom);
    
        recognizer = new KeywordRecognizer(keyActs.Keys.ToArray());
        recognizer.OnPhraseRecognized += OnPhraseRecognized;
        recognizer.Start();
        
    }
    void FixedUpdate()
    {
        float xMove = Input.GetAxis("Horizontal");
        float zMove = Input.GetAxis("Vertical");
    
        Vector3 move = new Vector3(xMove, 0.0f, zMove);
        //Vector3 move = new Vector3(rightForce, 0.0f, upForce).normalized;

        rb.AddForce(move * speed);
    }

    void OnPhraseRecognized(PhraseRecognizedEventArgs args)
    {
        Debug.Log("Command: " + args.text);
        keyActs[args.text].Invoke();
    }
    void Red()
    {
        cubeRend.material.SetColor("_Color", Color.red);
    }
    void Green()
    {
        cubeRend.material.SetColor("_Color", Color.green);
    }
    void Blue()
    {
        cubeRend.material.SetColor("_Color", Color.blue);
       
    }
    void White()
    {
        cubeRend.material.SetColor("_Color", Color.white);
    }
    void Zoom()
    {
        if (PlayerController.speedBoostState == true)
        {
            StartCoroutine(SpeedBoost());
        }
        else
        {
            Debug.Log("No powerup");
        }
    }
    IEnumerator SpeedBoost()
    {
        
        float oldspeed = speed;
        float newspeed = speed * 2;
        speed = newspeed;
        yield return new WaitForSeconds(3);
        speed = oldspeed;
        PlayerController.speedBoostState=false;

    }
}
