using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Collections.Generic;
public class TeslaGun : MonoBehaviour {
    [Header("Gameobjects that need to be set")]
    public GameObject outerLightningGameObject;
    public GameObject centreLightningGameObject;
    public GameObject teslaLightFlash;
    public GameObject sparkParticles;
    public AudioClip lightningLoop;
    public AudioClip lightningStart;
    [Header("Gameobjects on player")]
    public GameObject teslaGunViewModel;
    public Transform localBarrel;
    public Transform Barrel;
    public GameObject teslaFireCone;
    public GameObject[] GroundingSpots;
    public AudioSource audioSource;
    public AudioSource audioSource2;
    public GameObject auxRaycaster;
    public GameObject enableOnFire;
    public Camera playerCamera;
    [Header("Weapon Settings")]
    public float cooldown = 0.4f;
    float cooldownRemaining = 0.2f;
        SelfNetworkManagerII snm;
    public float cooldownLightning = 0.05f;
    float cooldownRemainingLightning = 0.05f;

    public float range = 10f;


    public int weaponID = 1;
    public string damageType = "Normal";
    public float damage = 2f; // This is temporary, damage falloff should be exponential.
    public float ammo = 100f;


    [Header("Outer Lightning Deviation Settings (I still don't understand these)")]
    public float scaleLimit = 2.5f;
    public float z = 50;



    [Header("Don't touch these (but you do need to initialise a length)")]
    public Vector3[] outerEndPosition;
    public Vector3[] centreEndPosition;
    public List<GameObject> enemiesInCone;
    public List<GameObject> enemiesInSight;

    CanFire CF;
    Animator viewModelAnimator;
    bool controlledLocally = false;
    bool remoteFiring = false;




    void Start () {
        CF = GetComponent<CanFire>();
        snm = GameObject.FindObjectOfType<SelfNetworkManagerII>();
        viewModelAnimator = teslaGunViewModel.GetComponent<Animator>();
        if(GetComponent<PhotonView>().isMine == true)
        {
            controlledLocally = true;
        }
        UpdateHUD();
    }
	
	// Update is called once per frame
	void Update () {

        cooldownRemainingLightning -= Time.deltaTime;
        if (remoteFiring == false && controlledLocally == true)
        {
            cooldownRemaining -= Time.deltaTime;
            if (Input.GetMouseButton(0) && ammo > 0 && CF.CanShoot == true && cooldownRemaining <= 0 && controlledLocally == true)
            {
                ammo--;
                UpdateHUD();
                cooldownRemaining = cooldown;
                viewModelAnimator.SetBool("FireTesla", true);
                StartSoundEffects();
                if (controlledLocally == true)
                {
                    GetComponent<PhotonView>().RPC("startRemoteFiringTesla", PhotonTargets.Others);
                }
                CalculateOuterLightningEndVector();

                FindEnemiesInFireCone();
                FindEnemiesInSight();
                fireCentreLightning();

                StartVisualEffects();
                if (enemiesInSight.Count > 0)
                {
                    dealDamage();
                }


            }
            if (Input.GetMouseButton(0) && ammo > 0 && CF.CanShoot == true && cooldownRemainingLightning <= 0 && controlledLocally == true)
            {

                fireOuterLightning();
                cooldownRemainingLightning = cooldownLightning;
            }
            if (Input.GetMouseButtonDown(0) && ammo > 0 && CF.CanShoot == true && controlledLocally == true)
            {

                audioSource2.clip = lightningStart;
                audioSource2.Play();
            }

            if (Input.GetMouseButtonUp(0) || ammo <= 0 && controlledLocally == true)
            {
                viewModelAnimator.SetBool("FireTesla", false);
                EndVisualEffects();
                EndSoundEffects();
                if (controlledLocally == true)
                {
                    GetComponent<PhotonView>().RPC("stopRemoteFiringTesla", PhotonTargets.Others);
                }
            }
        }

        if( remoteFiring == true && controlledLocally == false)
        {
            playerCamera.transform.localRotation = Quaternion.AngleAxis(GetComponent<NetworkCharacter>().RealAimAngle, Vector3.left);
            if (cooldownRemainingLightning <= 0)
            {
             //   Debug.Log("We're Just about to execute our lightning effects");
                Instantiate(teslaLightFlash, transform.position, Quaternion.identity);
                cooldownRemainingLightning = cooldownLightning;
                CalculateOuterLightningEndVector();
                fireOuterLightningRemote();
              //  Debug.Log("We've executed our lightning effects");
            }
        }

        }


    [PunRPC]
    void startRemoteFiringTesla()
    {
        GetComponent<TeslaGun>().enabled = true;
        remoteFiring = true;
        StartSoundEffects();
    }
    [PunRPC]
    void stopRemoteFiringTesla()
    {
        GetComponent<TeslaGun>().enabled = false;
        remoteFiring = false;
        EndSoundEffects();
    }

    void StartSoundEffects()
    {
        if(audioSource.isPlaying == false)
        {
            audioSource.clip = lightningLoop;
            audioSource.loop = true;
            audioSource.Play();
        }
    }
    void StartVisualEffects()
    {
        if (enableOnFire.GetActive() == false)
        {
            enableOnFire.SetActive(true);
        }
    }

    void EndVisualEffects()
    {
        enableOnFire.SetActive(false);
    }
    void EndSoundEffects()
    {
        audioSource.loop = false;
        audioSource.Stop();
    }
    void dealDamage()
    {
        foreach (GameObject enemy in enemiesInSight)
        {
            GameObject enemyInstance = enemy;
            //Get the current distance from this enemy to us:
            float damageToDeal = damageRampup(damage, enemy);
            Health h = enemyInstance.GetComponent<Health>();

            while (h == null && enemy.transform.parent)
            {
                enemyInstance = enemy.transform.parent.gameObject;
                h = enemyInstance.GetComponent<Health>();
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
                    TeamMember tm = enemyInstance.GetComponent<TeamMember>();
                    TeamMember myTm = this.GetComponent<TeamMember>();
                    if (tm == null || tm.teamID == 0 || myTm == null || myTm.teamID == 0 || tm.teamID != myTm.teamID)
                    {
                        GetComponent<HitSound>().PlaySound(damage);
                        h.GetComponent<PhotonView>().RPC("TakeDamage", PhotonTargets.All, damageToDeal, PhotonNetwork.player.name, damageType);         //RPC

                    }

                }
            }
        }
    }
    float damageRampup(float damage, GameObject hitEnemy) // OUR DAMAGE RAMPUP FUNCTION
    {
        float fltDistance = Vector3.Distance(hitEnemy.transform.position, transform.position);
        float fltNewDamage = damage * (2 * Mathf.Exp(-fltDistance / 4) + 1);
        return fltNewDamage;
    }
    public void UpdateHUD()
    {
        snm.UIClipAmmo.text = ammo.ToString();
        snm.UIReserveAmmo.text = "mF";

    }
    private void OnEnable()
    {
        snm.UIClipAmmo.text = ammo.ToString();
        snm.UIReserveAmmo.text = "mF";

    }

    void fireOuterLightning()
    {
        foreach (Vector3 endPosition in outerEndPosition){
            GameObject outerLightningInstance = outerLightningGameObject;
            outerLightningInstance.GetComponent<LightningBoltScript>().StartObject = localBarrel.gameObject;
            outerLightningInstance.GetComponent<LightningBoltScript>().EndObject.transform.position = endPosition;
            outerLightningInstance.GetComponent<LightningBoltScript>().ChaosFactor = Random.Range(0.01f, 0.1f);
            outerLightningInstance.GetComponent<TimedObjectDestructor>().m_TimeOut = cooldownLightning;
            Instantiate(outerLightningInstance);

        }
    }

    void fireOuterLightningRemote()
    {
        foreach (Vector3 endPosition in outerEndPosition)
        {
            GameObject outerLightningInstance = outerLightningGameObject;
            outerLightningInstance.GetComponent<LightningBoltScript>().StartObject = Barrel.gameObject;
            outerLightningInstance.GetComponent<LightningBoltScript>().EndObject.transform.position = endPosition;
            outerLightningInstance.GetComponent<LightningBoltScript>().ChaosFactor = Random.Range(0.01f, 0.1f);
            outerLightningInstance.GetComponent<TimedObjectDestructor>().m_TimeOut = cooldownLightning;
            Instantiate(outerLightningInstance);

        }
    }
    void fireCentreLightning()
    {
        foreach (GameObject enemy in enemiesInSight)
        {
            GameObject enemyInstance = enemy;
            GameObject centreLightningInstance = centreLightningGameObject;
            centreLightningInstance.GetComponent<LightningBoltScript>().StartObject = localBarrel.gameObject;
           // centreLightningInstance.GetComponent<LightningBoltScript>().EndObject.transform.position = enemy.transform.position;
            centreLightningInstance.GetComponent<LightningBoltScript>().EndObject = enemyInstance;
            centreLightningInstance.GetComponent<TimedObjectDestructor>().m_TimeOut = cooldown;
            Instantiate(centreLightningInstance);
        }
    }
    void FindEnemiesInFireCone()
    {
        enemiesInCone.Clear();
        Collider[] Colliders = Physics.OverlapBox(teslaFireCone.transform.position, teslaFireCone.transform.lossyScale , Quaternion.identity);
        foreach(Collider collider in Colliders)
        {
            GameObject colliderObject = collider.gameObject;
            if(colliderObject.GetComponent<TeamMember>() != null && colliderObject.GetComponent<Health>() != null)
            {
                TeamMember tm = colliderObject.GetComponent<TeamMember>();
                TeamMember myTm = this.GetComponent<TeamMember>();
                if (tm == null || tm.teamID == 0 || myTm == null || myTm.teamID == 0 || tm.teamID != myTm.teamID)
                {
                    // This collider belongs to the enemy! We'll add its gameObject to the array.
                    enemiesInCone.Add(colliderObject);
                }
            }
        }
    }
    void FindEnemiesInSight()
    {
        enemiesInSight.Clear();
        foreach (GameObject enemy in enemiesInCone)
        {
            auxRaycaster.transform.LookAt(enemy.transform);
            Ray ray = new Ray(auxRaycaster.transform.position, auxRaycaster.transform.forward);
            Transform hitTransform;
            Vector3 hitPoint;

            hitTransform = FindClosestHitObject(ray, out hitPoint);
            if(hitTransform.gameObject == enemy)
            {
                // This enemy is in the firing cone AND is in sight, he's ready to be shot!
                enemiesInSight.Add(enemy);
            }
        }
    }

    void CalculateOuterLightningEndVector()
    {
        Debug.Log("Running CalculateOuterLightningEndVector subroutine");
        for (int i = 0; i < outerEndPosition.Length; i++)
        {
            float randomRadius = Random.Range(0, scaleLimit);
            float randomAngle = Random.Range(0, 2 * Mathf.PI);
            Vector3 direction = new Vector3(
                randomRadius * Mathf.Cos(randomAngle),
                randomRadius * Mathf.Sin(randomAngle),
                z
                );
            direction = playerCamera.transform.TransformDirection(direction.normalized);
            Ray ray = new Ray(playerCamera.transform.position, direction);
            RaycastHit hitInfo;
            Vector3 hitPoint;
            Transform hitTransform = FindClosestHitObject(ray, out hitPoint);
            Debug.Log("We just hit " + hitTransform + " with a lightning fork");
            if (Physics.Raycast(ray, out hitInfo, range))
            {

                hitPoint = hitInfo.point;
                Instantiate(sparkParticles, hitPoint, Quaternion.identity);

            }
            else
            {
                int randomNumber = Random.Range(0, GroundingSpots.Length);
                hitPoint = GroundingSpots[randomNumber].transform.position;
            }
            outerEndPosition[i] = hitPoint;
            Debug.Log("We've just calculated a vector for lightning to strike and added it to our array: " + hitPoint);
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
