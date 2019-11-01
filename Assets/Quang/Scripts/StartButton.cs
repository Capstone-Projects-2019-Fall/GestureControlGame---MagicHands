using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Windows.Speech;
using System.Linq;
using System;

public class StartButton : MonoBehaviour
{
   
    public GameObject PauseAtStart;
    
    private Dictionary<string, Action> keyActs = new Dictionary<string, Action>();

    private KeywordRecognizer recognizer;
    // Start is called before the first frame update
    void Start()
    {   
        keyActs.Add("start", ButtonPressed);

        recognizer = new KeywordRecognizer(keyActs.Keys.ToArray());
        recognizer.OnPhraseRecognized += OnPhraseRecognized;
        recognizer.Start();
        PauseAtStart.SetActive(true);
        Time.timeScale = 0f;
    }

    void OnPhraseRecognized(PhraseRecognizedEventArgs args)
    {
        Debug.Log("Command: " + args.text);
        keyActs[args.text].Invoke();
    }

    public void ButtonPressed()
    {
        GameManager.started = true;
        PauseAtStart.SetActive(false);
        Time.timeScale = 1f;
    }
}
