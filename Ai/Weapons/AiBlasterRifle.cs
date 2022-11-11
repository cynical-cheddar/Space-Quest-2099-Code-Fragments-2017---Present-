using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
public class AiBlasterRifle : AiWeapon {


	public Transform barrel;
	Animator anim;
	public int weaponID = 0;
	public float damage = 20f;
	public GameObject projectile;
	public float projectileVelocity = 2000f;
	public float volume = 0.5f;
	public string damageType = "Normal";
    public bool stopWhenShoot = false;
    float prevSpeed = 8f;
	// Use this for initialization
	void Start () {
		Transform animParent = this.transform.Find("PlayerModel");
		anim = animParent.gameObject.GetComponent<Animator>();
        prevSpeed = GetComponent<NavMeshAgent>().speed;

    }
    void restoreSpeed()
    {
        GetComponent<NavMeshAgent>().speed = prevSpeed;
    }
	public override void fire(Vector3 target, Vector3 dir){
        if (stopWhenShoot)
        {

            GetComponent<NavMeshAgent>().speed = 0;
            Invoke("restoreSpeed", 0.5f);
        }
		Debug.Log ("BLAM Recieved!!");
		this.GetComponent<PhotonView>().RPC("ShootAnimation", PhotonTargets.All);
		ProjectileScript projScript;
		barrel.transform.LookAt (target);
		GameObject proj = Instantiate(projectile, barrel.position, barrel.rotation);
		projScript = proj.GetComponent<ProjectileScript>();
		Physics.IgnoreCollision(GetComponent<CapsuleCollider>(), proj.GetComponent<BoxCollider>());
		projScript.sourcePlayer = gameObject;
		projScript.DealsDamage = true;
		projScript.damage = damage;
		projScript.damageType = damageType;
		proj.GetComponent<Rigidbody>().AddForce(proj.transform.forward * projectileVelocity);
		proj.GetComponent<ProjectileScript>().volume = volume;

		this.GetComponent<PhotonView>().RPC("rpcAiFireBlasterRifle", PhotonTargets.Others, target);
	}

	[PunRPC]
	void rpcAiFireBlasterRifle(Vector3 target){
		barrel.transform.LookAt(target); //REAL BARREL

		GameObject proj = Instantiate(projectile, barrel.position, barrel.rotation);
		//Physics.IgnoreCollision(GetComponent<CharacterController>(), proj.GetComponent<BoxCollider>());
		Physics.IgnoreCollision(GetComponent<CapsuleCollider>(), proj.GetComponent<BoxCollider>());

		proj.GetComponent<ProjectileScript>().volume = volume;
		proj.GetComponent<Rigidbody>().AddForce(proj.transform.forward * projectileVelocity);
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
