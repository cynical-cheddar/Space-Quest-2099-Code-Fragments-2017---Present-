using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AiSetAnimationState : MonoBehaviour {

	public int defaultWeaponId = 0;

	void Start(){
		setWeaponAnimation (defaultWeaponId);
	}

	// Use this for initialization
	public void setWeaponAnimation(int id){
		Animator anim;
		Transform animParent = this.transform.Find ("PlayerModel");
		anim = animParent.gameObject.GetComponent<Animator> ();
		anim.SetInteger ("WeaponType", id);
	
	}
}
