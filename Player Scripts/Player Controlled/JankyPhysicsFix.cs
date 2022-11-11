using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JankyPhysicsFix : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        Vector3 v = transform.rotation.eulerAngles;
        transform.rotation = Quaternion.Euler(0, v.y, 0);
    }
}
