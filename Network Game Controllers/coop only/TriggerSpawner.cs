using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerSpawner : MonoBehaviour {

	public SpawnAI[] aiSpawner;
	bool triggered = false;
	void OnTriggerEnter(Collider collider){
		if (collider.gameObject.tag == "Player" && triggered == false) {
			triggered = true;
			if (PhotonNetwork.isMasterClient) {
				foreach (SpawnAI spawner in aiSpawner) {
					spawner.spawnEnemy ();
				}
			}

		}
	}
	}
