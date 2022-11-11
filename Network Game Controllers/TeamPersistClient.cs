using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class TeamPersistClient : MonoBehaviour {

    public bool firstRound = true;
    public string[] whiteLevelNames; // we recieve this from the master
    public int lastTeam;
    public bool IsMasterClient = false;
    public string EngineeringOwner = "Red";
    public string WasteDisposalOwner = "Red";
    public string CargoBayOwner = "Red";
    public string ComputerCoreOwner = "Yellow";
    public string VehicleBayOwner = "Yellow";
    public string BridgeOwner = "Yellow";
    SelfNetworkManagerII snm;
	// Use this for initialization

 //  public void GetTeam()
 //   { // this is called from TeamMember script
 //       snm = GameObject.FindObjectOfType<SelfNetworkManagerII>();
 //       lastTeam = snm.teamIDSaved;
 //   }a
    private void OnLevelWasLoaded(int level)
    {
        bool foundscene = false;
        foreach (string sceneName in whiteLevelNames)
        {
            if (SceneManager.GetActiveScene().name == sceneName)
            {
                foundscene = true;
            }
        }
        if(foundscene == false)
        {
            Destroy(gameObject);
        }
        else
        {
            firstRound = false;
            snm = GameObject.FindObjectOfType<SelfNetworkManagerII>();
            snm.teamIDSaved = lastTeam;
        }

    }

    public void RoundFinished(string mapName) // This is accessed by the capture point script on the master's instance of the game
        
    {

    }

 
}
