using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hellwailer : MonoBehaviour {



    SelfNetworkManagerII snm;

    public int ammo = 100;
    public int maxAmmo = 200;
    public float fireRate = 0.15f;
    public float secondaryfireRate = 0.75f;
    float cooldown = 0;
    public float damage = 10f;
    public string damageType = "Normal";
    public float explosiveDamage = 50f;
    public float explosiveRadius = 4f;
    public float explosiveForce = 0f;
    int teamID;
    public int secondaryFireCost = 20;
    public Transform barrel;
    public Transform ProjectileBarrel;
    public Transform viewModelBarrel;
    public Transform viewModelBarrel2;
    FXManager fxManager;
    Animator anim;
    public AnimationClip primaryFireClip;
    public AnimationClip secondaryFireClip;
    public GameObject viewModel;
    public GameObject projectile;
    public GameObject projectileSecondary;
    Transform barrelYcomponent;
    public float volume = 1f;
    public float volumeSecondary = 1f;
    public float projectileVelocity = 3000f;
    public float projectileVelocitySecondary = 1250f;
    public int weaponID = 0;
    CanFire CF;
    CPMPlayer movementController;
    int currentBarrel = 0;
    public bool localMuzzleflash = true;
    public bool localSecondary2dSoundImpact = true;
    public float scaleLimit = 1;
    public float z = 50;
    public float fireWalkSpeed = 5;
    public float fireSprintSpeed = 7;
    float normalWalkSpeed;
    float normalSprintSpeed;
    bool slow = false;


    ProjectileScript projScript;
    // Update is called once per frame
    void Start()
    {
        fxManager = GameObject.FindObjectOfType<FXManager>();
        Transform animParent = this.transform.Find("PlayerModel");
        anim = animParent.gameObject.GetComponent<Animator>();
        barrelYcomponent = this.transform.Find("BarrelPosition");
        CF = this.GetComponent<CanFire>();
        movementController = this.GetComponent<CPMPlayer>();
        normalWalkSpeed = movementController.moveSpeed;


        snm = GameObject.FindObjectOfType<SelfNetworkManagerII>();
        snm.UIClipAmmo.text = "";
        snm.UIReserveAmmo.text = ammo.ToString();

        teamID = this.GetComponent<TeamMember>().teamID;
    }






    void Update()
    {
        cooldown -= Time.deltaTime;
 
        if (Input.GetButton("Fire1") && CF.CanShoot == true && ammo > 0)
        {
            // Player wants to shoot...so. Shoot.

            Fire();


            if (slow == false)
            {
                SlowMovement();
                Invoke("Endslow", fireRate);
            }
        }
        if (Input.GetButton("Fire2") && CF.CanShoot == true && ammo >= secondaryFireCost)
        {
            // Player wants to shoot...so. Shoot.

            Fire2();

            snm.UIClipAmmo.text = "";
            snm.UIReserveAmmo.text = ammo.ToString();

        }
        // barrelPos = barrel.GetComponent<Transform>().position;
        //    Debug.Log(barrelPos.y);

    }
    void Fire2()
    {
        if (cooldown > 0)
        {
            return;
        }
        ammo -= secondaryFireCost;

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


        viewModel.GetComponent<Animation>().clip = secondaryFireClip;
        viewModel.GetComponent<Animation>().Play();
        DoGunFx(hitPoint, 2);


        cooldown = secondaryfireRate;
        this.GetComponent<PhotonView>().RPC("ShootAnimation", PhotonTargets.All);
    }
    [PunRPC]
    void ClientProjectile3(Vector3 hitPoint, Vector3 startPos)
    { //MUST ONLY SEND VECTORS
        ProjectileBarrel.transform.position = startPos;
        ProjectileBarrel.transform.LookAt(hitPoint); //REAL BARREL
        GameObject proj = Instantiate(projectile, startPos, ProjectileBarrel.rotation);
        proj.GetComponent<ProjectileScript>().volume = volume;
        proj.GetComponent<Rigidbody>().AddForce(proj.transform.forward * projectileVelocity);

    }


    public void UpdateHUD()
    {
        snm.UIClipAmmo.text = "";
        snm.UIReserveAmmo.text = ammo.ToString();
    }
    private void OnEnable()
    {
        snm.UIClipAmmo.text = "";
        snm.UIReserveAmmo.text = ammo.ToString();
 
    }
    void SlowMovement()
    {

            slow = true;
            movementController.moveSpeed = fireWalkSpeed;


    }
    void Endslow()
    {
        slow = false;
        movementController.moveSpeed = normalWalkSpeed;

    }
    void Fire()
    {
        if (cooldown > 0)
        {
            return;
        }
        if(currentBarrel == 0)
        {
            currentBarrel = 1;
        }
        else
        {
            currentBarrel = 0;
        }
        ammo -= 1;
        snm.UIClipAmmo.text = "";
        snm.UIReserveAmmo.text = ammo.ToString();

        Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
        RaycastHit hitInfo;

        Physics.Raycast(ray, out hitInfo);
            Vector3 hitPoint = hitInfo.point;
        Vector3 hitVector;
        Transform  hitTransform = FindClosestHitObject(ray, out hitVector);
        if (hitTransform == null)
        {
            hitPoint = Camera.main.transform.position + (Camera.main.transform.forward * 150f);
        }



        viewModel.GetComponent<Animation>().clip = primaryFireClip;
        viewModel.GetComponent<Animation>().Play();
        DoGunFx(hitPoint, 0);


        cooldown = fireRate;
        this.GetComponent<PhotonView>().RPC("ShootAnimation", PhotonTargets.All);
    }

    void DoGunFx(Vector3 hitPoint, int barrelNum)
    {
        Vector3 startPos = new Vector3(barrel.position.x, barrel.position.y, barrel.position.z);
        ProjectileBarrel.transform.position = startPos;
        ProjectileBarrel.transform.LookAt(hitPoint);
        if (anim.GetFloat("Speed") < 0)
        {
            startPos += (ProjectileBarrel.transform.forward);
        }

 

        //Local "real" projectile
        if(barrelNum == 0)
        {
            GetComponent<PhotonView>().RPC("ClientProjectile", PhotonTargets.Others, hitPoint);
            RealProjectile(hitPoint);
        }
        if (barrelNum == 1)
        {
            GetComponent<PhotonView>().RPC("ClientProjectile", PhotonTargets.Others, hitPoint);
            RealProjectile(hitPoint);
        }
        if (barrelNum == 2)
        {
            GetComponent<PhotonView>().RPC("ClientProjectile3", PhotonTargets.Others, hitPoint);
            RealProjectile3(hitPoint);
        }



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
        Physics.IgnoreCollision(GetComponent<CharacterController>(), proj.GetComponent<BoxCollider>());
        Physics.IgnoreCollision(GetComponent<CapsuleCollider>(), proj.GetComponent<BoxCollider>());
        proj.GetComponent<Rigidbody>().AddForce(proj.transform.forward * projectileVelocity);


    }

    [PunRPC]
    void ClientProjectile3(Vector3 hitPoint)
    { //MUST ONLY SEND VECTORS

        ProjectileBarrel.transform.position = barrel.position;
        ProjectileBarrel.transform.LookAt(hitPoint); //REAL BARREL
        GameObject proj = Instantiate(projectileSecondary, barrel.position + barrel.transform.forward, ProjectileBarrel.rotation); // may change forward bit
        proj.GetComponent<Rigidbody>().AddForce(proj.transform.forward * projectileVelocitySecondary);
        Physics.IgnoreCollision(GetComponent<CharacterController>(), proj.GetComponent<BoxCollider>());
        Physics.IgnoreCollision(GetComponent<CapsuleCollider>(), proj.GetComponent<BoxCollider>());
        proj.GetComponent<ProjectileScript>().volume = volumeSecondary;



    }

    void RealProjectile(Vector3 hitPoint)
    {
        viewModelBarrel.transform.LookAt(hitPoint);
        viewModelBarrel2.transform.LookAt(hitPoint);
        GameObject proj = Instantiate(projectile, viewModelBarrel.position, viewModelBarrel.rotation);
         projScript = proj.GetComponent<ProjectileScript>();
        projScript.DealsDamage = true;
        projScript.damage = damage;
        projScript.sourcePlayer = gameObject;
        projScript.volume = volume;
        projScript.projectileTeamID = this.GetComponent<TeamMember>().teamID;
        projScript.sourcePlayer = gameObject;
        projScript.muzzleFlashLocal = localMuzzleflash;
        projScript.damageType = damageType;
        projScript.playerSourceName = PhotonNetwork.player.name;
        proj.GetComponent<Rigidbody>().AddForce(proj.transform.forward * projectileVelocity);
        projScript.physicsSettings();
        if (localMuzzleflash)
        {
            Invoke("muzzleLocalInvoke", 0.001f);
        }
        
    }
    void muzzleLocalInvoke()
    {
        projScript.MuzzleLocal(gameObject);
    }
    void RealProjectile2(Vector3 hitPoint)
    {
        viewModelBarrel2.transform.LookAt(hitPoint);
        GameObject proj = Instantiate(projectile, viewModelBarrel.position, viewModelBarrel.rotation);
         projScript = proj.GetComponent<ProjectileScript>();
        projScript.DealsDamage = true;
        projScript.damage = damage;
        projScript.sourcePlayer = gameObject;
        projScript.volume = volume;
        projScript.projectileTeamID = this.GetComponent<TeamMember>().teamID;
        projScript.damageType = damageType;
        projScript.playerSourceName = PhotonNetwork.player.name;
        proj.GetComponent<Rigidbody>().AddForce(proj.transform.forward * projectileVelocity);
        projScript.physicsSettings();

        if (localMuzzleflash)
        {
            Invoke("muzzleLocalInvoke", 0.001f);
        }
    }
    void RealProjectile3(Vector3 hitPoint)
    { //Shooter's end
        viewModelBarrel.transform.LookAt(hitPoint);
        GameObject proj = Instantiate(projectileSecondary, viewModelBarrel.position, viewModelBarrel.rotation);
         projScript = proj.GetComponent<ProjectileScript>();
        projScript.DealsDamage = true;
        projScript.damage = damage;
        projScript.projectileTeamID = teamID;
        projScript.Explosive = true;
        projScript.ExplosiveDamage = explosiveDamage;
        projScript.ExplosionRadius = explosiveRadius;
        projScript.ExplosiveForce = explosiveForce;
        projScript.local2dAudio = localSecondary2dSoundImpact;
        projScript.sourcePlayer = gameObject;
        projScript.damageType = damageType;
        projScript.playerSourceName = PhotonNetwork.player.name;
        projScript.volume = volume;
        projScript.projectileTeamID = this.GetComponent<TeamMember>().teamID;
        projScript.physicsSettings();
        proj.GetComponent<Rigidbody>().AddForce(proj.transform.forward * projectileVelocitySecondary);
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
