using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeamPersist : MonoBehaviour
{
    public GameObject teamPersistGameObject; // this thing here doesn't destroy on load
    bool madeForMe = false;
    public string[] whiteLevelNames = new string[] { "REDWasteDisposalVsYELLOWCargoBay", "REDWasteDisposalVsYELLOWComputerCore", "REDWasteDisposalVsYELLOWVehicleBay", "REDCargoBayVsYELLOWComputerCore", "REDCargoBayVsYELLOWVehicleBay", "REDComputerCoreVsYELLOWVehicleBay", "REDEngineeringDEFENCE", "YELLOWBridgeDEFENCE", "YELLOWCargoBayVsREDComputerCore", "YELLOWCargoBayVsREDVehicleBay", "YELLOWComputerCoreVsREDVehicleBay", "YELLOWWasteDisposalVsREDCargoBay", "YELLOWWasteDisposalVsREDComputerCore", "YELLOWWasteDisposalVsREDVehicleBay" };
    // Use this for initialization
    void Start()
    {
        if (madeForMe == false)
        {
            madeForMe = true;
            // Create the teampersist gameObject for me
            Invoke("checkIfTeamPersistExists", 0.1f);

        }
    }
    void checkIfTeamPersistExists()
    {
        if (GameObject.FindGameObjectWithTag("TeamPersist") == null) // If the team persist client object doesn't exist:
        {
            GameObject teamPersistGameObjectInstance = Instantiate(teamPersistGameObject);
            teamPersistGameObjectInstance.GetComponent<TeamPersistClient>().whiteLevelNames = whiteLevelNames;
            if(PhotonNetwork.isMasterClient == true)
            {
                teamPersistGameObjectInstance.GetComponent<TeamPersistClient>().IsMasterClient = true; // We make sure the client team persist object knows that it belongs to the master. This is used for the TC game mode.
            }
        }
    }

    void OnPhotonPlayerConnected(PhotonPlayer newPlayer)
    {
        if(PhotonNetwork.isMasterClient == true)
        {
            // Give the target player the teampersist gameObject
            string strUserID = newPlayer.UserId;
         //   GetComponent<PhotonView>().RPC("GiveTeamPersistGameObject", PhotonTargets.Others, strUserID, whiteLevelNames);
        }
    }
    [PunRPC]
    void GiveTeamPersistGameObject(string strUserID, string[] whiteLevelNames)
    {
        if(PhotonNetwork.player.UserId == strUserID)
        {
            GameObject teamPersistGameObjectInstance = Instantiate(teamPersistGameObject);
            teamPersistGameObjectInstance.GetComponent<TeamPersistClient>().whiteLevelNames = whiteLevelNames;
        }

    }
}
