using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Explosion : MonoBehaviour
{

    public float ExpRadius = 5f;
    public float ForceRadius;
    public float force = 5f;
    public float baseDamage = 20f;
    Vector3 tossDirection;
    public string damageType = "Explosive";
    public string sourcePlayerName;
    public GameObject sourcePlayer;
    public int projectileTeamID = 0;
    public int myID;

    // Use this for initialization
    void Start()
    {
        ForceRadius = ExpRadius * 0.9f;
        Vector3 explosionPos = transform.position;
        Collider[] colliders = Physics.OverlapSphere(explosionPos, ForceRadius);
        if(PhotonNetwork.offlineMode == false) {
            foreach (Collider hit in colliders)
            {
                Rigidbody rb = hit.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    if (rb.gameObject.tag != "Grenade")
                    {
                        CharacterController cc = hit.GetComponent<CharacterController>();

                        if (cc != null && cc.gameObject.GetComponent<PhotonView>().isMine == true)
                        {
                            cc.enabled = false;
                            // Call a funtion on the player movement script in which the cc does not enable for a short time. Also activates a collider trigger that checks below the playervoid to detect the moment before the player lands.
                            cc.gameObject.GetComponent<PlayerMovement>().RocketJumping();

                            if (rb != null)
                            {
                                rb.isKinematic = false;
                                rb.AddExplosionForce(force, explosionPos, ForceRadius, 3f);
                                //Explosion Damage
                                Vector3 distance = cc.transform.position - transform.position;
                                float damage = (((ExpRadius - distance.magnitude) / ExpRadius) * baseDamage);
                                Transform hitTransform = cc.transform;
                                Health h = cc.gameObject.GetComponent<Health>();

                                while (h == null && hitTransform.parent) // Searches for bit with health script
                                {
                                    hitTransform = hitTransform.parent;
                                    h = hitTransform.GetComponent<Health>();
                                }
                                if (h != null) // if it finds the health script
                                {
                                    TeamMember tm = cc.GetComponent<TeamMember>();  //gets the other team id
                                    if (tm.teamID == 0 || tm.teamID != projectileTeamID)
                                    {

                                        h.GetComponent<PhotonView>().RPC("TakeDamage", PhotonTargets.All, damage, sourcePlayerName, damageType);         //RPC
                                        if (sourcePlayer != null)
                                        {
                                            sourcePlayer.GetComponent<HitSound>().PlaySound(damage);
                                        }
                                    }

                                }
                            }
                        }
                        else if(hit.GetComponent<AiBehaviour>() != null)
                        {
                            if (rb != null)
                            {
                                rb.isKinematic = false;
                                rb.AddExplosionForce(force, explosionPos, ForceRadius, 3f);
                                //Explosion Damage
                                Vector3 distance = hit.transform.position - transform.position;
                                float damage = (((ExpRadius - distance.magnitude) / ExpRadius) * baseDamage);
                                Transform hitTransform = hit.transform;
                                Health h = hit.gameObject.GetComponent<Health>();

                                while (h == null && hitTransform.parent) // Searches for bit with health script
                                {
                                    hitTransform = hitTransform.parent;
                                    h = hitTransform.GetComponent<Health>();
                                }
                                if (h != null) // if it finds the health script
                                {
                                    TeamMember tm = hit.GetComponent<TeamMember>();  //gets the other team id
                                    if (tm.teamID == 0 || tm.teamID != projectileTeamID)
                                    {

                                        h.GetComponent<PhotonView>().RPC("TakeDamage", PhotonTargets.All, damage, sourcePlayerName, damageType);         //RPC
                                        sourcePlayer.GetComponent<HitSound>().PlaySound(damage);
                                    }

                                }
                            }
                        }
                        else if (cc == null)
                        {
                            rb.AddExplosionForce(force, explosionPos, ExpRadius, 3f);
                        }
                    }
                }



                else if (rb != null)
                {
                    rb.AddExplosionForce(force, explosionPos, ExpRadius, 3f);
                }
            }
        }
        else
        {
            foreach (Collider hit in colliders)
            {
                
                CharacterController cc = hit.GetComponent<CharacterController>();
                if (cc != null)
                {
                    Vector3 distance = cc.transform.position - transform.position;
                    float damage = (((ExpRadius - distance.magnitude) / ExpRadius) * baseDamage);
                    Transform hitTransform = cc.transform;
                    Health h = cc.gameObject.GetComponent<Health>();
                    h.GetComponent<PhotonView>().RPC("TakeDamage", PhotonTargets.All, damage, sourcePlayerName, damageType);         //RPC
                }
            }
        }

    }

}
