using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortalTeleport : MonoBehaviour
{

    public Transform player;
    public Transform reciever;

    bool inPortal = false;

    public bool flip = false;
    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindObjectOfType<SelfNetworkManagerII>().myPlayer.transform;
    }

    // Update is called once per frame
    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.GetComponent<CPMPlayer>() != null)
        {
            inPortal = true;
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.GetComponent<CPMPlayer>() != null)
        {
            inPortal = false;
        }
    }


    private void Update()
    {
        if (inPortal)
        {
            float dotProduct = Vector3.Dot(transform.up, (player.position - transform.position));
            if(dotProduct < 0f)
            {
                // correct position, enter through portal
                float rotationDiff = -Quaternion.Angle(transform.rotation, reciever.rotation);
                if (flip) rotationDiff += 180;
                player.Rotate(Vector3.up, rotationDiff);
                Vector3 positionOffset = Quaternion.Euler(0f, rotationDiff, 0f) * (player.position - transform.position);
                player.position = reciever.position + positionOffset;
                inPortal = false;
            }
        }
    }
}
