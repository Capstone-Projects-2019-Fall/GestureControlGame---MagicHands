using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectilePlasma : MonoBehaviour
{
    public float damage= 10.0f;
    public float range= 100.0f;
    public Camera cam;

    public GameObject projectilePrefab;
    public ParticleSystem muzzleFlash;
    public GameObject beamRay;
    public GameObject impactEffect;
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown("m"))
        {
            ShootPlasma();
        }
        if (Input.GetKeyDown("n"))
        {
            ShootBeam();
        }
        if (Input.GetKeyDown("c"))
        {
            if (BulletSelection.bulletChoice3 == true)
            {
                Launch();
            }
            else if (BulletSelection.bulletChoice2 == true)
            {
                ShootPlasma();
            }
            else
            {
                ShootBeam();
            }
            
        }
    }
    void ShootPlasma()
    {
        muzzleFlash.Play();
        RaycastHit hit;
        if(Physics.Raycast(cam.transform.position, cam.transform.forward, out hit, range)){
            Debug.Log(hit.transform.name);
            //Target target = hit.transform.GetComponent<Target>();
            GameObject imEf = Instantiate(impactEffect, hit.point, Quaternion.LookRotation(hit.normal));
            Destroy(imEf, 2.0f);
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
    void ShootBeam()
    {
        if (beamRay.activeSelf)
        {
            beamRay.SetActive(false);
        }
        else
        {
            beamRay.SetActive(true);
        }
        RaycastHit hit;
        if (Physics.Raycast(cam.transform.position, cam.transform.forward, out hit, range))
        {
            Debug.Log(hit.transform.name);
            //Target target = hit.transform.GetComponent<Target>();
            //GameObject imEf = Instantiate(impactEffect, hit.point, Quaternion.LookRotation(hit.normal));
            //Destroy(imEf, 2.0f);
        }

    }
}
