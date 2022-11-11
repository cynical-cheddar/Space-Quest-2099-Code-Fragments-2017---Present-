using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LanceBeam : MonoBehaviour {

    SelfNetworkManagerII snm;
    public float volume = 1f;
    public int ammoPerShot = 3;
    public int weaponID = 1;
    public Transform localBarrel;
    public AudioSource auxAudio;
    public AudioSource auxAudio2;
    public AudioClip drillSoundEffect;
    public AudioClip powerUpSound;
    public AudioClip cockSound;
    public Transform Barrel;
    public float fireRate = 0.1f;
    public string damageType = "Normal";
    float cooldown;
    public GameObject placeHolderObject;
    public float damage = 25f; // This is temporary, damage falloff should be exponential.
    public float ammo = 100f;
    public int clips = 3;
    Animator anim;
    CanFire CF;
    Animator viewModelAnimator;
    public GameObject viewModel;
    [Header("Prefabs")]
    public GameObject[] beamLineRendererPrefab;
    public GameObject[] beamStartPrefab;
    public GameObject[] beamEndPrefab;
    float timer = 0f;
    float disabledTime;
    float enabledTime;
    private int currentBeam = 0;
    bool reloaded = false;
    bool reloading = false;
    private GameObject beamStart;
    private GameObject beamEnd;
    private GameObject beam;
    private LineRenderer line;

    [Header("Adjustable Variables")]
    public float beamEndOffset = 1f; //How far from the raycast hit point the end effect is positioned
    public float textureScrollSpeed = 8f; //How fast the texture scrolls along the beam
    public float textureLengthScale = 3; //Length of the beam texture

    Vector3 oldHitpoint;
    Vector3 CurHitPoint;

    Vector3 oldStart;
    Vector3 CurStart;
    // Use this for initialization

    public void UpdateHud()
    {
        snm.UIClipAmmo.text = ammo.ToString() + "/ ";
        snm.UIReserveAmmo.text = clips.ToString(); ;
    }
    void Start()
    {
        cooldown = fireRate;
        snm = GameObject.FindObjectOfType<SelfNetworkManagerII>();
        snm.UIClipAmmo.text = ammo.ToString() + "/ ";
        snm.UIReserveAmmo.text = clips.ToString(); ;
        CF = this.GetComponent<CanFire>();
        viewModelAnimator = viewModel.GetComponent<Animator>();
    }
    void reload()
    {
        ammo = 100;
        clips--;
        reloading = false;
    }
    void drillSound()
    {
        auxAudio.clip = drillSoundEffect;
        auxAudio.volume = volume;
        auxAudio.Play();
    }
    void powerupSoundVoid()
    {
        auxAudio2.clip = powerUpSound;
        auxAudio2.volume = 0.5f;
        auxAudio2.Play();
    }
    // Update is called once per frame
    void Update()
    {
        if (cooldown > 0)
        {
            cooldown -= Time.deltaTime;
        }
        if (viewModelAnimator.GetBool("FireLance") == false && ammo <= 0 && clips >0 && reloading == false)
        {
            if (reloaded == false)
            {
                viewModelAnimator.SetTrigger("Load");
                ammo = 100;
                clips--;
                reloaded = true;
                Invoke("drillSound", 0f);
            }
            else
            {
                viewModelAnimator.SetTrigger("Reload");
                reloading = true;
                auxAudio.clip = cockSound;
                auxAudio.Play();
                Invoke("powerupSoundVoid", 0.7f);
                Invoke("reload", 2f);
                Invoke("drillSound", 3f);
            }
        }
        if (ammo <= 100)
        {
            timer += Time.deltaTime;
            if (timer >= 0.1)
            {
                snm.UIClipAmmo.text = ammo.ToString() + "/ ";
                snm.UIReserveAmmo.text = clips.ToString();

                timer = 0f;
            }
        }


        if (Input.GetMouseButtonDown(0) && ammo > 0 && CF.CanShoot == true)
        {
            beamStart = Instantiate(beamStartPrefab[currentBeam], localBarrel.position, Quaternion.identity) as GameObject;
            beamEnd = Instantiate(beamEndPrefab[currentBeam], localBarrel.position, Quaternion.identity) as GameObject;
            beam = Instantiate(beamLineRendererPrefab[currentBeam], localBarrel.position, Quaternion.identity) as GameObject;
            line = beam.GetComponent<LineRenderer>();
            this.GetComponent<PhotonView>().RPC("CreateServerBeamLance", PhotonTargets.Others);         //RPC
            viewModelAnimator.SetBool("FireLance", true);
       //     auxAudio.clip = beamStartAudio;
     //       auxAudio.Play();

        }
        if (Input.GetMouseButtonUp(0) || ammo <= 0)
        {
            Destroy(beamStart);
            Destroy(beamEnd);
            Destroy(beam);
            this.GetComponent<PhotonView>().RPC("RemoveServerBeamLance", PhotonTargets.Others);         //RPC
            viewModelAnimator.SetBool("FireLance", false);
        }

        if (Input.GetMouseButton(0) && ammo > 0 && CF.CanShoot == true)
        {
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
                this.GetComponent<PhotonView>().RPC("LaserPositionServerLance", PhotonTargets.Others, hit.point);         //RPC
            }

            Vector3 tdir = hit.point - localBarrel.position;

            Vector3 tempLocal = Barrel.localPosition;
            ShootBeamInDir(localBarrel.position, tdir);
            Fire();

        }
        if (Input.GetButton("CharacterChange") || Input.GetButton("TeamChange"))
        {
            endBeam();
        }
    }
    public void endBeam()
    {
        Destroy(beamStart);
        Destroy(beamEnd);
        Destroy(beam);
        this.GetComponent<PhotonView>().RPC("RemoveServerBeamLance", PhotonTargets.Others);         //RPC
    }
    [PunRPC]
    public void LaserPositionServerLance(Vector3 hitPoint) // We need to somehow interpolate the hit position between updates.
    {

        //  Debug.Log(start);
        //  GameObject worldPlaceholder = Instantiate(placeHolderObject);

        //worldPlaceholder.transform.parent = transform;
        // worldPlaceholder.transform.localPosition = start;
        // Vector3 worldPosStart = worldPlaceholder.transform.position;
        // Debug.Log(worldPosStart);
        line.SetVertexCount(2);

        if (oldHitpoint != null && oldStart != null)
        {
            StartCoroutine(LerpHitAndStart(oldHitpoint, hitPoint, 0.1f));
        }
        // beamEnd.transform.position = hitPoint;


        // initiate lerp here


        // oldStart = worldPosStart;
        oldHitpoint = hitPoint;
    }
    IEnumerator LerpHitAndStart(Vector3 oldPos, Vector3 newPos, float time) // We need to pass this the old and new vector positions of the hitpoint.
    {
        float ElapsedTime = 0f;
        while (ElapsedTime <= time)
        { // until one second passed

            ElapsedTime += Time.deltaTime;
            CurHitPoint = Vector3.Lerp(oldPos, newPos, (ElapsedTime / time)); // lerp from A to B in one second


            beamStart.transform.position = Barrel.transform.position;
            line.SetPosition(0, Barrel.transform.position);


            line.SetPosition(1, CurHitPoint);
            beamEnd.transform.position = CurHitPoint;
            beamStart.transform.LookAt(beamEnd.transform.position);
            beamEnd.transform.LookAt(beamStart.transform.position);

            float distance = Vector3.Distance(Barrel.transform.position, CurHitPoint);
            line.sharedMaterial.mainTextureScale = new Vector2(distance / textureLengthScale, 1);
            line.sharedMaterial.mainTextureOffset -= new Vector2(Time.deltaTime * textureScrollSpeed, 0);
            yield return null; // wait for next frame
        }
    }

    [PunRPC]
    void CreateServerBeamLance()
    {
        beamStart = Instantiate(beamStartPrefab[currentBeam], localBarrel.position, Quaternion.identity) as GameObject;
        beamEnd = Instantiate(beamEndPrefab[currentBeam], localBarrel.position, Quaternion.identity) as GameObject;
        beam = Instantiate(beamLineRendererPrefab[currentBeam], localBarrel.position, Quaternion.identity) as GameObject;
        line = beam.GetComponent<LineRenderer>();
    }
    [PunRPC]
    void RemoveServerBeamLance()
    {
        Destroy(beamStart);
        Destroy(beamEnd);
        Destroy(beam);
    }
    void OnEnable()
    {
        snm.UIClipAmmo.text = ammo.ToString() + "/ ";
        snm.UIReserveAmmo.text = clips.ToString();
    }
    private void OnDisable()
    {
        disabledTime = Time.time;
    }

    void Fire()
    {
        if (cooldown > 0)
        {
            return;
        }
        ammo -= ammoPerShot;
        if (ammo < 0)
        {
            ammo = 0;
        }
        snm.UIClipAmmo.text = ammo.ToString() + "/ ";
        snm.UIReserveAmmo.text = clips.ToString();

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