using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortalCamera : MonoBehaviour
{

    public Transform playerCamera;
    public Transform portal;
    public Transform otherPortal;
    public Material cameraMaterial;
    // Start is called before the first frame update
    void Start()
    {

        playerCamera = GameObject.FindObjectOfType<SelfNetworkManagerII>().myPlayer.transform.Find("FirstPersonCharacter");
        if(GetComponent<Camera>().targetTexture != null)
        {
            GetComponent<Camera>().targetTexture.Release();
        }
        GetComponent<Camera>().targetTexture = new RenderTexture(Screen.width, Screen.height, 24);
        cameraMaterial.mainTexture = GetComponent<Camera>().targetTexture;
    }

    // Update is called once per frame
    void Update()
    {

        Vector3 playerOffset = playerCamera.position - otherPortal.position;
        transform.position = portal.position + playerOffset;

        float anglularDifference = Quaternion.Angle(portal.rotation, otherPortal.rotation);

        //Quaternion rotationalDifference = Quaternion.AngleAxis(anglularDifference, Vector3.up);
        Vector3 newCameraDirection = playerCamera.forward;
        
        transform.rotation = Quaternion.LookRotation(newCameraDirection, Vector3.up);
    }
}
