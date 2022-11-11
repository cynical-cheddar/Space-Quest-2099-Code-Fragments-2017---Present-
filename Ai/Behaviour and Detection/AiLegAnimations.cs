using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AiLegAnimations : MonoBehaviour {

	Animator anim;
	Vector3 lastPos = Vector3.zero;
	// Use this for initialization
	void Start () {

		Transform animParent = this.transform.Find("PlayerModel");
		anim = animParent.gameObject.GetComponent<Animator>();

	}
	
	// Update is called once per frame
	void Update () {
		float speed = (transform.position - lastPos).magnitude;
		lastPos = transform.position;
		anim.SetFloat("Speed", speed);
	}
}
