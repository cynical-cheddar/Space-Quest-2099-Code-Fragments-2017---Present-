using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosionInstantiator : MonoBehaviour {

    public GameObject impactExplosion;
    [PunRPC]
    void InstantiateExplosion(Vector3 position, Quaternion rotation, float ExplosiveDamage, float ExplosionRadius, float ExplosiveForce, float directDamage, int teamID, string sourcePlayerName,string damageType) // EVERYONE GETS THIS, ALL PLAYERS ARE RESPONSIBLE FOR THEIR OWN PHYSICS.
    {
        //Instantiate explosion physics
        GameObject Explosive = impactExplosion;
        // Now sets the parameters of the explosion:
        //Damage, radius[tick], force[tick]
        Explosion ex = Explosive.GetComponent<Explosion>();
        ex.baseDamage = ExplosiveDamage;
        ex.ExpRadius = ExplosionRadius;
        ex.force = ExplosiveForce;
        ex.projectileTeamID = teamID;
        ex.sourcePlayerName = sourcePlayerName;
        ex.damageType = damageType;
        ex.myID = GetComponent<PhotonView>().ownerId;
        ex.sourcePlayer = gameObject;
        Instantiate(Explosive, position, rotation);
    }
    public void offlineExp(Vector3 position, Quaternion rotation, float ExplosiveDamage, float ExplosionRadius, float ExplosiveForce, float directDamage, int teamID, string sourcePlayerName, string damageType)
    {
        //Instantiate explosion physics
        GameObject Explosive = impactExplosion;
        // Now sets the parameters of the explosion:
        //Damage, radius[tick], force[tick]
        Explosion ex = Explosive.GetComponent<Explosion>();
        ex.baseDamage = ExplosiveDamage;
        ex.ExpRadius = ExplosionRadius;
        ex.force = ExplosiveForce;
        ex.projectileTeamID = teamID;
        ex.sourcePlayerName = sourcePlayerName;
        ex.damageType = damageType;
        ex.myID = GetComponent<PhotonView>().ownerId;
        ex.sourcePlayer = gameObject;
        Instantiate(Explosive, position, rotation);
    }
}
