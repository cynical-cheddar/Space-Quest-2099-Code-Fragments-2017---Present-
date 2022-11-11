using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightIntensity : MonoBehaviour {
    Light lt;
    public float time = 1;
    float startIntensity;
	// Use this for initialization
	void Start () {
        lt = this.GetComponent<Light>();
        startIntensity = lt.intensity;

    }
	
	// Update is called once per frame
	void Update () {
        lt.intensity = Mathf.Lerp(startIntensity, 0, time);
    }
}
