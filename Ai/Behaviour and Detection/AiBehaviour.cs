using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
public class AiBehaviour : MonoBehaviour {

	public float cooldown;
	public float fireRate = 1f;
	float prevSpeed;
	public virtual void setTarget (Transform target){
	}
   
	public virtual void unclaimCover (){
	}
	public virtual bool setHasTarget (bool hasTarget){
		return false;
	}
	public virtual bool getHasTarget (){
		return false;
	}
	public void applyPain(){
		prevSpeed = GetComponent<NavMeshAgent> ().speed;
		GetComponent<NavMeshAgent> ().speed = 0f;
		cooldown = fireRate;
		Invoke ("restoreSpeed", 0.5f);
	}
	void restoreSpeed(){
		GetComponent<NavMeshAgent> ().speed = prevSpeed;
	}
    public virtual void removeTarget()
    {

    }

}
