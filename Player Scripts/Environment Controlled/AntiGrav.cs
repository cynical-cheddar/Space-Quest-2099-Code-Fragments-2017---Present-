using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AntiGrav : MonoBehaviour {
    public float force = 1100f;
    public float velMax = 5f;
    Rigidbody rb;
    public bool ResetVelocity = true;
    public bool PlaySplashSound = false;
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == ("Player") && other.gameObject.GetComponent<PhotonView>().isMine == true && other.gameObject.GetComponent<CharacterController>().enabled == true)
        {
            // Call a funtion on the player movement script in which the cc does not enable for a short time. Also activates a collider trigger that checks below the playervoid to detect the moment before the player lands.
            if (PlaySplashSound == true)
            {
                other.gameObject.GetComponent<PhotonView>().RPC("SoundEffectPlay", PhotonTargets.All, 0);         //RPC
            }
            other.gameObject.GetComponent<PlayerMovement>().RocketJumping();
            other.gameObject.GetComponent<ConstantForce>().enabled = true;
            if(ResetVelocity == true)
            {
                other.gameObject.GetComponent<Rigidbody>().velocity = new Vector3(0, 1, 0);
            }


            other.gameObject.GetComponent<ConstantForce>().force = new Vector3 (0, force, 0);
            rb = other.gameObject.GetComponent<Rigidbody>();
        }
    }
    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.tag == ("Player") && other.gameObject.GetComponent<PhotonView>().isMine == true && other.gameObject.GetComponent<CharacterController>().enabled == false)
        {
            // Call a funtion on the player movement script in which the cc does not enable for a short time. Also activates a collider trigger that checks below the playervoid to detect the moment before the player lands.
            if(rb.velocity.y > velMax)
            {
                other.gameObject.GetComponent<ConstantForce>().force = new Vector3(0, 0, 0);
            }
            else
            {
                other.gameObject.GetComponent<ConstantForce>().force = new Vector3(0, force, 0);
            }
           }
        }
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == ("Player") && other.gameObject.GetComponent<PhotonView>().isMine == true && other.gameObject.GetComponent<CharacterController>().enabled == false)
        {
            // Call a funtion on the player movement script in which the cc does not enable for a short time. Also activates a collider trigger that checks below the playervoid to detect the moment before the player lands.
            other.gameObject.GetComponent<ConstantForce>().enabled = false;
            other.gameObject.GetComponent<ConstantForce>().force = new Vector3(0, 0, 0);
        }
    }
}
