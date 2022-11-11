using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkDialogueInstantiator : MonoBehaviour
{
    public bool trigger = false;
    bool triggered = false;
    public GameObject dialogueBoxes;
    // Start is called before the first frame update
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.GetComponent<CPMPlayer>() != null && PhotonNetwork.isMasterClient && triggered == false && trigger == true)
        {
            GetComponent<PhotonView>().RPC("startDialogue", PhotonTargets.All);
            triggered = true;
            GetComponent<BoxCollider>().enabled = false;
        }
    }

    [PunRPC]
    void startDialogue()
    {
        // see if someone is already talking
            Instantiate(dialogueBoxes);

    }
}
