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

        cubeRend = GetComponent<MeshRenderer>();
        
        //Voice commands for changing color
        keyActs.Add("red", Red);
        keyActs.Add("green", Green);
        keyActs.Add("blue", Blue);
        keyActs.Add("white", White);
    
        recognizer = new KeywordRecognizer(keyActs.Keys.ToArray());
        recognizer.OnPhraseRecognized += OnPhraseRecognized;
        recognizer.Start();
        
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

}
