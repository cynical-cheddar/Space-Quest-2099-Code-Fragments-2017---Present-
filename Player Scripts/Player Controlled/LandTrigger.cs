using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LandTrigger : MonoBehaviour {

    public GameObject player;

	void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag == "Terrain" || other.gameObject.tag == "Slide")
        {
            Debug.Log("Hit Terrain");
            PlayerMovement pm = player.GetComponent<PlayerMovement>();
            pm.EndRocketJump();
        }
    }
}
