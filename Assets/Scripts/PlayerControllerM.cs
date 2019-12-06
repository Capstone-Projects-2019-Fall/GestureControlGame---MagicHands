using UnityEngine;
using EZCameraShake;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Windows.Speech;
using Photon.Pun;
using UnityEngine.SceneManagement;
using System.IO;

public class PlayerControllerM : MonoBehaviour
{
    //this is test message

    private PhotonView PV;
    private CharacterController myCC;

    [SerializeField]
    public GameObject projectilePrefab;
    [SerializeField]
    Camera avatarCamera;
    private Rigidbody rb;
    public ParticleSystem warp;
    public ParticleSystem flame;
    public float speed;
    
    public float rotationSpeed;
    private int count;
    //Controller controller;
    //public GameObject gameManager;
    //public GameObject player;
    private int i = 0;
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
    private PauseMenu pauseMenu;
    void Start()
    {
        PV = GetComponent<PhotonView>();
        myCC = GetComponent<CharacterController>();
        //avatarCamera = Camera.main;
       /* if (!PV.IsMine)
        {
            Destroy(avatarCamera);
        }*/

        currentInvincibleTimer = maxInvincibleTimer;
        flame.Clear();
        flame.Stop();
        warp.Clear();
        warp.Stop();
      //  rb = GetComponent<Rigidbody>();
        count = 0;
        PrevPos = transform.position;
        NewPos = transform.position;
        speedMultiplier = 1f;
        if(PV.IsMine)
        {
            keyActs.Add("zoom", Zoom);
            keyActs.Add("shoot", Shoot);

            recognizer = new KeywordRecognizer(keyActs.Keys.ToArray());
            recognizer.OnPhraseRecognized += OnPhraseRecognized;
            recognizer.Start();
        }
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
    void Shoot()
    {  
        Launch();
    }

    public void StartSpeedBoost()
    {
        StartCoroutine(SpeedBoost());
    }
    void Update()
    {   
        if (PV.IsMine)
        {
            if (!GameManager.started)
            {
                return;
            }
            Vector3 rotate = GameManager.controller.GetRotation();
            float speedMag = GameManager.controller.GetSpeed();

            rotate = rotate * Time.deltaTime * rotationSpeed;

            transform.Rotate(-rotate.y, rotate.x, rotate.z, Space.Self);
            transform.position += speedMultiplier * speed * transform.forward * Time.deltaTime * speedMag;
            myCC. Move(speedMultiplier * speed * this.transform.forward * Time.deltaTime * speedMag);


            if (Input.GetKey(KeyCode.V))
            {
                CameraShaker.Instance.ShakeOnce(10f, 10f, .5f, 1.5f);
            }
            if (Input.GetKey(KeyCode.C))
            {
                Launch();
            }
            if (Input.GetKey(KeyCode.Space))
            {
                if (speedBoostState == true)
                {
                    StartCoroutine(SpeedBoost());
                }
            }
            if (Input.GetKey(KeyCode.P))
            {
                speedMultiplier = 0f;
            }
            if (Input.GetKey(KeyCode.S))
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
            if (Input.GetKeyDown("space"))
            {
                if (speedBoostState == true)
                {
                    StartCoroutine(SpeedBoost());
                }
            }

            /*psedocode
             * need some sort of ring counter that will count each time pass through a ring
             * so that once a ring is travelled through, it won't count anymore until a full ring circle is completed
             * */

        }
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

    void Launch()
    {
        Vector3 oldV = transform.forward;
        Vector3 newV = new Vector3(transform.position.x, transform.position.y + 0.5f,
                transform.position.z);
        //GameObject projectileObject = Instantiate(projectilePrefab, newV, Quaternion.LookRotation(newV, Vector3.forward));
        GameObject projectileObject = PhotonNetwork.Instantiate(Path.Combine("Prefabs", "projectileM"),
            newV, Quaternion.LookRotation(newV, Vector3.forward), 0);
        Projectile projectile = projectileObject.GetComponent<Projectile>();
        projectile.Launch(oldV, 20);
        //projectileObject.GetComponent<Projectile>().rigidbody.velocity=newV.TransformDirection(transform.forward * 20);
    }

    [PunRPC]
    private void RPC_CreateBullet()
    {
        PhotonNetwork.Instantiate(Path.Combine("Prefabs", "projectileM"),
            transform.position, Quaternion.identity, 0);
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


}