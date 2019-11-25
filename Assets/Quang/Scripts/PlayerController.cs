using UnityEngine;
using EZCameraShake;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Windows.Speech;
using Photon.Pun;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    //this is test message
    private PhotonView PV;
    private Rigidbody rb;
    public ParticleSystem warp;
    public ParticleSystem flame;
    public float speed;
    public GameObject projectilePrefab;
    public float rotationSpeed;
    private int count;
    public GameObject[] planeType;
    //Controller controller;
    //public GameObject gameManager;
    public GameObject player;
    public static bool speedBoostState = false;
    //GameManager gameManagerCode;
    Vector3 PrevPos;
    Vector3 NewPos;
    Vector3 ObjVelocity;
    float speedMultiplier;
    public float hp;
    private float currentInvincibleTimer;
    public static bool isInvincible = false;
    const float maxInvincibleTimer = 3.0f;
    private Dictionary<string, Action> keyActs = new Dictionary<string, Action>();

    private KeywordRecognizer recognizer;
    // Var needed for color manipulation

    void Start()
    {
        PV = GetComponent<PhotonView>();
        currentInvincibleTimer = maxInvincibleTimer;
        flame.Clear();
        flame.Stop();
        warp.Clear();
        warp.Stop();
        rb = GetComponent<Rigidbody>();
        count = 0;
        PrevPos = transform.position;
        NewPos = transform.position;
        speedMultiplier = 1f;
        keyActs.Add("zoom", Zoom);

        recognizer = new KeywordRecognizer(keyActs.Keys.ToArray());
        recognizer.OnPhraseRecognized += OnPhraseRecognized;
        recognizer.Start();
        PlaneTypeCheck();
        //gameManagerCode = gameManager.GetComponent<GameManager>();
    }
    //void FixedUpdate()
    //{
    //    float xMove = Input.GetAxis("Horizontal");
    //    float zMove = Input.GetAxis("Vertical");

    //    Vector3 move = new Vector3(xMove, 0.0f, zMove);
    //    //Vector3 move = new Vector3(rightForce, 0.0f, upForce).normalized;

    //    rb.AddForce(move * speed);

    //}
    void OnPhraseRecognized(PhraseRecognizedEventArgs args)
    {
        Debug.Log("Command: " + args.text);
        keyActs[args.text].Invoke();
    }
    void Zoom()
    {
        if (speedBoostState == true)
        {
            StartCoroutine(SpeedBoost());
        }
        else
        {
            Debug.Log("No powerup");
        }
    }
    void Update()
    {
        //if (PV.IsMine)
        //{
        if (!GameManager.started)
        {
            return;
        }
        Vector3 rotate = GameManager.controller.GetRotation();
        float speedMag = GameManager.controller.GetSpeed();

        rotate = rotate * Time.deltaTime * rotationSpeed;

        player.transform.Rotate(-rotate.y, rotate.x, rotate.z, Space.Self);
        player.transform.position += speedMultiplier * speed * player.transform.forward * Time.deltaTime * speedMag;


        if (Input.GetKeyDown("v"))
        {
            CameraShaker.Instance.ShakeOnce(10f, 10f, .5f, 1.5f);
        }
        if (Input.GetKeyDown("c"))
        {
            Launch();
        }
        if (Input.GetKeyDown("space"))
        {
            if (speedBoostState == true)
            {
                StartCoroutine(SpeedBoost());
            }
        }
        if (Input.GetKeyDown("p"))
        {
            speedMultiplier = 0f;
        }
        if (Input.GetKeyUp("p"))
        {
            speedMultiplier = 1f;
        }
        NewPos = transform.position;  // each frame track the new position
        //Debug.Log("delta time:" + Time.deltaTime);
        //Debug.Log("fixed delta time:" + Time.fixedDeltaTime);
        ObjVelocity = (NewPos - PrevPos) / Time.deltaTime;  // velocity = dist/time
        PrevPos = NewPos;  // update position for next frame calculation


        //}
        if (isInvincible == true)
        {

            currentInvincibleTimer -= Time.deltaTime;
            if (currentInvincibleTimer <= 0)
            {
                isInvincible = false;
                currentInvincibleTimer = maxInvincibleTimer;
            }
        }
        if (hp <= 0)
        {
            Time.timeScale = 1f;
            WinLose.isWin = false;
            SceneManager.LoadScene("WinLose");
        }

        /*psedocode
         * need some sort of ring counter that will count each time pass through a ring
         * so that once a ring is travelled through, it won't count anymore until a full ring circle is completed
         * */
         
    }
    IEnumerator SpeedBoost()
    {
        warp.Play();
        flame.Play();
        float oldspeed = speed;
        float newspeed = speed * 2;
        speed = newspeed;
        yield return new WaitForSeconds(3);
        warp.Stop();
        flame.Stop();
        speed = oldspeed;
        speedBoostState = false;

    }
    public float GetHealth()
    {
        return hp;
    }
    public float GetSpeed()
    {
        return speed;
    }

    public void SetHealth(float hpnew)
    {
        if (isInvincible == false)
        {
            hp = hp - hpnew;
            isInvincible = true;
        }
    }
    void Launch()
    {
        Vector3 oldV = transform.forward;
        Vector3 newV = new Vector3(transform.position.x, transform.position.y + 0.5f,
                transform.position.z);
        GameObject projectileObject = Instantiate(projectilePrefab, newV, Quaternion.LookRotation(newV, Vector3.forward));

        Projectile projectile = projectileObject.GetComponent<Projectile>();
        projectile.Launch(oldV, 20);
        //projectileObject.GetComponent<Projectile>().rigidbody.velocity=newV.TransformDirection(transform.forward * 20);
    }
    void SetPlaneType(int planeNum)
    {
        planeType[planeNum].SetActive(true);
        for(int i = 0; i < planeType.Length; i++)
        {
            if (i != planeNum)
            {
                planeType[i].SetActive(false);
            }
        }
    }
    void PlaneTypeCheck()
    {
        int counter = 0;
        Debug.Log("this is planeType.Length " + planeType.Length);
        for (int i = 0; i < planeType.Length; i++)
        {
            if (!planeType[i].activeSelf)
            {
                counter++;
            }
        }
        Debug.Log("this is counter" + counter);
        if (counter== planeType.Length)
        {
            planeType[0].SetActive(true);
        }
    }

}