using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrenadeLauncher : MonoBehaviour {

    SelfNetworkManagerII snm;
    public int ammo = 1;
    public int maxAmmo = 16;
    public float fireRate = 1.01f;
    public float fuseTime = 1f;
    float cooldown = 0;
    public string damageType = "Explosive";
    public float damage = 25f;
    public float explosiveDamage = 80f;
    public float explosiveRadius = 12f;
    public float explosiveForce = 30000f;
    public GameObject impactExplosion;
    int teamID;
    public Transform barrel;
    public Transform ProjectileBarrel;
    public Transform viewModelBarrel;
    FXManager fxManager;
    Animator anim;
    public GameObject viewModel;
    public GameObject Grenade;
    public GameObject Spigot;
    public GameObject projectile;
    public GameObject projectileDummy;
    Transform barrelYcomponent;
    public float volume = 1f;
    public float projectileVelocity = 1250;
    CharacterController cc;
    Rigidbody rb;


    public AnimationClip FireAnimation;
    public AnimationClip GrenadeAnimation;
    public AnimationClip LastGrenadeAnimation;
    public AnimationClip ReloadAnimation;
    public AnimationClip SpigotAnimation;
    CanFire CF;
    public int weaponID = 0;
    // Update is called once per frame
    void Start()
    {
        fxManager = GameObject.FindObjectOfType<FXManager>();
        Transform animParent = this.transform.Find("PlayerModel");
        anim = animParent.gameObject.GetComponent<Animator>();
        barrelYcomponent = this.transform.Find("BarrelPosition");
        teamID = this.GetComponent<TeamMember>().teamID;
        snm = GameObject.FindObjectOfType<SelfNetworkManagerII>();
        snm.UIClipAmmo.text = ammo.ToString() + "/ ";
        snm.UIReserveAmmo.text = ammo.ToString();
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
    }
    void OnEnable()
    {
        snm.UIClipAmmo.text = " ";
        snm.UIReserveAmmo.text = ammo.ToString();

    }
    public void UpdateHUD()
    {
        snm.UIClipAmmo.text = " ";
        snm.UIReserveAmmo.text = ammo.ToString();
    }


    void Fire()
    {
        if (cooldown > 0)
        {
            return;
        }
        if (ammo == 0)
        {
            return;
        }

        ammo -= 1;
        snm.UIClipAmmo.text = " ";
        snm.UIReserveAmmo.text = ammo.ToString();
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
        DoGunFx(hitPoint);
        Animations();
        cooldown = fireRate;

    }
    public void pickupGrenadeAnimationCheck()
    {
        if (ammo == 0)
        {
            Grenade.GetComponent<Animation>().clip = ReloadAnimation;
            Grenade.GetComponent<Animation>().Play();
        }
    }
    void Animations()
    {

            viewModel.GetComponent<Animation>().clip = FireAnimation;
            viewModel.GetComponent<Animation>().Play();
        if (ammo > 0)
        {
            Grenade.GetComponent<Animation>().clip = GrenadeAnimation;
            Grenade.GetComponent<Animation>().Play();
        }
        if (ammo == 0)
        {
            Grenade.GetComponent<Animation>().clip = LastGrenadeAnimation;
            Grenade.GetComponent<Animation>().Play();
        }

        Spigot.GetComponent<Animation>().clip = SpigotAnimation;
        Spigot.GetComponent<Animation>().Play();


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

        GetComponent<PhotonView>().RPC("ClientProjectileGrenade", PhotonTargets.Others, hitPoint);

        //Local "real" projectile
        RealProjectile(hitPoint);


        //Animations:
        this.GetComponent<PhotonView>().RPC("ShootAnimation", PhotonTargets.All);
        //End animations



    }
    [PunRPC]
    void ClientProjectileGrenade(Vector3 hitPoint)
    { //MUST ONLY SEND VECTORS
        ProjectileBarrel.transform.position = barrel.position;
        ProjectileBarrel.transform.LookAt(hitPoint); //REAL BARREL
        GameObject proj = Instantiate(projectileDummy, barrel.position + barrel.forward, ProjectileBarrel.rotation);
        DummyGrenadeScript projScript = proj.GetComponent<DummyGrenadeScript>();
        proj.GetComponent<Rigidbody>().AddForce(proj.transform.forward * projectileVelocity);
        projScript.grenadeTimer = fuseTime;
        Physics.IgnoreCollision(GetComponent<CharacterController>(), proj.GetComponent<BoxCollider>());
        Physics.IgnoreCollision(GetComponent<CapsuleCollider>(), proj.GetComponent<BoxCollider>());




    }
    void RealProjectile(Vector3 hitPoint)
    { //Shooter's end
        viewModelBarrel.transform.LookAt(hitPoint);
        GameObject proj = Instantiate(projectile, viewModelBarrel.position + viewModelBarrel.forward, viewModelBarrel.rotation);
        ProjectileScript projScript = proj.GetComponent<ProjectileScript>();
        projScript.DealsDamage = true;
        projScript.damage = damage;
        projScript.projectileTeamID = teamID;
        projScript.Explosive = true;
        projScript.ExplosiveDamage = explosiveDamage;
        projScript.ExplosionRadius = explosiveRadius;
        projScript.ExplosiveForce = explosiveForce;
        projScript.sourcePlayer = gameObject;
        projScript.playerSourceName = PhotonNetwork.player.name;
        projScript.damageType = damageType;
        projScript.volume = volume;
        projScript.projectileTeamID = this.GetComponent<TeamMember>().teamID;
        projScript.physicsSettings();
        projScript.grenadeTimer = fuseTime;
        projScript.IsGrenade = true;
        proj.GetComponent<Rigidbody>().AddForce(proj.transform.forward * projectileVelocity);
    }


    [PunRPC]
    void SetFireTrigger2()   //Network fire animation occurs here
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
