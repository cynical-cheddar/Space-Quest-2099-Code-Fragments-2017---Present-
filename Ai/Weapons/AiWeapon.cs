using System.Collections;
using System.Collections.Generic;
using UnityEngine;

abstract public class AiWeapon : MonoBehaviour {


	//public Transform barrel;
	//Animator anim;
	//public int weaponID = 0;
	//public float damage = 20f;


	// Use this for initialization

	abstract public void fire (Vector3 target, Vector3 dir);



}
