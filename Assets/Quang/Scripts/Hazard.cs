using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Hazard : MonoBehaviour
{
    public GameObject player;
    public RawImage image;
    public Canvas canvas;
    // Start is called before the first frame update
    public void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {       
                canvas.gameObject.SetActive(true);
                Color c = image.color;
                image.color = c;
                StartCoroutine(Damage());
                player.GetComponent<PlayerController>().SetHealth(10.0f);
        }
    }
    public IEnumerator Damage()
    {
        yield return new WaitForSeconds(0.5f);
        canvas.gameObject.SetActive(false);
    }
}
