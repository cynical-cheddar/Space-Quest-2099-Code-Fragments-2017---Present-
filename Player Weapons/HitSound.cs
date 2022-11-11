using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitSound : MonoBehaviour {

    public AudioClip hitSound;
	// Use this for initialization
	public void PlaySound (float damage) {
        AudioSource audioSource = GetComponent<AudioSource>();
        audioSource.clip = hitSound;
        float prevVol = audioSource.volume;
        audioSource.volume = 1;
        float spacialBlend = audioSource.spatialBlend;
        audioSource.spatialBlend = 0;
        if (damage < 10)
        {
            audioSource.pitch = 1.5f;
        }
        if ( damage <= 10 && damage < 25)
        {
            audioSource.pitch = 1.2f;
        }
        if (damage <=25 && damage < 50)
        {
            audioSource.pitch = 1f;
        }
        if (damage <= 50 && damage < 100)
        {
            audioSource.pitch = 0.8f;
        }

        audioSource.Play();

        audioSource.volume = prevVol;
        audioSource.pitch = 1;
        audioSource.spatialBlend = spacialBlend;
    }
}
