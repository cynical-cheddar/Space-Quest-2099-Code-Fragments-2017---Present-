using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WankPortal : MonoBehaviour {

    public bool isLinked = true;
    public Transform[] linkedPortals;
    Transform linkedPortal;
    public Transform exitDirection;
    public GameObject teleFX;
    public bool justTeleported = false;
    Vector3 pos;
                    Quaternion rot;
    // Use this for initialization

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player" && other.gameObject.GetComponent<PhotonView>().isMine == true && other.gameObject.GetComponent<teleportFlag>().canTeleport == true && justTeleported == false)
        {

            determineLinkedPortal();
             pos = other.transform.position;
             rot = other.transform.rotation;
            Invoke("engageRPC", 0.0f);
            other.gameObject.GetComponent<teleportFlag>().canTeleport = false;
            justTeleported = true;
            if (isLinked == false)
            {
                justTeleported = false;
            }
            other.transform.position = linkedPortal.transform.position;
            pos = other.transform.position;
            rot = other.transform.rotation;
            Invoke("engageRPC", 0.0f);
            other.transform.rotation = linkedPortal.GetComponent<TeleporterPortal>().exitDirection.rotation;



        }
    }
    void determineLinkedPortal()
    {

        linkedPortal = linkedPortals[Random.Range(0, linkedPortals.Length)];
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Player" && other.gameObject.GetComponent<PhotonView>().isMine == true && other.gameObject.GetComponent<teleportFlag>().canTeleport == false && justTeleported == false)
        {
            other.gameObject.GetComponent<teleportFlag>().canTeleport = true;
            linkedPortal.GetComponent<TeleporterPortal>().justTeleported = false;
        }
    }
    void engageRPC()
    {
        this.GetComponent<PhotonView>().RPC("fx", PhotonTargets.All, pos, rot);
    }
    [PunRPC]
    void fx(Vector3 pos, Quaternion rot)
    {
        Instantiate(teleFX, pos, rot);
    }
}
