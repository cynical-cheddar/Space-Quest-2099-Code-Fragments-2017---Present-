using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AiDetection : MonoBehaviour {

	public float fltDetectionRadius = 10f;
	public float fltDetectionRadiusIfHit = 200f;
	public float dotProductThreshold = 0.7f;
	public bool alertNearbyAlliesIfHit = true;
	public float fltNearbyAlliesAlertRadius = 20f;

	public int[] detectionIDs;
	public List<Transform> playersInRange;
	public List<Transform> playersInVision;
	public bool playerSeen = false;

	public float fltRaycastCooldown = 0.5f;
	float cooldown = 1f;
	AiBehaviour beahaviourScript;
	// Update is called once per frame
	void Start(){
		beahaviourScript = GetComponent<AiBehaviour> ();

	}
	void Update () {

		// Once a player has been seen, chase em.
		cooldown -= Time.deltaTime;
		if (cooldown <= 0 && PhotonNetwork.isMasterClient && playerSeen == false) {
			getPlayersInRange ();
			if (playersInRange.Count > 0) {
				getPlayersInVision ();
			}
			if (playerSeen == true) {
				if (PhotonNetwork.isMasterClient) {
					beahaviourScript.setTarget (playersInVision [0]);
				}
			}
			cooldown = fltRaycastCooldown;
		}

	}
    public void clearLists()
    {
        playersInRange.Clear();
        playersInVision.Clear();
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



	void getPlayersInRange(){
		Collider[] detectedColliders = Physics.OverlapSphere (transform.position, fltDetectionRadius);
		// iterate through detected colliders and get the ones that are players and have a team ID
		playersInRange.Clear();
		foreach(Collider col in detectedColliders){
			if(col.gameObject.tag == "Player"){
				for(int i = 0; i < detectionIDs.Length; i++){
                    if(col.gameObject.GetComponent<TeamMember>() != null) {
                        if (col.gameObject.GetComponent<TeamMember>().teamID == detectionIDs[i] && col.gameObject != gameObject && !playersInRange.Contains(col.gameObject.transform))
                        {
                            playersInRange.Add(col.gameObject.transform);
                        }
			}
		}
	}
}
}
	void getPlayersInVision(){
		playersInVision.Clear ();
		foreach(Transform player in playersInRange){
			// calculate dot product
			Vector3 direction = Vector3.Normalize(player.position - transform.position);
			float dotProduct = Vector3.Dot (direction, transform.forward);
			if (dotProduct >= dotProductThreshold) { // is the player in our vision cone?
				if (hasLineOfSight (player.gameObject)) {		// have we also got a direct line of sight to our player?
					playersInVision.Add (player);
					playerSeen = true;
				}
			}
	}
}

	public void alertIfShot(){
		if (beahaviourScript.getHasTarget() == false) {
			if (getClosestPlayer() != transform) {
				beahaviourScript.setTarget(getClosestPlayer());
			}
			if (alertNearbyAlliesIfHit) {
				// get an overlap of all nearby allies and alert them that we have been hit. they will then alert their friends in turn
				Collider[] detectedColliders = Physics.OverlapSphere (transform.position, fltNearbyAlliesAlertRadius);
				foreach(Collider col in detectedColliders){
					if(col.gameObject.tag == "Player"){
						if(col.gameObject.GetComponent<TeamMember>().teamID == GetComponent<TeamMember>().teamID && col.gameObject != gameObject ){
							col.gameObject.GetComponent<AiDetection> ().alertIfShot ();
						}
					}
				}
			}
		}
	}

	public Transform getClosestPlayer(){
		Transform minDistPlayer = transform;
		Collider[] detectedColliders = Physics.OverlapSphere (transform.position, fltDetectionRadiusIfHit);
		List<Transform> playersInHitRange = playersInRange;
		playersInHitRange.Clear ();
		// iterate through detected colliders and get the ones that are players and have a team ID
		foreach(Collider col in detectedColliders){
			if(col.gameObject.tag == "Player"){
				for(int i = 0; i < detectionIDs.Length; i++){
					if(col.gameObject.GetComponent<TeamMember>().teamID == detectionIDs[i] && col.gameObject != gameObject && !playersInRange.Contains(col.gameObject.transform)){
						playersInHitRange.Add (col.gameObject.transform);
					}
				}
			}
		}
		foreach (Transform player in playersInHitRange) {

			float minDist = fltDetectionRadiusIfHit;
			if(Vector3.Magnitude(transform.position - player.position) < minDist){
				minDist = Vector3.Magnitude (transform.position - player.position);
				minDistPlayer = player;
		}
	}
		if (minDistPlayer != null) {
			return minDistPlayer;
		} else {
			return transform;
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
