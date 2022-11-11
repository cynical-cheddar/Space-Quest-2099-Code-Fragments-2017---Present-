using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AiCoverBurstFire : AiBehaviour {

	public Transform claimedCover;
	public float coverDistanceToRun = 100f;

	public Transform currentTarget;
	public float shootingRange = 10f;
	public float rotationSpeed = 5f;
	public int shotsPerBurst = 5;
	//public float fireRate = 1f;


	public List<Transform> coverPositions;


	public bool fireWhileRunning = true;
	public float reloadTime = 5f;
	public float fireRateDuringBurst = 0.5f;
	public float runningInaccuracy = 40f;

	//public float cooldown;
	public AiWeapon weaponScript;

	Vector3 direction;

	NavMeshAgent agent;

	void Start () {
		agent = GetComponent<NavMeshAgent> ();
		CoverWaypoint[] waypoints = Transform.FindObjectsOfType<CoverWaypoint>();
		foreach (CoverWaypoint w in waypoints) {
			coverPositions.Add (w.transform);
		}
	}

	public override void unclaimCover (){
		claimedCover.GetComponent<CoverWaypoint> ().releaseClaim ();
	}


	public override void setTarget(Transform target){
		currentTarget = target;
		// When we set our target, we also run for cover
		if(claimedCover == null){
			Transform targetCover = GetClosestFreeCover(coverPositions);
			if (targetCover != null) {
				targetCover.gameObject.GetComponent<CoverWaypoint> ().layClaim (gameObject);
				claimedCover = targetCover;
				// Set our destination to the cover
				agent.destination = claimedCover.position;
			}
		}
	}

	public override bool getHasTarget(){
		if(currentTarget == null){
			return false;
		}
        if (currentTarget == transform)
        {
            return false;
        }
        else
        {
			return true;
		}
	}

	Transform GetClosestFreeCover(List<Transform> cover)
	{
		Transform tMin = null;
		float minDist = coverDistanceToRun;
		Vector3 currentPos = transform.position;
		foreach (Transform t in cover)
		{
			float dist = Vector3.Distance(t.position, currentPos);
			if (dist < minDist && t.gameObject.GetComponent<CoverWaypoint>().isFree == true)
			{
				tMin = t;
				minDist = dist;
			}
		}
		return tMin;
	}

	void shoot(Vector3 dir){

		GetComponent<AiDetection> ().alertIfShot ();

		direction = dir;
		//
		for (int i = 0; i < shotsPerBurst; i++) {
			Invoke ("actuallyShoot", i * fireRateDuringBurst);
		}
		cooldown = reloadTime;
	}

	void actuallyShoot(){






		Ray ray = new Ray(transform.position, direction);
		RaycastHit hit; //From camera to hitpoint, not as curent
		Transform hitTransform;
		Vector3 hitVector;
		hitTransform = FindClosestHitObject(ray, out hitVector);

		if (hitTransform != transform) {
			this.GetComponent<PhotonView>().RPC("ShootAnimation", PhotonTargets.All);
			weaponScript.fire (hitVector, direction);
		}
	}
	void Update () {
		if(currentTarget != null){
			Vector3 dir = currentTarget.position - transform.position;
			if (Vector3.Magnitude (transform.position - currentTarget.position) <= shootingRange && (agent.velocity.magnitude < 1 || fireWhileRunning)) {
				// We are in range, look at the target

				Quaternion toRotation = Quaternion.LookRotation (dir);
				transform.rotation = Quaternion.Lerp (transform.rotation, toRotation, rotationSpeed * Time.deltaTime);
				// We should fire if we are on cooldown
				cooldown -= Time.deltaTime;
				if (cooldown <= 0) {
					//Check if the line of sight is clear before shooting
					if (hasLineOfSight (currentTarget.gameObject)) {
						shoot (dir);
					}
				}
			}
			// if we are not in firing range, we have no cover, but we do have a target. Shoot run towards the enemy.
			else if (Vector3.Magnitude (transform.position - currentTarget.position) >= shootingRange && currentTarget != null && claimedCover == null) {
				agent.destination = currentTarget.position;
				cooldown -= Time.deltaTime;
				// move towards the player
			} else if (Vector3.Magnitude (transform.position - currentTarget.position) >= shootingRange) {
				Quaternion toRotation = Quaternion.LookRotation (dir);
				transform.rotation = Quaternion.Lerp (transform.rotation, toRotation, rotationSpeed * Time.deltaTime);
			}

			// if we have no cover then check if some cover is free. If it is, take it.
			if (claimedCover == null) {
				Transform targetCover = GetClosestFreeCover(coverPositions);
				if (targetCover != null) {
					targetCover.gameObject.GetComponent<CoverWaypoint> ().layClaim (gameObject);
					claimedCover = targetCover;
					// Set our destination to the cover
					agent.destination = claimedCover.position;
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
	bool hasLineOfSight(GameObject player){
		// Do a raycast to see if the player in range actually is in our line of sight.

		Vector3 dir = player.transform.position - transform.position;

		Debug.Log("Checking line of sight");

		Ray ray = new Ray(transform.position, dir);
		RaycastHit hit;
		Transform hitTransform;
		Vector3 hitVector;
		hitTransform = FindClosestHitObject(ray, out hitVector);

		Debug.Log ("We have raycasted");
		if (hitTransform != null) {
			Debug.Log (player);
			Debug.Log (hitTransform);
			if (hitTransform.gameObject == player) {
				return true;
			}
		}
		return false;
	}


}
