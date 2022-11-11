using UnityEngine;
using System.Collections;

public class ProjectileScript : MonoBehaviour
{
    public GameObject impactParticle;
    public GameObject projectileParticle;
    public GameObject muzzleParticle;
    public GameObject muzzleParticleInstance;
    public GameObject[] trailParticles;
    public bool DealsDamage = false;
    public string damageType = "Normal";
    public string playerSourceName = "";
    public GameObject sourcePlayer;
    public bool Explosive = false;
    public float damage = 10f;
    public float ExplosiveDamage = 0f;
    public float ExplosionRadius = 0f;
    public float ExplosiveForce = 0f;
    public bool local2dAudio = false;
    public int projectileTeamID = 0; //Damages all playes by default
    public bool muzzleFlashLocal = false;
    public float volume = 1f;
    public bool IsGrenade = false;
    public float grenadeTimer = 2f;
    float remainingTime = 2f;
    bool bounced = false;
    [HideInInspector]
    public Vector3 impactNormal; //Used to rotate impactparticle.

    private bool hasCollided = false;

    void Start()
    {
		gameObject.layer = 11;
		Physics.IgnoreLayerCollision (11, 11);
        remainingTime = grenadeTimer;
        projectileParticle = Instantiate(projectileParticle, transform.position, transform.rotation) as GameObject;
        projectileParticle.transform.parent = transform;
        projectileParticle.GetComponent<AudioSource>().volume = volume;
            muzzleParticleInstance = Instantiate(muzzleParticle, transform.position, transform.rotation);
            muzzleParticleInstance.transform.rotation = transform.rotation * Quaternion.Euler(180, 0, 0);
       // Debug.Log("muzzleParticle");
            //    muzzleParticle.GetComponent<AudioSource>().volume = volume;

              Destroy(muzzleParticleInstance, 1.5f); // Lifetime of muzzle effect.
        if (muzzleFlashLocal)
        {
            muzzleParticleInstance.transform.parent = sourcePlayer.transform.Find("FirstPersonCharacter");
        }

    }
    private void Update()
    {
        if (IsGrenade == true && bounced == true)
        {
            remainingTime -= Time.deltaTime;
            if(remainingTime <= 0)
            {
                IsGrenade = false;
                if (Explosive == true) //If explosive is selected on shooter's end
                {
                    impactParticle = Instantiate(impactParticle, transform.position, Quaternion.FromToRotation(Vector3.up, impactNormal)) as GameObject;
                    if (GetComponent<PhotonView>().isMine == true && local2dAudio == true)
                    {
                        impactParticle.GetComponent<AudioSource>().spatialBlend = 0f;
                    }
                    impactParticle.GetComponent<AudioSource>().volume = volume;


                    if (PhotonNetwork.offlineMode == true)
                    {
                        sourcePlayer.GetComponent<ExplosionInstantiator>().offlineExp(transform.position, transform.rotation, ExplosiveDamage, ExplosionRadius, ExplosiveForce, damage, projectileTeamID, playerSourceName, damageType);
                    }
                    else
                    {
                        sourcePlayer.GetComponent<PhotonView>().RPC("InstantiateExplosion", PhotonTargets.All, transform.position, transform.rotation, ExplosiveDamage, ExplosionRadius, ExplosiveForce, damage, projectileTeamID, playerSourceName, damageType);
                    }


                }
                if (!muzzleParticle)
                {
                    Destroy(gameObject);
                }
                Destroy(projectileParticle, 3f);
                Destroy(impactParticle, 5f);
                Destroy(gameObject);
            }
        }
    }

    public void MuzzleLocal(GameObject player)
    {
        if (muzzleFlashLocal == true)
        {
            muzzleParticleInstance.transform.parent = player.transform.Find("FirstPersonCharacter");
        }
    }

    void OnCollisionEnter(Collision hit)
    {
		if (hit.gameObject.GetComponent<ProjectileScript> () != null) {
			Physics.IgnoreCollision (hit.collider, GetComponent<Collider> ());
		} else {
			bounced = true;
            if(hit.gameObject.tag == "Player")
            {
                GetComponent<Rigidbody>().mass = 0f;
            }
			if (!hasCollided && gameObject.GetComponent<Collider> () != null && IsGrenade == false) {
				collided (hit);
			}
			if (!hasCollided && gameObject.GetComponent<Collider> () != null && hit.gameObject.tag == "Player" && IsGrenade == true) {
				collided (hit);
                Physics.IgnoreCollision(hit.collider, GetComponent<Collider>());
            }
   
        }
    }


    void collided(Collision hit)
    {
        Invoke("ensuredead", 0.1f);
        hasCollided = true;
        //transform.DetachChildren();
        impactParticle = Instantiate(impactParticle, transform.position, Quaternion.FromToRotation(Vector3.up, impactNormal)) as GameObject;
        if (GetComponent<PhotonView>().isMine == true && local2dAudio == true)
        {
            impactParticle.GetComponent<AudioSource>().spatialBlend = 0f;
        }
        impactParticle.GetComponent<AudioSource>().volume = volume;
        //Debug.DrawRay(hit.contacts[0].point, hit.contacts[0].normal * 1, Color.yellow);

        if (hit.gameObject.tag == "Destructible") // Projectile will destroy objects tagged as Destructible
        {
            Destroy(hit.gameObject);
        }


        //yield WaitForSeconds (0.05);
        foreach (GameObject trail in trailParticles)
        {
            GameObject curTrail = transform.Find(projectileParticle.name + "/" + trail.name).gameObject;
            curTrail.transform.parent = null;
            Destroy(curTrail, 3f);
        }
        if (Explosive == true) //If explosive is selected on shooter's end
        {
            if (PhotonNetwork.offlineMode == true)
            {
                sourcePlayer.GetComponent<ExplosionInstantiator>().offlineExp(transform.position, transform.rotation, ExplosiveDamage, ExplosionRadius, ExplosiveForce, damage, projectileTeamID, playerSourceName, damageType);
            }
            else
            {
                sourcePlayer.GetComponent<PhotonView>().RPC("InstantiateExplosion", PhotonTargets.All, transform.position, transform.rotation, ExplosiveDamage, ExplosionRadius, ExplosiveForce, damage, projectileTeamID, playerSourceName, damageType);
            }


        }
        if (DealsDamage == true)
        {
            Transform hitTransform = hit.transform;
            Health h = hit.gameObject.GetComponent<Health>();

            while (h == null && hitTransform.parent)
            {
                hitTransform = hitTransform.parent;
                h = hitTransform.GetComponent<Health>();
            }
            if (h != null)
            {
                TeamMember tm = hitTransform.gameObject.GetComponent<TeamMember>();
                if (tm.teamID == 0 || projectileTeamID == 0 || tm.teamID != projectileTeamID)
                {
                    sourcePlayer.GetComponent<HitSound>().PlaySound(damage);
                    h.GetComponent<PhotonView>().RPC("TakeDamage", PhotonTargets.All, damage, playerSourceName, damageType);         //RPC

                }

            }
        }

        if (!muzzleParticle)
        {
            Destroy(gameObject);
        }
        Destroy(projectileParticle, 3f);
        Destroy(impactParticle, 5f);
        Destroy(gameObject);

        //projectileParticle.Stop();

    }
    public void physicsSettings()
    {
        Physics.IgnoreCollision(sourcePlayer.GetComponent<CharacterController>(), GetComponent<Collider>());
        Physics.IgnoreCollision(sourcePlayer.GetComponent<Collider>(), GetComponent<Collider>());
    }
    void ensuredead()
    {
        Destroy(gameObject);
    }
}