using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChanceAudio : MonoBehaviour {

    // Use this for initialization
    public int chanceOutOf = 100;
	void Start () {
        AudioSource source = GetComponent<AudioSource>();
        int randomValue = Random.Range(1, chanceOutOf);
        if(randomValue == 1)
        {
            source.Play();
        }
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
