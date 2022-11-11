using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TetryonRepeater : MonoBehaviour {




    SelfNetworkManagerII snm;
    // public float projectileVelocity = 10000f;
    public AudioClip fireLoop;
    public AudioClip windDownSound;

    public bool IsRemoteFiring = false;
    bool IsLocal = true;
    public GameObject auxAudio;
    public GameObject muzzleFlash;
    public GameObject impactEffect;
    public Transform playercam;
    public int weaponID = 0;
    public GameObject[] barrels;
    public GameObject[] animationBarrels;
    public Transform Barrel;
    public float fireRate = 0.1f;
    public float volume = 0.5f;
    float cooldown;
    public bool shooting = false;
    public bool warmUp = false;
    public GameObject placeHolderObject;
    public GameObject steamVent;
    public Transform steamVentLocation;
    float damage = 2f;
    public float PrimaryDamage = 10f;
    public int ammoPrimaryPerShot = 1;
    public float ammo = 25;
    public float scaleLimit = 2.5f;
    public float z = 50;
    public string damageType = "Normal";
    Animator anim;
    CanFire CF;
    public AnimationClip shootClip;
    public AnimationClip shootClipViewModel;
    public AnimationClip tetryonDrop;
    public AnimationClip tetryonRaise;
    public AnimationClip reloadClip;
    public GameObject viewModel;
    float ammoToReload;
    CPMPlayer movementController;
    float normalWalkSpeed;
    float normalSprintSpeed;
    bool slow = false;

    public float fireWalkSpeed = 5;

    //  public GameObject tracerProjectile;

    int currentBarrel = 0;

 

    // Use this for initialization
    void Start()
    {
        cooldown = fireRate;
        snm = GameObject.FindObjectOfType<SelfNetworkManagerII>();

        snm.UIReserveAmmo.text = ammo.ToString();
        CF = this.GetComponent<CanFire>();
        movementController = this.GetComponent<CPMPlayer>();
        normalWalkSpeed = movementController.moveSpeed;


        if (this.GetComponent<PhotonView>().isMine == false)
        {
            IsLocal = false;
        }
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


    public void UpdateHUD()
    {

        snm.UIReserveAmmo.text = ammo.ToString();
    }
    private void OnEnable()
    {
        snm.UIClipAmmo.text = "";
        snm.UIReserveAmmo.text = ammo.ToString();
    }
    IEnumerator SetFiringVariables(float time1, bool wu, bool s) 
    {
        while (time1 > 0)
        {
            time1 -= Time.deltaTime;

            if (time1 <= 0)
            {
                warmUp = wu;
                shooting = s;

            }
            yield return null;
        }

    }
    void ShootingSounds()
    {
        if (Input.GetMouseButton(0) == false && GetComponent<PhotonView>().isMine == true) // If the mouse button is up right now, then return to default state.
        {
            StopShooting();
            return;
        }
        auxAudio.GetComponent<AudioSource>().clip = fireLoop;
        auxAudio.GetComponent<AudioSource>().loop = true;
        auxAudio.GetComponent<AudioSource>().volume = volume;
        auxAudio.GetComponent<AudioSource>().Play();
        // This is where we're actually shooting. We need to tell this to other players on the network
        // We can do this by setting a variable that is true for other players.
        // This variable would run a local version of the firing algorithm

        //RPC OTHERS
        this.GetComponent<PhotonView>().RPC("startRemoteShooting", PhotonTargets.Others);

    }
    [PunRPC]
    void startRemoteShooting()
    {
        GetComponent<TetryonRepeater>().enabled = true;
        IsRemoteFiring = true;
        auxAudio.GetComponent<AudioSource>().clip = fireLoop;
        auxAudio.GetComponent<AudioSource>().loop = true;
        auxAudio.GetComponent<AudioSource>().volume = volume;
        auxAudio.GetComponent<AudioSource>().Play();
    }
    [PunRPC]
    void stopRemoteShooting()
    {

        IsRemoteFiring = false;
        if (IsLocal == false)
        {
            auxAudio.GetComponent<AudioSource>().clip = windDownSound;
            auxAudio.GetComponent<AudioSource>().loop = false;
            auxAudio.GetComponent<AudioSource>().Play();
            GetComponent<TetryonRepeater>().enabled = false;
        }
    }

    void StopShooting()
    {
        warmUp = true;
        auxAudio.GetComponent<AudioSource>().clip = windDownSound;
        auxAudio.GetComponent<AudioSource>().loop = false;
        auxAudio.GetComponent<AudioSource>().Play();
        viewModel.GetComponent<Animation>().clip = tetryonRaise;
        viewModel.GetComponent<Animation>().Play();
        StartCoroutine(SetFiringVariables(tetryonRaise.length, false, false));
        Invoke("CanNowSwitch", tetryonRaise.length);
        movementController.moveSpeed = normalWalkSpeed;

    }
    void CanNowSwitch()
    {
        CF.CanSwitch = true;
    } 
   void Update()
    {
        if (IsLocal == true)
        {

            if (cooldown > 0)
            {
                cooldown -= Time.deltaTime;
            }
            if (Input.GetMouseButton(0) && ammo > 0 && CF.CanShoot == true && cooldown <= 0 && warmUp == false && shooting == false)
            {
                // If we're not shooting or preparing to fire our weapon.
                if(slow == false)
                {
                    SlowMovement();
                }

                warmUp = true;
                viewModel.GetComponent<Animation>().clip = tetryonDrop;
                viewModel.GetComponent<Animation>().Play();
                StartCoroutine(SetFiringVariables(tetryonDrop.length, false, true));
                Invoke("ShootingSounds", tetryonDrop.length);
                CF.CanSwitch = false;
            }


            if (Input.GetMouseButtonUp(0) && CF.CanShoot == true && warmUp == false && shooting == true)
            {
                // Stopping shooting
                StopShooting();

                Endslow();
                this.GetComponent<PhotonView>().RPC("stopRemoteShooting", PhotonTargets.Others);
            }
            //Raising weapon

            if (Input.GetMouseButton(0) && ammo > 0 && CF.CanShoot == true && cooldown <= 0 && shooting == true)
            {

                damage = PrimaryDamage; 

                ammo -= ammoPrimaryPerShot;
                
                float randomRadius = Random.Range(0, scaleLimit);
                float randomAngle = Random.Range(0, 2 * Mathf.PI);
                Vector3 direction = new Vector3(
                    randomRadius * Mathf.Cos(randomAngle),
                    randomRadius * Mathf.Sin(randomAngle),
                    z
                    );
                direction = playercam.transform.TransformDirection(direction.normalized);
                Ray ray = new Ray(playercam.position, direction);
                RaycastHit hit;
                Transform hitTransform;
                Vector3 hitVector;
                hitTransform = FindClosestHitObject(ray, out hitVector);


                Physics.Raycast(ray.origin, ray.direction, out hit);


                if (hitTransform == null)
                {
                    hit.point = Camera.main.transform.position + (Camera.main.transform.forward * 150f);
                }


                Vector3 tdir = hit.point - barrels[currentBarrel].transform.position;
                barrels[currentBarrel].transform.LookAt(hit.point);

                // Now we're going to multiply the damage by our rampup function based on proximity:
                damage = damageRampup(damage, hit.point);

                Vector3 tempLocal = Barrel.localPosition;

                //  ShootBeamInDir(localBarrel.position, tdir);
                Fire(ray, hit.point);

                cooldown = fireRate;

            }

            if (Input.GetMouseButton(0) && ammo <= 0 && CF.CanShoot == true && cooldown <= 0 && shooting == true)
            {
                StopShooting();
                Endslow();
                this.GetComponent<PhotonView>().RPC("stopRemoteShooting", PhotonTargets.Others);
            }
           }
        if(IsRemoteFiring == true && IsLocal == false)
        {
            playercam.transform.localRotation = Quaternion.AngleAxis(GetComponent<NetworkCharacter>().RealAimAngle, Vector3.left);
            cooldown -= Time.deltaTime;
            if(cooldown <= 0)
            {
                float randomRadius = Random.Range(0, scaleLimit);
                float randomAngle = Random.Range(0, 2 * Mathf.PI);
                Vector3 direction = new Vector3(
                    randomRadius * Mathf.Cos(randomAngle),
                    randomRadius * Mathf.Sin(randomAngle),
                    z
                    );
                direction = playercam.transform.TransformDirection(direction.normalized);
                Ray ray = new Ray(playercam.position, direction);
                RaycastHit hit;
                Transform hitTransform;
                Vector3 hitVector;
                hitTransform = FindClosestHitObject(ray, out hitVector);


                Physics.Raycast(ray.origin, ray.direction, out hit);
                if (hitTransform == null)
                {
                    hit.point = playercam.transform.position + (playercam.transform.forward * 150f);
                }
               GameObject hitImpactInstance = Instantiate(impactEffect, hit.point, transform.rotation );
                Instantiate(muzzleFlash, Barrel.position, playercam.transform.rotation);
               // muzzleFlash.transform.LookAt(hitImpactInstance.transform);
                cooldown = fireRate;
            }
        }

        
    }
    void reloadMaths()
    {
        ammo -= ammoToReload;
        ammo += ammoToReload;
        snm.UIClipAmmo.text = "";
        snm.UIReserveAmmo.text = ammo.ToString();
    }
    float damageRampup(float damage, Vector3 hitPoint) // OUR DAMAGE RAMPUP FUNCTION
    {
        float fltDistance = Vector3.Distance(hitPoint, transform.position);
        float fltNewDamage = damage * (4 * Mathf.Exp(-fltDistance/4) + 1);
        return fltNewDamage;
    }






    void Fire(Ray ray, Vector3 hitPt)
    {
        if (cooldown > 0)
        {
            return;
        }
        if (ammo < 0)
        {
            ammo = 0;
        }
        currentBarrel += 1;
        if (currentBarrel == barrels.Length)
        {
            currentBarrel = 0;
        }
        snm.UIClipAmmo.text = "";
        snm.UIReserveAmmo.text = ammo.ToString();

        cooldown = fireRate;

        Transform hitTransform;
        Vector3 hitPoint;

        hitTransform = FindClosestHitObject(ray, out hitPoint);
                animationBarrels[currentBarrel].GetComponent<Animation>().clip = shootClip;
            animationBarrels[currentBarrel].GetComponent<Animation>().Play();
        viewModel.GetComponent<Animation>().clip = shootClipViewModel;
        viewModel.GetComponent<Animation>().Play();
        //Time to do an effect
        barrels[currentBarrel].transform.LookAt(hitPt);
        GameObject mFlash = Instantiate(muzzleFlash, barrels[currentBarrel].transform.position, barrels[currentBarrel].transform.rotation);
        mFlash.transform.parent = barrels[currentBarrel].transform;
        //Time to do an effect
        GameObject impactFX = Instantiate(impactEffect, hitPt, barrels[currentBarrel].transform.rotation);
        if (hitTransform != null)
        {

            // We could do a special effect at the hit location
            // DoRicochetEffectAt( hitPoint );

            Health h = hitTransform.GetComponent<Health>();

            while (h == null && hitTransform.parent)
            {
                hitTransform = hitTransform.parent;
                h = hitTransform.GetComponent<Health>();
            }

            // Once we reach here, hitTransform may not be the hitTransform we started with!

            if (h != null)
            {
                // This next line is the equivalent of calling:
                //    				h.TakeDamage( damage );
                // Except more "networky"
                PhotonView pv = h.GetComponent<PhotonView>();
                if (pv == null)
                {
                    Debug.LogError("Freak out!");
                }
                else
                {
                    TeamMember tm = hitTransform.GetComponent<TeamMember>();
                    TeamMember myTm = this.GetComponent<TeamMember>();
                    if (tm == null || tm.teamID == 0 || myTm == null || myTm.teamID == 0 || tm.teamID != myTm.teamID)
                    {
                        GetComponent<HitSound>().PlaySound(damage);
                        h.GetComponent<PhotonView>().RPC("TakeDamage", PhotonTargets.All, damage, PhotonNetwork.player.name, damageType);         //RPC

                    }

                }
            }
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
