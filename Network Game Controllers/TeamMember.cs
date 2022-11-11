using UnityEngine;
using System.Collections;

public class TeamMember : MonoBehaviour {
    public bool isPlayer = true;
    public int _teamID = 0;
    public int teamID
    {
        get { return _teamID; }
    }
    public Material[] CharacterMaterials;
    [PunRPC]
    void SetTeamID(int id)
    {
        _teamID = id;
        Transform top = this.transform.Find("PlayerModel/Tops");
        if (top != null)
        {
            SkinnedMeshRenderer mySkin = top.GetComponent<SkinnedMeshRenderer>();
            if (mySkin == null)
            {
                Debug.Log("BLOODY HELL! YOU HAVE NO SKIN!");
            }
            else
            {
                mySkin.material = CharacterMaterials[id];
            }
        }


    }

    public void Start()
    {
        // Find termPersist
     //   GameObject.FindWithTag("TeamPersist").GetComponent<TeamPersistClient>().GetTeam();
    }
}
