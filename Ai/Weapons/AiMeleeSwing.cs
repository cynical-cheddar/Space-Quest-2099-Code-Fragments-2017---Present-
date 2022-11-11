using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AiMeleeSwing : AiWeapon
{
    public GameObject meleeHead;
    
    public override void fire(Vector3 target, Vector3 dir)
    {
        GetComponent<PhotonView>().RPC("swing", PhotonTargets.All);
        
    }

    [PunRPC]
    void swing()
    {
        meleeHead.GetComponent<MeleeScript>().swing();
    }
}
