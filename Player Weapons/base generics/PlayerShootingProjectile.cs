using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerShootingProjectile : MonoBehaviour
{

    public float fireRate = 0.75f;
    float cooldown = 0;
    public float damage = 25f;
    public Transform barrel;
    public Transform ProjectileBarrel;
    public Transform viewModelBarrel;
    FXManager fxManager;
    Animator anim;
    public GameObject viewModel;
    public GameObject projectile;
    Transform barrelYcomponent;
    public float volume = 1f;
    public float projectileVelocity;
    public int weaponID = 0;
    CanFire CF;
    // Update is called once per frame
    void Start()
    {
        fxManager = GameObject.FindObjectOfType<FXManager>();
        Transform animParent = this.transform.Find("PlayerModel");
        anim = animParent.gameObject.GetComponent<Animator>();
        barrelYcomponent = this.transform.Find("BarrelPosition");
        CF = this.GetComponent<CanFire>();
    }
    void Update()
    {
        cooldown -= Time.deltaTime;

        if (Input.GetButtonDown("Fire1") && CF.CanShoot == true) 
        {
            // Player wants to shoot...so. Shoot.

            Fire();
        }
        if (cooldown <= 0.2f)
        {
            anim.SetBool("Fire", false);  //Network fire animation terminated here
        }
        // barrelPos = barrel.GetComponent<Transform>().position;
        //    Debug.Log(barrelPos.y);

    }

    void Fire()
    {
        if (cooldown > 0)
        {
            return;
        }

        Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
        Transform hitTransform;
        Vector3 hitPoint;

        hitTransform = FindClosestHitObject(ray, out hitPoint);

        if (hitTransform != null)
        {

            Debug.Log(hitTransform);
        }
        if (hitTransform == null)
        {
            Debug.Log("no real hitpoint");
            hitPoint = Camera.main.transform.position + (Camera.main.transform.forward * 150f);
        }



        viewModel.GetComponent<Animation>().Play();
        DoGunFx(hitPoint);


        cooldown = fireRate;

    }

    void DoGunFx(Vector3 hitPoint)
    {
        Vector3 startPos = new Vector3(barrel.position.x, barrel.position.y, barrel.position.z);
        ProjectileBarrel.transform.position = startPos;
        ProjectileBarrel.transform.LookAt(hitPoint);
        if (anim.GetFloat("Speed") < 0)
        {
            startPos += (ProjectileBarrel.transform.forward);
        }

        GetComponent<PhotonView>().RPC("ClientProjectile", PhotonTargets.Others, hitPoint); 

        //Local "real" projectile
        RealProjectile(hitPoint);
        this.GetComponent<PhotonView>().RPC("ShootAnimation", PhotonTargets.All);

        //Animations:
        //  this.GetComponent<PhotonView>().RPC("SetFireTrigger", PhotonTargets.All);
        //End animations



    }
    [PunRPC]
    void ClientProjectile(Vector3 hitPoint)
    { //MUST ONLY SEND VECTORS

        ProjectileBarrel.transform.position = barrel.position;
        ProjectileBarrel.transform.LookAt(hitPoint); //REAL BARREL
        GameObject proj = Instantiate(projectile, barrel.position, ProjectileBarrel.rotation);
        Debug.Log("Fired a projectile RPC");
        proj.GetComponent<ProjectileScript>().volume = volume;
        proj.GetComponent<Rigidbody>().AddForce(proj.transform.forward * projectileVelocity);

    }
    void RealProjectile(Vector3 hitPoint)
    {
        viewModelBarrel.transform.LookAt(hitPoint);
        GameObject proj = Instantiate(projectile, viewModelBarrel.position, viewModelBarrel.rotation);
        ProjectileScript projScript = proj.GetComponent<ProjectileScript>();
        projScript.DealsDamage = true;
        projScript.damage = damage;

        projScript.volume = volume;
        projScript.projectileTeamID = this.GetComponent<TeamMember>().teamID;

        proj.GetComponent<Rigidbody>().AddForce(proj.transform.forward * projectileVelocity);
    }


    [PunRPC]
    void SetFireTrigger()   //Network fire animation occurs here
    {
        if (anim != null)
        {
            anim.SetBool("Fire", true);
        }

    }
    Transform FindClosestHitObject(Ray ray, out Vector3 hitPoint)
    {

        RaycastHit[] hits = Physics.RaycastAll(ray);

        Transform closestHit = null;
        float distance = 0;
        hitPoint = Vector3.zero;

        foreach (RaycastHit hit in hits)
        {
            if (hit.transform != this.transform && (closestHit == null || hit.distance < distance))
            {
                // We have hit something that is:
                // a) not us
                // b) the first thing we hit (that is not us)
                // c) or, if not b, is at least closer than the previous closest thing

                closestHit = hit.transform;
                distance = hit.distance;
                hitPoint = hit.point;
            }
        }

        // closestHit is now either still null (i.e. we hit nothing) OR it contains the closest thing that is a valid thing to hit

        return closestHit;

    }
}