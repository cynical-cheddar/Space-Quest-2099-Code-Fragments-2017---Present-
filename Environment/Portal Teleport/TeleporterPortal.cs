using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeleporterPortal : MonoBehaviour {
    public bool isLinked = true;
    public Transform linkedPortal;
    public Transform exitDirection;
    public GameObject teleFX;
    public bool justTeleported = false;
    // Use this for initialization

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag == "Player" && other.gameObject.GetComponent<PhotonView>().isMine == true && other.gameObject.GetComponent<teleportFlag>().canTeleport == true && justTeleported == false)
        {
            Vector3 pos = other.transform.position;
            Quaternion rot = other.transform.rotation;
            other.gameObject.GetComponent<teleportFlag>().canTeleport = false;
            justTeleported = true;
            if (isLinked == false)
            {
                justTeleported = false;
            }
            other.transform.position = linkedPortal.transform.position;
            other.transform.rotation = linkedPortal.GetComponent<TeleporterPortal>().exitDirection.rotation;
            this.GetComponent<PhotonView>().RPC("fx", PhotonTargets.All, pos, rot);


        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Player" && other.gameObject.GetComponent<PhotonView>().isMine == true && other.gameObject.GetComponent<teleportFlag>().canTeleport == false && justTeleported == false)
        {
            other.gameObject.GetComponent<teleportFlag>().canTeleport = true;
            linkedPortal.GetComponent<TeleporterPortal>().justTeleported = false;
        }
    }
    [PunRPC]
    void fx(Vector3 pos, Quaternion rot)
    {
        Instantiate(teleFX, pos, rot);
    }
}
