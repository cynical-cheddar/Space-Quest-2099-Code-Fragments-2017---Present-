using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
public class AiMoveShoot : AiBehaviour {

	public Transform currentTarget;
	public float shootingRange = 10f;
	public float rotationSpeed = 5f;
	//public float fireRate = 1f;
	public float inaccuracy = 0.5f;

	public bool fireWhileRunning = true;
	public float fireRateWhileRunning = 2f;
	public float runningInaccuracy = 2f;

    float cooldownTargetMove = 2f;
    float cooldownMax = 2f;
    public float rangeCutOff = 2f;
	//public float cooldown;
	public AiWeapon weaponScript;
	// A very basic AI
	// Moves towards the player and shoots when in range
	NavMeshAgent agent;

	void Start () {
		agent = GetComponent<NavMeshAgent> ();
        cooldownMax = cooldownMax += Random.Range(0f, 0.5f);
	}


	public override void setTarget(Transform target){
		//cooldown = fireRate;
		currentTarget = target;
		agent.destination = target.position;
	}
    public override void removeTarget()
    {
        currentTarget = transform;
    }
    public override bool getHasTarget(){
		if(currentTarget == null || currentTarget == transform)
        {
			return false;
		}
		else{
			return true;
		}
	}


	void Update () {
		if(currentTarget != null && currentTarget != transform){
            cooldownTargetMove -= Time.deltaTime;
        if(cooldownTargetMove <= 0f)
            {
                agent.destination = currentTarget.position;
                cooldownTargetMove = cooldownMax;
            }
		if (Vector3.Magnitude (transform.position - currentTarget.position) <= shootingRange && Vector3.Magnitude(transform.position - currentTarget.position) >= rangeCutOff) {
			// We are in range, look at the target
			Vector3 dir = currentTarget.position - transform.position;
			Quaternion toRotation = Quaternion.LookRotation (dir);
			transform.rotation = Quaternion.Lerp (transform.rotation, toRotation, rotationSpeed * Time.deltaTime);
			// We should fire if we are on cooldown
			cooldown -= Time.deltaTime;
			if(cooldown <= 0){
				Debug.Log ("BLAM!!");
				//Check if the line of sight is clear before shooting
				if(hasLineOfSight(currentTarget.gameObject)){
					dir.x += Random.Range (inaccuracy, -inaccuracy) /100;
					dir.y += Random.Range (inaccuracy, -inaccuracy)/100;
					dir.z += Random.Range (inaccuracy, -inaccuracy)/100;
					Ray ray = new Ray(transform.position, dir);
					RaycastHit hit; //From camera to hitpoint, not as curent
					Transform hitTransform;
					Vector3 hitVector;
					hitTransform = FindClosestHitObject(ray, out hitVector);
					
					if (hitTransform != transform) {
							GetComponent<AiDetection> ().alertIfShot ();
							weaponScript.fire (hitVector, dir);
					}
					cooldown = fireRate;
				}
			}
	}
		else if(Vector3.Magnitude (transform.position - currentTarget.position) >= shootingRange && fireWhileRunning ){
			// We should fire if we are on cooldown
			cooldown -= Time.deltaTime;
				if (cooldown <= 0) {
					if (hasLineOfSight (currentTarget.gameObject)) {
						Vector3 dir = currentTarget.position - transform.position;
						dir.x += Random.Range (runningInaccuracy, -runningInaccuracy) / 100;
						dir.y += Random.Range (runningInaccuracy, -runningInaccuracy) / 100;
						dir.z += Random.Range (runningInaccuracy, -runningInaccuracy) / 100;

						Debug.Log ("BLAM!!");
						//Check if the line of sight is clear before shooting
						Ray ray = new Ray (transform.position, dir);

						RaycastHit hit; //From camera to hitpoint, not as curent
						Transform hitTransform;
						Vector3 hitVector;
						hitTransform = FindClosestHitObject (ray, out hitVector);
						//
						if (hitTransform != transform) {
							GetComponent<AiDetection> ().alertIfShot ();
							weaponScript.fire (hitVector, dir);
						}
						cooldown = fireRateWhileRunning;
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
	bool hasLineOfSight(GameObject player){
		// Do a raycast to see if the player in range actually is in our line of sight.

		Vector3 dir = player.transform.position - transform.position;

	//	Debug.Log("Checking line of sight");

		Ray ray = new Ray(transform.position, dir);
		RaycastHit hit;
		Transform hitTransform;
		Vector3 hitVector;
		hitTransform = FindClosestHitObject(ray, out hitVector);

	//	Debug.Log ("We have raycasted");
		if (hitTransform != null) {
		//	Debug.Log (player);
		//	Debug.Log (hitTransform);
			if (hitTransform.gameObject == player || hitTransform.gameObject.tag == "Player") {
				return true;
			}
		}
		return false;
	}


}
