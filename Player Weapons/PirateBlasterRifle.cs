using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PirateBlasterRifle : MonoBehaviour
{
    public float fireRate = 0.3f;
        public float fireRate2 = 0.1f;
    public int ammoPerShotPrimary = 1;
    public int ammoPerShotSecondary = 1;
    public int currentAmmo = 100;
    public int MaxAmmo = 100;


    public string damageType = "Normal";
    float cooldown = 0;
    public float damagePrimary = 10f;
    public float damageSecondary = 10f;
    public bool LocalMuzzleFlash = true;
    public bool LocalImpact2dSound = false;
    public Transform barrel;
    public Transform ProjectileBarrel;
    public Transform viewModelBarrel;
    public Transform steamVentLocation;

    FXManager fxManager;
    Animator anim;
    public GameObject viewModel;
    public GameObject projectile;
    public GameObject projectileSecondary;
    public GameObject steamVent;
    Transform barrelYcomponent;
    public float volume = 0.4f;
    public float projectileVelocity = 8000f;
    public int weaponID = 2;
    CanFire CF;
    public AnimationClip primaryFire;
    public AnimationClip[] secondaryFire;
    public float scaleLimit = 1f;
    public float z = 50;
    SelfNetworkManagerII snm;
    ProjectileScript projScript;


    // Update is called once per frame
    void Start()
    {
        fxManager = GameObject.FindObjectOfType<FXManager>();
        Transform animParent = this.transform.Find("PlayerModel");
        anim = animParent.gameObject.GetComponent<Animator>();
        barrelYcomponent = this.transform.Find("BarrelPosition");
        CF = this.GetComponent<CanFire>();
        snm = GameObject.FindObjectOfType<SelfNetworkManagerII>();
        snm.UIClipAmmo.text = currentAmmo.ToString();
        snm.UIReserveAmmo.text = "";

    }
    void Update()
    {
        cooldown -= Time.deltaTime;

        if (Input.GetButton("Fire1") && CF.CanShoot == true)
        {
            // Player wants to shoot...so. Shoot.

            Fire();
        }
        if (Input.GetButton("Fire2") && CF.CanShoot == true)
        {
            // Player wants to shoot...so. Shoot.

            Fire2();
        }
        if (cooldown <= 0.2f)
        {
            anim.SetBool("Fire", false);  //Network fire animation terminated here
        }
        // barrelPos = barrel.GetComponent<Transform>().position;
        //    Debug.Log(barrelPos.y);
        }
    
    void steam()
    {
        GameObject steam = Instantiate(steamVent, steamVentLocation.position, steamVentLocation.rotation);
        steam.transform.parent = steamVentLocation;
        steam.GetComponent<AudioSource>().volume = volume;
    }
    public void UpdateHUD()
    {
        snm.UIClipAmmo.text = currentAmmo.ToString();
        snm.UIReserveAmmo.text = "";
    }
    private void OnEnable()
    {
        snm.UIClipAmmo.text = currentAmmo.ToString();
        snm.UIReserveAmmo.text = "";
    }
    void Fire()
    {
        if (cooldown > 0)
        {
            return;
        }
        if (CF.CanShoot == false)
        {
            return;
        }
        if (currentAmmo < ammoPerShotPrimary)
        {
            return;
        }

        currentAmmo -= ammoPerShotPrimary;
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


        viewModel.GetComponent<Animation>().clip = primaryFire;
        viewModel.GetComponent<Animation>().Play();
        DoGunFx(hitPoint, 1);


        cooldown = fireRate;
        snm.UIClipAmmo.text = currentAmmo.ToString();
        snm.UIReserveAmmo.text = "";
    }

    void Fire2()
    {
        if (cooldown > 0)
        {
            return;
        }
        if (currentAmmo < ammoPerShotSecondary)
        {
            return;
        }

        currentAmmo -= ammoPerShotSecondary;
           snm.UIClipAmmo.text = currentAmmo.ToString() + "/ ";
        //  snm.UIReserveAmmo.text = "100";
        float randomRadius = Random.Range(0, scaleLimit);
        float randomAngle = Random.Range(0, 2 * Mathf.PI);
        Vector3 direction = new Vector3(
            randomRadius * Mathf.Cos(randomAngle),
            randomRadius * Mathf.Sin(randomAngle),
            z
            );
        direction = Camera.main.transform.TransformDirection(direction.normalized);
        Ray ray = new Ray(Camera.main.transform.transform.position, direction);
        RaycastHit hitInfo;

        Physics.Raycast(ray, out hitInfo);
        Vector3 hitPoint = hitInfo.point;
        Vector3 hitVector;
        Transform hitTransform = FindClosestHitObject(ray, out hitVector);
        if (hitTransform == null)
        {
            hitPoint = Camera.main.transform.position + (Camera.main.transform.forward * 150f);
        }


        int secondaryAnimID = Random.Range(0, secondaryFire.Length);
        viewModel.GetComponent<Animation>().clip = secondaryFire[secondaryAnimID];
        viewModel.GetComponent<Animation>().Play();
        DoGunFx(hitPoint, 2);


        cooldown = fireRate2;
        snm.UIClipAmmo.text = currentAmmo.ToString();
        snm.UIReserveAmmo.text = "";
    }

    void DoGunFx(Vector3 hitPoint, int type)
    {
        Vector3 startPos = new Vector3(barrel.position.x, barrel.position.y, barrel.position.z);
        ProjectileBarrel.transform.position = startPos;
        ProjectileBarrel.transform.LookAt(hitPoint);
        if (anim.GetFloat("Speed") < 0)
        {
            startPos += (ProjectileBarrel.transform.forward);
        }

        GetComponent<PhotonView>().RPC("ClientProjectileBlasterPirate", PhotonTargets.Others, hitPoint, type);

        //Local "real" projectile
        RealProjectile(hitPoint, type);
        this.GetComponent<PhotonView>().RPC("ShootAnimation", PhotonTargets.All);

        //Animations:
        //  this.GetComponent<PhotonView>().RPC("SetFireTrigger", PhotonTargets.All);
        //End animations



    }
    [PunRPC]
    void ClientProjectileBlasterPirate(Vector3 hitPoint, int type)
    { //MUST ONLY SEND VECTORS

        ProjectileBarrel.transform.position = barrel.position;
        ProjectileBarrel.transform.LookAt(hitPoint); //REAL BARREL

        if(type == 1)
        {
            GameObject proj = Instantiate(projectile, barrel.position, ProjectileBarrel.rotation);
            Physics.IgnoreCollision(GetComponent<CharacterController>(), proj.GetComponent<BoxCollider>());
            Physics.IgnoreCollision(GetComponent<CapsuleCollider>(), proj.GetComponent<BoxCollider>());
            proj.GetComponent<ProjectileScript>().volume = volume;
            proj.GetComponent<Rigidbody>().AddForce(proj.transform.forward * projectileVelocity);

        }
        else
        {
            GameObject proj = Instantiate(projectileSecondary, barrel.position, ProjectileBarrel.rotation);
            Physics.IgnoreCollision(GetComponent<CharacterController>(), proj.GetComponent<BoxCollider>());
            Physics.IgnoreCollision(GetComponent<CapsuleCollider>(), proj.GetComponent<BoxCollider>());
            proj.GetComponent<ProjectileScript>().volume = volume;
            proj.GetComponent<Rigidbody>().AddForce(proj.transform.forward * projectileVelocity);
        }



    }
    void RealProjectile(Vector3 hitPoint, int type)
    {
        viewModelBarrel.transform.LookAt(hitPoint);

        if(type == 1)
        {
            GameObject proj = Instantiate(projectile, viewModelBarrel.position, viewModelBarrel.rotation);
            Physics.IgnoreCollision(GetComponent<CapsuleCollider>(), proj.GetComponent<BoxCollider>());
            Physics.IgnoreCollision(GetComponent<CharacterController>(), proj.GetComponent<BoxCollider>());
            projScript = proj.GetComponent<ProjectileScript>();

            projScript.sourcePlayer = gameObject;
            projScript.DealsDamage = true;
            projScript.damage = damagePrimary;
            proj.GetComponent<Rigidbody>().AddForce(proj.transform.forward * projectileVelocity);
        }
        else
        {
            GameObject proj = Instantiate(projectileSecondary, viewModelBarrel.position, viewModelBarrel.rotation);
            Physics.IgnoreCollision(GetComponent<CapsuleCollider>(), proj.GetComponent<BoxCollider>());
            Physics.IgnoreCollision(GetComponent<CharacterController>(), proj.GetComponent<BoxCollider>());
            projScript = proj.GetComponent<ProjectileScript>();

            projScript.sourcePlayer = gameObject;
            projScript.DealsDamage = true;
            projScript.damage = damageSecondary;
            proj.GetComponent<Rigidbody>().AddForce(proj.transform.forward * projectileVelocity);

        }
        projScript.playerSourceName = PhotonNetwork.player.name;
        projScript.damageType = damageType;
        projScript.volume = volume;
        projScript.projectileTeamID = this.GetComponent<TeamMember>().teamID;
        projScript.physicsSettings();
        if (LocalImpact2dSound == true)
        {
            projScript.local2dAudio = true;
        }
        if (LocalMuzzleFlash == true)
        {
            projScript.muzzleFlashLocal = true;
            Invoke("muzzleLocalInvoke", 0.01f);
        }

       

    }
    void muzzleLocalInvoke()
    {
        projScript.MuzzleLocal(gameObject);
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
