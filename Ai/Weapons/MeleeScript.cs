using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeScript : MonoBehaviour
{


    public GameObject sourcePlayer;
    int projectileTeamID = 0;
    public float damage = 20f;
    public AudioClip swingSound;
    public AudioClip swingHitSound;
    public AudioSource auxAudio;
    public GameObject impactEffect;
    public List<GameObject> hitThings;


    [PunRPC]
    public void swing()
    {

        GetComponent<AudioSource>().clip = swingSound;
        GetComponent<AudioSource>().Play();
        Invoke("startSwing", 0.3f);
        Invoke("endswing", 2.3f);
    }
    void startSwing()
    {
        hitThings.Clear();
        GetComponent<Collider>().enabled = true;
    }
    void endswing()
    {
        GetComponent<Collider>().enabled = false;
    }

    void OnTriggerEnter(Collider hit)
    {
        if (hit.gameObject.GetComponent<ProjectileScript>() != null)
        {
            Physics.IgnoreCollision(hit, GetComponent<Collider>());
        }
        else
        {
            if (hit.gameObject.tag == "Player" && !hitThings.Contains(hit.gameObject))
            {
                hitThings.Add(hit.gameObject);
                collided(hit);
            }

        }
    }
    [PunRPC]
    void InstantiateImpactEffect()
    {
        Instantiate(impactEffect, transform.position, transform.rotation);
    }

    [PunRPC]
    void hitPlaySound()
    {
        auxAudio.clip = swingHitSound;
        auxAudio.Play();
    }

    void collided(Collider hit)
    {
        if (PhotonNetwork.isMasterClient)
        {


            Transform hitTransform = hit.transform;
            if (hitTransform != sourcePlayer.transform)
            {
                Health h = hit.gameObject.GetComponent<Health>();

                /* while (h == null && hitTransform.parent)
                 {
                     hitTransform = hitTransform.parent;
                     h = hitTransform.GetComponent<Health>();
                 }*/
                if (h != null)
                {
                    TeamMember tm = hitTransform.gameObject.GetComponent<TeamMember>();
                    if (tm.teamID == 0 || projectileTeamID == 0 || tm.teamID != projectileTeamID)
                    {

                        h.GetComponent<PhotonView>().RPC("TakeDamage", PhotonTargets.All, damage, "Lizard", "Normal");         //RPC
                        GetComponent<PhotonView>().RPC("InstantiateImpactEffect", PhotonTargets.All);
                        GetComponent<PhotonView>().RPC("hitPlaySound", PhotonTargets.All);

                        sourcePlayer.GetComponent<HitSound>().PlaySound(damage);
                    }

                }
            }
        }

        //projectileParticle.Stop();

    }
}
