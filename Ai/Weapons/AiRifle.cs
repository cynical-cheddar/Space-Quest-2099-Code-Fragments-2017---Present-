using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AiRifle : AiWeapon {

	public Transform barrel;
	Animator anim;
	public int weaponID = 0;
	public float damage = 20f;


	public float fireDelay = 0.4f;
	public float volume = 0.5f;
	public string damageType = "Normal";


	Vector3 currentTarget;
	Vector3 direction;

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
	void Start () {
		Transform animParent = this.transform.Find("PlayerModel");
		anim = animParent.gameObject.GetComponent<Animator>();
	}
	public override void fire(Vector3 target, Vector3 dir){
		currentTarget = target;
		direction = dir;
		Invoke ("actuallyFire", fireDelay);
	}



	void actuallyFire(){
		Debug.Log ("BLAM Recieved!!");
		this.GetComponent<PhotonView>().RPC("ShootAnimation", PhotonTargets.All);
		barrel.transform.LookAt (currentTarget);
		///-------
		/// 
		Ray ray = new Ray(transform.position, direction);
		RaycastHit hit; //From camera to hitpoint, not as curent
		Transform hitTransform;
		Vector3 hitVector;
		hitTransform = FindClosestHitObject(ray, out hitVector);

		// we should do a new raycast in the direction we were given to find out where we hit with our server streak
		// Also make streak louder
		currentBeam = 0;
		beamStart = Instantiate(beamStartPrefab[currentBeam], barrel.position, Quaternion.identity) as GameObject;
		beamEnd = Instantiate(beamEndPrefab[currentBeam], barrel.position, Quaternion.identity) as GameObject;
		beam = Instantiate(beamLineRendererPrefab[currentBeam], barrel.position, Quaternion.identity) as GameObject;
		line = beam.GetComponent<LineRenderer>();
		this.GetComponent<PhotonView>().RPC("CreateServerStreak", PhotonTargets.Others, currentBeam);         //RPC
		this.GetComponent<PhotonView>().RPC("StreakPositionServer", PhotonTargets.All, hitVector);         //RPC

		Fire();
		///--------------------
		this.GetComponent<PhotonView>().RPC("rpcAiFireRifle", PhotonTargets.All, hitVector);
	}

    [PunRPC]
    void CreateServerStreak(int currentBeam)
    {
        beamStart = Instantiate(beamStartPrefab[currentBeam], barrel.position, Quaternion.identity) as GameObject;
        beamEnd = Instantiate(beamEndPrefab[currentBeam], barrel.position, Quaternion.identity) as GameObject;
        beam = Instantiate(beamLineRendererPrefab[currentBeam], barrel.position, Quaternion.identity) as GameObject;
        line = beam.GetComponent<LineRenderer>();
    }

    [PunRPC]
	public void StreakPositionServer(Vector3 hitPoint) // We need to somehow interpolate the hit position between updates.
	{
		line.SetVertexCount(2);

		beamStart.transform.position = barrel.transform.position;
		line.SetPosition(0, barrel.transform.position);

		line.SetPosition(1, hitPoint);
		beamEnd.transform.position = hitPoint;
		beamStart.transform.LookAt(beamEnd.transform.position);
		beamEnd.transform.LookAt(beamStart.transform.position);
		StartCoroutine(RPCbeamstart());
		float distance = Vector3.Distance(barrel.transform.position, hitPoint);
		line.sharedMaterial.mainTextureScale = new Vector2(distance / textureLengthScale, 1);
		line.sharedMaterial.mainTextureOffset -= new Vector2(Time.deltaTime * textureScrollSpeed, 0);
	}
	[PunRPC]
	void rpcAiFireRifle(Vector3 currentTarget){
		barrel.transform.LookAt(currentTarget); //REAL BARREL
	}

	IEnumerator RPCbeamstart() // We need to pass this the old and new vector positions of the hitpoint.
	{
		float totalTime = 0.2f;
		float elapsedTime = 0.0f;
		while (totalTime > elapsedTime)
		{
			elapsedTime += Time.deltaTime;
			beamStart.transform.position = barrel.position;
			yield return null;
		}

	}



	void Fire()
	{

		Ray ray = new Ray(transform.position, currentTarget - transform.position);
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
					if (tm == null || tm.teamID == 0 || myTm == null || myTm.teamID == 0)
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
