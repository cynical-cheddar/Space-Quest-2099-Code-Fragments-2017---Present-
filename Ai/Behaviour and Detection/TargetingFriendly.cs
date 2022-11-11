using UnityEngine; 
using System.Collections;

public class TargetingFriendly : MonoBehaviour 
{
	
	// Use this for initialization
	public Transform[] respawns;
	
	void call () 
	{
		GameObject[] respawnObjects = GameObject.FindGameObjectsWithTag ("Enemy");
		respawns = new Transform[respawnObjects.Length];
		
		for ( int i = 0; i < respawns.Length; ++i )
			respawns[i] = respawnObjects[i].transform;
		
		Debug.Log (respawns.Length);
	}
	
	// Update is called once per frame
	void Update () 
	{ 	call ();
		GetClosestEnemy (respawns);
	}
	
	Transform GetClosestEnemy(Transform[] enemies)
	{
		Transform tMin = null;
		float minDist = Mathf.Infinity;
		Vector3 currentPos = transform.position;
		foreach (Transform t in enemies)
		{
			float dist = Vector3.Distance(t.position, currentPos);
			if (dist < minDist)
			{
				tMin = t;
				minDist = dist;
			}
		}
		transform.LookAt(tMin);
		return tMin;
	}
}