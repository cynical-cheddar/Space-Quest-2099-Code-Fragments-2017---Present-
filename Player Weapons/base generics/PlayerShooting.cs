using UnityEngine;
using System.Collections;

public class PlayerShooting : MonoBehaviour {



    SelfNetworkManagerII snm;

    
    public int weaponID = 0;
    public Transform localBarrel;
    public Transform Barrel;
    public float fireRate = 0.1f;
    float cooldown;
    public GameObject placeHolderObject;
    public GameObject steamVent;
    public Transform steamVentLocation;
    float damage = 2f; 
    public float PrimaryDamage = 10f; 
    public float SecondaryDamage = 40f; // Constant
    public int ammoPrimaryPerShot = 1;
    public int ammoSecondaryPerShot = 5;
    public float ammo = 25;
    public float ammoInClip = 5;
    public float MaxAmmo = 50;
    public float ClipSize = 5;
    public string damageType = "Normal";
    public string damageType2 = "BlueEnergy";
    Animator anim;
    CanFire CF;
    public AnimationClip shootClip;
    public AnimationClip reloadClip;
    public GameObject viewModel;
    float ammoToReload;
    [Header("Prefabs")]
    public GameObject[] beamLineRendererPrefab;
    public GameObject[] beamStartPrefab;
    public GameObject[] beamEndPrefab;
    float timer = 0f;
    float disabledTime;
    float enabledTime;
    private int currentBeam = 0;

    private GameObject beamStart;
    private GameObject beamEnd;
    private GameObject beam;
    private LineRenderer line;

    [Header("Adjustable Variables")]
    public float beamEndOffset = 1f; //How far from the raycast hit point the end effect is positioned
    public float textureScrollSpeed = 8f; //How fast the texture scrolls along the beam
    public float textureLengthScale = 3; //Length of the beam texture


    // Use this for initialization
    void Start()
    {
        cooldown = fireRate;
        snm = GameObject.FindObjectOfType<SelfNetworkManagerII>();
        snm.UIClipAmmo.text = ammoInClip.ToString() + "/ ";
        snm.UIReserveAmmo.text = ammo.ToString();
        CF = this.GetComponent<CanFire>();
    }
    public void UpdateHUD()
    {
        snm.UIClipAmmo.text = ammoInClip.ToString() + "/ ";
        snm.UIReserveAmmo.text = ammo.ToString();
    }
    private void OnEnable()
    {
        quickReload();
        snm.UIClipAmmo.text = ammoInClip.ToString() + "/ ";
        snm.UIReserveAmmo.text = ammo.ToString();
    }


    // Update is called once per frame
    void Update()
    {
        if (cooldown > 0)
        {
            cooldown -= Time.deltaTime;
        }

        if (Input.GetMouseButtonDown(0) && ammoInClip > 0 && CF.CanShoot == true && cooldown <= 0)
        {
            viewModel.GetComponent<Animation>().clip = shootClip;
            viewModel.GetComponent<Animation>().Play();
            currentBeam = 0;
            damage = PrimaryDamage;
            ammoInClip -= ammoPrimaryPerShot;
            beamStart = Instantiate(beamStartPrefab[currentBeam], localBarrel.position, Quaternion.identity) as GameObject;
            beamEnd = Instantiate(beamEndPrefab[currentBeam], localBarrel.position, Quaternion.identity) as GameObject;
            beam = Instantiate(beamLineRendererPrefab[currentBeam], localBarrel.position, Quaternion.identity) as GameObject;
            line = beam.GetComponent<LineRenderer>();
            this.GetComponent<PhotonView>().RPC("CreateServerStreak", PhotonTargets.Others, currentBeam);         //RPC
            Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
            RaycastHit hit; 
            Transform hitTransform;
            Vector3 hitVector;
            hitTransform = FindClosestHitObject(ray, out hitVector);


            Physics.Raycast(ray.origin, ray.direction, out hit);


            if (hitTransform == null)
            {
                hit.point = Camera.main.transform.position + (Camera.main.transform.forward * 150f);
            }
            if (cooldown <= 0)
            {
                this.GetComponent<PhotonView>().RPC("StreakPositionServer", PhotonTargets.Others, hit.point);         //RPC
                this.GetComponent<PhotonView>().RPC("ShootAnimation", PhotonTargets.All);
            }

            Vector3 tdir = hit.point - localBarrel.position;

            Vector3 tempLocal = Barrel.localPosition;
            ShootBeamInDir(localBarrel.position, tdir);
            Fire();

            cooldown = fireRate;
        }
        if (Input.GetMouseButtonDown(1) && ammoInClip >= ammoSecondaryPerShot && CF.CanShoot == true && cooldown <= 0)
        {
            viewModel.GetComponent<Animation>().clip = shootClip;
            viewModel.GetComponent<Animation>().Play();
            currentBeam = 1;
            damage = SecondaryDamage;
            ammoInClip -= ammoSecondaryPerShot;
            beamStart = Instantiate(beamStartPrefab[currentBeam], localBarrel.position, Quaternion.identity) as GameObject;
            beamEnd = Instantiate(beamEndPrefab[currentBeam], localBarrel.position, Quaternion.identity) as GameObject;
            beam = Instantiate(beamLineRendererPrefab[currentBeam], localBarrel.position, Quaternion.identity) as GameObject;
            line = beam.GetComponent<LineRenderer>();
            this.GetComponent<PhotonView>().RPC("CreateServerStreak", PhotonTargets.Others, currentBeam);         //RPC
            Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
            RaycastHit hit; //From camera to hitpoint, not as curent
            Transform hitTransform;
            Vector3 hitVector;
            hitTransform = FindClosestHitObject(ray, out hitVector);


            Physics.Raycast(ray.origin, ray.direction, out hit);

            if (hitTransform == null)
            {
                hit.point = Camera.main.transform.position + (Camera.main.transform.forward * 150f);
            }
            if (cooldown <= 0)
            {
                this.GetComponent<PhotonView>().RPC("StreakPositionServer", PhotonTargets.Others, hit.point);         //RPC
                this.GetComponent<PhotonView>().RPC("ShootAnimation", PhotonTargets.All);
            }

            Vector3 tdir = hit.point - localBarrel.position;

            Vector3 tempLocal = Barrel.localPosition;
            ShootBeamInDir(localBarrel.position, tdir);
            Fire();

            cooldown = fireRate;
        }
        if (Input.GetButtonDown("Reload") && CF.CanShoot == true && cooldown <= 0 && ammo > 0)
        {
            cooldown = reloadClip.length;
            viewModel.GetComponent<Animation>().clip = reloadClip;
            viewModel.GetComponent<Animation>().Play();
            ammoToReload = ClipSize - ammoInClip;
            if (ammoToReload > ammo) // not enough reserve ammo.
            {
                int i = 0;
                ammoToReload = 0;
                while (i < ammo + ammoInClip)
                {
                    if (i < ammo)
                    {
                        ammoToReload++;
                    }
                    i++;
                }
                //ammo -= ammoToReload;
            }
            GameObject steam = Instantiate(steamVent, steamVentLocation.position, steamVentLocation.rotation);
            steam.transform.parent = steamVentLocation;
            Invoke("reloadMaths", reloadClip.length - 0.3f);
        }
    }
    void quickReload()
    {

        ammoToReload = ClipSize - ammoInClip;
        if(ammoToReload != 0)
        {
            GameObject steam = Instantiate(steamVent, steamVentLocation.position, steamVentLocation.rotation);
            steam.transform.parent = steamVentLocation;
        }
        if (ammoToReload > ammo) // not enough reserve ammo.
        {
            int i = 0;
            ammoToReload = 0;
            while (i < ammo + ammoInClip)
            {
                if (i < ammo)
                {
                    ammoToReload++;
                }
                i++;
            }
            //ammo -= ammoToReload;
        }
        reloadMaths();
    }
    void reloadMaths()
    {
        ammo -= ammoToReload;
        ammoInClip += ammoToReload;
        snm.UIClipAmmo.text = ammoInClip.ToString() + "/ ";
        snm.UIReserveAmmo.text = ammo.ToString();
    }
    public void endBeam()
    {
        Destroy(beamStart);
        Destroy(beamEnd);
        Destroy(beam);
        this.GetComponent<PhotonView>().RPC("RemoveServerBeam", PhotonTargets.Others);         //RPC
    }
    [PunRPC]
    public void StreakPositionServer(Vector3 hitPoint) // We need to somehow interpolate the hit position between updates.
    {
        line.SetVertexCount(2);

        beamStart.transform.position = Barrel.transform.position;
        line.SetPosition(0, Barrel.transform.position);

        line.SetPosition(1, hitPoint);
        beamEnd.transform.position = hitPoint;
        beamStart.transform.LookAt(beamEnd.transform.position);
        beamEnd.transform.LookAt(beamStart.transform.position);
        StartCoroutine(RPCbeamstart());
        float distance = Vector3.Distance(Barrel.transform.position, hitPoint);
        line.sharedMaterial.mainTextureScale = new Vector2(distance / textureLengthScale, 1);
        line.sharedMaterial.mainTextureOffset -= new Vector2(Time.deltaTime * textureScrollSpeed, 0);
    }
    IEnumerator RPCbeamstart() // We need to pass this the old and new vector positions of the hitpoint.
    {
        float totalTime = 0.2f;
        float elapsedTime = 0.0f;
        while (totalTime > elapsedTime)
        {
            elapsedTime += Time.deltaTime;
            beamStart.transform.position = localBarrel.position;
            yield return null;
        }

    }


    [PunRPC]
    void CreateServerStreak(int currentBeam)
    {
        beamStart = Instantiate(beamStartPrefab[currentBeam], localBarrel.position, Quaternion.identity) as GameObject;
        beamEnd = Instantiate(beamEndPrefab[currentBeam], localBarrel.position, Quaternion.identity) as GameObject;
        beam = Instantiate(beamLineRendererPrefab[currentBeam], localBarrel.position, Quaternion.identity) as GameObject;
        line = beam.GetComponent<LineRenderer>();
    }

    [PunRPC]
    void RemoveServerStreak()
    {
        Destroy(beamStart);
        Destroy(beamEnd);
        Destroy(beam);
    }

    void Fire()
    {
        if (cooldown > 0)
        {
            return;
        }
        if (ammo < 0)
        {
            ammo = 0;
        }
        snm.UIClipAmmo.text = ammoInClip.ToString() + "/ ";
        snm.UIReserveAmmo.text = ammo.ToString();

        cooldown = fireRate;
        Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
        Transform hitTransform;
        Vector3 hitPoint;

        hitTransform = FindClosestHitObject(ray, out hitPoint);

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

    public void nextBeam() // Next beam
    {
        if (currentBeam < beamLineRendererPrefab.Length - 1)
            currentBeam++;
        else
            currentBeam = 0;
    }

    public void previousBeam() // Previous beam
    {
        if (currentBeam > -0)
            currentBeam--;
        else
            currentBeam = beamLineRendererPrefab.Length - 1;

    }




    void ShootBeamInDir(Vector3 start, Vector3 dir)
    {
        line.SetVertexCount(2);
        line.SetPosition(0, start);
        beamStart.transform.position = start;

        Vector3 end = Vector3.zero;
        RaycastHit hit;
        if (Physics.Raycast(start, dir, out hit))
            end = hit.point - (dir.normalized * beamEndOffset);
        else
            end = transform.position + (dir * 100);




        // Replace above with own code - Raycast from camera.
        beamEnd.transform.position = end;
        line.SetPosition(1, end);

        beamStart.transform.LookAt(beamEnd.transform.position);
        beamEnd.transform.LookAt(beamStart.transform.position);

        float distance = Vector3.Distance(start, end);
        line.sharedMaterial.mainTextureScale = new Vector2(distance / textureLengthScale, 1);
        line.sharedMaterial.mainTextureOffset -= new Vector2(Time.deltaTime * textureScrollSpeed, 0);
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