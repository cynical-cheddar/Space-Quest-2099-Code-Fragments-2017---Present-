using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CapturePointManager : MonoBehaviour
{
    public int CapPointAmount = 0;
    public List<GameObject> CapturePoints = new List<GameObject>();
    int MyTeamID;
    public bool restartMap;
    string SceneNameGame;
    public bool AttackDefence = false;
    public bool TC = false;
    // Use this for initialization
    void Start()
    {
        SceneNameGame = Application.loadedLevelName;
        Invoke("GetCapturePoints", 0.5f);
    }

    void GetCapturePoints()
    {

        CapturePoint[] points = FindObjectsOfType(typeof(CapturePoint)) as CapturePoint[];
        foreach (CapturePoint point in points)
        {
            CapPointAmount += 1;
            Debug.Log(point.PointNumber);
            CapturePoints.Add(point.gameObject);
            //When a point is captured, set the next one CanCapture = true
        }
    }
    [PunRPC]
    void PointCaptured(int NewTeamID, int CapPointNumber)
    {
        Debug.Log("Point Captured RPC called with: " + NewTeamID + " as team");
        if (AttackDefence == true)
        {


            GameObject pointToDisable;
            GameObject pointToEnable;
            if (CapPointNumber == CapPointAmount - 1) //The game has been won!
            {
                Debug.Log("Congrats, you won!");
                Time.timeScale = 0.1f;
                Time.fixedDeltaTime = 0.1f * 0.02f;
                // Get our player
                TeamMember[] players = FindObjectsOfType<TeamMember>();
                foreach (TeamMember m in players)
                {
                    if (m.gameObject.GetComponent<PhotonView>().isMine == true && m.gameObject.tag == "Player")
                    {
                        MyTeamID = m.gameObject.GetComponent<TeamMember>().teamID;
                    }
                }
                if (NewTeamID == MyTeamID)
                {
                    GameObject.Find("_UserHUD").transform.Find("VICTORY").gameObject.SetActive(true);
                }
                if (NewTeamID != MyTeamID)
                {
                    GameObject.Find("_UserHUD").transform.Find("DEFEAT").gameObject.SetActive(true);
                }

                Invoke("RealTime", 1f);

            }
            else
            {
                foreach (GameObject point in CapturePoints)
                {
                    if (point.GetComponent<CapturePoint>().PointNumber == CapPointNumber)
                    {
                        pointToDisable = point;
                        pointToDisable.GetComponent<CapturePoint>().CanCapture = false;
                    }
                }
                foreach (GameObject point in CapturePoints)
                {
                    if (point.GetComponent<CapturePoint>().PointNumber == CapPointNumber + 1)
                    {
                        pointToEnable = point;
                        pointToEnable.GetComponent<CapturePoint>().CanCapture = true;
                    }
                }
            }

        }

        if (TC == true)
        {
            //    GameObject pointToDisable;
            //    GameObject pointToEnable;
            // Loop through all cap points, if they are all one team, we have won
            bool won = true;
            CapturePoint[] points = FindObjectsOfType(typeof(CapturePoint)) as CapturePoint[];
            foreach (CapturePoint point in points)
            {
                if (point.teamID != NewTeamID)
                {
                    won = false;
                    Debug.Log("There hasn't been a victory");
                }
            }




            if (won == true && PhotonNetwork.isMasterClient == true) //The game has been won!
            {
                GetComponent<PhotonView>().RPC("TimeEffects", PhotonTargets.All);
                GetComponent<PhotonView>().RPC("VictoryDefeatText", PhotonTargets.All, NewTeamID);






                // Now we call a function on everyone's instance of the game to update the territories. However, only the mater client makes the decision on what two areas will be in play next.

                TeamPersistClient tpc = GameObject.FindObjectOfType<TeamPersistClient>().GetComponent<TeamPersistClient>();
                if (PhotonNetwork.isMasterClient == true)
                {

                    updateTerritories(NewTeamID, SceneNameGame); //updates territories on master client
                    this.GetComponent<PhotonView>().RPC("updateOthersOnAreaOwnership", PhotonTargets.OthersBuffered, tpc.CargoBayOwner, tpc.ComputerCoreOwner, tpc.BridgeOwner, tpc.EngineeringOwner, tpc.WasteDisposalOwner, tpc.VehicleBayOwner); // passes territory ownership info to other players

                }

                // TC information is stored in the teampersistclient object.

                // Here, we should check if ALL of the territories belong to a team.






            }
        }
    }
    [PunRPC]
    void TimeEffects()
    {
        Time.timeScale = 0.1f;
        Time.fixedDeltaTime = 0.1f * 0.02f;
        Invoke("RealTime", 1f); //This function slows down time for a nice effect. It also handles the process of loading th next level.
    }
    [PunRPC]
    void VictoryDefeatText(int NewTeamID1)
    {
        // Get our player
        TeamMember[] players = FindObjectsOfType<TeamMember>();
        foreach (TeamMember m in players)
        {
            if (m.gameObject.GetComponent<PhotonView>().isMine == true && m.gameObject.tag == "Player" && m.gameObject.GetComponent<TeamMember>().isPlayer == true)
            {
                MyTeamID = m.gameObject.GetComponent<TeamMember>().teamID;
            }
        }
        Debug.Log(MyTeamID + " is supposedly my teamID");
        if (NewTeamID1 == MyTeamID)
        {
            GameObject.Find("_UserHUD").transform.Find("VICTORY").gameObject.SetActive(true);
        }
        if (NewTeamID1 != MyTeamID)
        {
            GameObject.Find("_UserHUD").transform.Find("DEFEAT").gameObject.SetActive(true);
        }
    }
    [PunRPC]
    void updateOthersOnAreaOwnership(string cBay, string cCore, string bridge, string eng, string wDisposal, string vBay)
    {
        GameObject.FindObjectOfType<TeamPersistClient>().GetComponent<TeamPersistClient>().CargoBayOwner = "Red";
        GameObject.FindObjectOfType<TeamPersistClient>().GetComponent<TeamPersistClient>().ComputerCoreOwner = "Red";
        GameObject.FindObjectOfType<TeamPersistClient>().GetComponent<TeamPersistClient>().BridgeOwner = "Red";
        GameObject.FindObjectOfType<TeamPersistClient>().GetComponent<TeamPersistClient>().EngineeringOwner = "Red";
        GameObject.FindObjectOfType<TeamPersistClient>().GetComponent<TeamPersistClient>().WasteDisposalOwner = "Red";
        GameObject.FindObjectOfType<TeamPersistClient>().GetComponent<TeamPersistClient>().VehicleBayOwner = "Red";

    }

    void updateTerritories(int NewTeamID, string sceneName)
    {
        if (sceneName == "REDWasteDisposalVsYELLOWCargoBay")
        {
            if (NewTeamID == 1)
            {
                GameObject.FindObjectOfType<TeamPersistClient>().GetComponent<TeamPersistClient>().CargoBayOwner = "Red";
            }
            if (NewTeamID == 3)
            {
                GameObject.FindObjectOfType<TeamPersistClient>().GetComponent<TeamPersistClient>().WasteDisposalOwner = "Yellow";
            }
        }
        if (sceneName == "REDWasteDisposalVsYELLOWComputerCore")
        {
            if (NewTeamID == 1)
            {
                GameObject.FindObjectOfType<TeamPersistClient>().GetComponent<TeamPersistClient>().ComputerCoreOwner = "Red";
            }
            if (NewTeamID == 3)
            {
                GameObject.FindObjectOfType<TeamPersistClient>().GetComponent<TeamPersistClient>().WasteDisposalOwner = "Yellow";
            }
        }
        if (sceneName == "REDWasteDisposalVsYELLOWVehicleBay")
        {
            if (NewTeamID == 1)
            {
                GameObject.FindObjectOfType<TeamPersistClient>().GetComponent<TeamPersistClient>().VehicleBayOwner = "Red";
            }
            if (NewTeamID == 3)
            {
                GameObject.FindObjectOfType<TeamPersistClient>().GetComponent<TeamPersistClient>().WasteDisposalOwner = "Yellow";
            }
        }
        if (sceneName == "REDCargoBayVsYELLOWComputerCore")
        {
            if (NewTeamID == 1)
            {
                GameObject.FindObjectOfType<TeamPersistClient>().GetComponent<TeamPersistClient>().ComputerCoreOwner = "Red";
            }
            if (NewTeamID == 3)
            {
                GameObject.FindObjectOfType<TeamPersistClient>().GetComponent<TeamPersistClient>().CargoBayOwner = "Yellow";
            }
        }
        if (sceneName == "REDCargoBayVsYELLOWVehicleBay")
        {
            if (NewTeamID == 1)
            {
                GameObject.FindObjectOfType<TeamPersistClient>().GetComponent<TeamPersistClient>().VehicleBayOwner = "Red";
            }
            if (NewTeamID == 3)
            {
                GameObject.FindObjectOfType<TeamPersistClient>().GetComponent<TeamPersistClient>().CargoBayOwner = "Yellow";
            }
        }
        if (sceneName == "REDComputerCoreVsYELLOWVehicleBay")
        {
            if (NewTeamID == 1)
            {
                GameObject.FindObjectOfType<TeamPersistClient>().GetComponent<TeamPersistClient>().VehicleBayOwner = "Red";
            }
            if (NewTeamID == 3)
            {
                GameObject.FindObjectOfType<TeamPersistClient>().GetComponent<TeamPersistClient>().ComputerCoreOwner = "Yellow";
            }
        }
        if (sceneName == "REDEngineeringDEFENCE")
        {
            if (NewTeamID == 1)
            {
                GameObject.FindObjectOfType<TeamPersistClient>().GetComponent<TeamPersistClient>().WasteDisposalOwner = "Red";
            }
            if (NewTeamID == 3)
            {
                GameObject.FindObjectOfType<TeamPersistClient>().GetComponent<TeamPersistClient>().EngineeringOwner = "Yellow";
            }
        }
        if (sceneName == "YELLOWBridgeDEFENCE")
        {
            if (NewTeamID == 1)
            {
                GameObject.FindObjectOfType<TeamPersistClient>().GetComponent<TeamPersistClient>().BridgeOwner = "Red";
            }
            if (NewTeamID == 3)
            {
                GameObject.FindObjectOfType<TeamPersistClient>().GetComponent<TeamPersistClient>().ComputerCoreOwner = "Yellow";
            }
        }
        if (sceneName == "YELLOWCargoBayVsREDComputerCore")
        {
            if (NewTeamID == 1)
            {
                GameObject.FindObjectOfType<TeamPersistClient>().GetComponent<TeamPersistClient>().CargoBayOwner = "Red";
            }
            if (NewTeamID == 3)
            {
                GameObject.FindObjectOfType<TeamPersistClient>().GetComponent<TeamPersistClient>().ComputerCoreOwner = "Yellow";
            }
        }
        if (sceneName == "YELLOWCargoBayVsREDVehicleBay")
        {
            if (NewTeamID == 1)
            {
                GameObject.FindObjectOfType<TeamPersistClient>().GetComponent<TeamPersistClient>().CargoBayOwner = "Red";
            }
            if (NewTeamID == 3)
            {
                GameObject.FindObjectOfType<TeamPersistClient>().GetComponent<TeamPersistClient>().VehicleBayOwner = "Yellow";
            }
        }
        if (sceneName == "YELLOWComputerCoreVsREDVehicleBay")
        {
            if (NewTeamID == 1)
            {
                GameObject.FindObjectOfType<TeamPersistClient>().GetComponent<TeamPersistClient>().ComputerCoreOwner = "Red";
            }
            if (NewTeamID == 3)
            {
                GameObject.FindObjectOfType<TeamPersistClient>().GetComponent<TeamPersistClient>().VehicleBayOwner = "Yellow";
            }
        }
        if (sceneName == "YELLOWWasteDisposalVsREDCargoBay")
        {
            if (NewTeamID == 1)
            {
                GameObject.FindObjectOfType<TeamPersistClient>().GetComponent<TeamPersistClient>().WasteDisposalOwner = "Red";
            }
            if (NewTeamID == 3)
            {
                GameObject.FindObjectOfType<TeamPersistClient>().GetComponent<TeamPersistClient>().CargoBayOwner = "Yellow";
            }
        }
        if (sceneName == "YELLOWWasteDisposalVsREDComputerCore")
        {
            if (NewTeamID == 1)
            {
                GameObject.FindObjectOfType<TeamPersistClient>().GetComponent<TeamPersistClient>().WasteDisposalOwner = "Red";
            }
            if (NewTeamID == 3)
            {
                GameObject.FindObjectOfType<TeamPersistClient>().GetComponent<TeamPersistClient>().ComputerCoreOwner = "Yellow";
            }
        }
        if (sceneName == "YELLOWWasteDisposalVsREDVehicleBay")
        {
            if (NewTeamID == 1)
            {
                GameObject.FindObjectOfType<TeamPersistClient>().GetComponent<TeamPersistClient>().WasteDisposalOwner = "Red";
            }
            if (NewTeamID == 3)
            {
                GameObject.FindObjectOfType<TeamPersistClient>().GetComponent<TeamPersistClient>().VehicleBayOwner = "Yellow";
            }
        }

    }
    void RealTime()
    {
        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;

        if (restartMap == true && PhotonNetwork.isMasterClient == true)
        {
            PhotonNetwork.LoadLevel(SceneNameGame); //LOADS THE CURRENT MAP
        }
        if (TC == true && PhotonNetwork.isMasterClient == true)
        {
            bool FoundStage1 = false;
            // the master client should figure out which level to load.
            TeamPersistClient tpc = GameObject.FindObjectOfType<TeamPersistClient>().GetComponent<TeamPersistClient>();
            // First we check if the game has been won:

            if (tpc.CargoBayOwner == "Red" && tpc.ComputerCoreOwner == "Red" && tpc.BridgeOwner == "Red" && tpc.EngineeringOwner == "Red" && tpc.WasteDisposalOwner == "Red" && tpc.VehicleBayOwner == "Red")
            {
                //Red won!
                PhotonNetwork.LoadLevel("TCVictoryRed");
                FoundStage1 = true;
            }
            if (tpc.CargoBayOwner == "Yellow" && tpc.ComputerCoreOwner == "Yellow" && tpc.BridgeOwner == "Yellow" && tpc.EngineeringOwner == "Yellow" && tpc.WasteDisposalOwner == "Yellow" && tpc.VehicleBayOwner == "Yellow")
            {
                //Yellow won!
                PhotonNetwork.LoadLevel("TCVictoryYellow");
                FoundStage1 = true;
            }


            // Secondly, we check if the next area is just going to be attack/defend.
            else if (tpc.CargoBayOwner == "Red" && tpc.ComputerCoreOwner == "Red" && tpc.WasteDisposalOwner == "Red" && tpc.VehicleBayOwner == "Red" && tpc.EngineeringOwner == "Red" && FoundStage1 == false)
            {
                // Load bridge assault
                PhotonNetwork.LoadLevel("YELLOWBridgeDEFENCE"); //LOADS THE MAP
            }
            else if (tpc.CargoBayOwner == "Yellow" && tpc.ComputerCoreOwner == "Yellow" && tpc.WasteDisposalOwner == "Yellow" && tpc.VehicleBayOwner == "Yellow" && tpc.BridgeOwner == "Yellow" && FoundStage1 == false)
            {
                // Load engineering assault
                PhotonNetwork.LoadLevel("REDEngineeringDEFENCE"); //LOADS THE MAP

            }
            else if (FoundStage1 == false)
            {


                LoadNewScene();



            }
        }
    }

    void LoadNewScene()
    {
        TeamPersistClient tpc = GameObject.FindObjectOfType<TeamPersistClient>().GetComponent<TeamPersistClient>();
        // A bit more complicated, we need to decide which two areas can fight.
        // To do this, we can choose a random territory from the central four and then select a random adjacent territory from the opposing team.
        string[] centreMaps = new string[] { "CargoBay", "ComputerCore", "WasteDisposal", "VehicleBay" };
        int maxComparisions = 5;
        int comparisions = 0;
        string Comparision1 = centreMaps[Random.Range(0, centreMaps.Length)];
        bool found = false;
        // We can randomise the second comparision later
        if (Comparision1 == "CargoBay")
        {
            //   string CargoBayOwner = tpc.CargoBayOwner;
            // Get an adjacent area of opposing team.
            for (int i = 0; i < 5; i++)
            {
                if (found == false)
                {
                    int randomComparision = Random.Range(0, 2);
                    comparisions += 1;

                    if (randomComparision == 0)
                    {
                        if (tpc.ComputerCoreOwner != tpc.CargoBayOwner)
                        {
                            if (tpc.CargoBayOwner == "Red")
                            {
                                PhotonNetwork.LoadLevel("REDCargoBayVsYELLOWComputerCore");
                                found = true;
                            }
                            else
                            {
                                PhotonNetwork.LoadLevel("YELLOWCargoBayVsREDComputerCore");
                                found = true;
                            }
                        }
                    }
                    if (randomComparision == 1)
                    {
                        if (tpc.WasteDisposalOwner != tpc.CargoBayOwner)
                        {
                            if (tpc.CargoBayOwner == "Red")
                            {
                                PhotonNetwork.LoadLevel("YELLOWWasteDisposalVsREDCargoBay");
                                found = true;
                            }
                            else
                            {
                                PhotonNetwork.LoadLevel("REDWasteDisposalVsYELLOWCargoBay");
                                found = true;
                            }
                        }
                    }
                    if (randomComparision == 2)
                    {
                        if (tpc.VehicleBayOwner != tpc.CargoBayOwner)
                        {
                            if (tpc.CargoBayOwner == "Red")
                            {
                                PhotonNetwork.LoadLevel("REDCargoBayVsYELLOWVehicleBay");
                                found = true;
                            }
                            else
                            {
                                PhotonNetwork.LoadLevel("YELLOWCargoBayVsREDVehicleBay");
                                found = true;
                            }
                        }
                    }
                }
            }
        }


        if (Comparision1 == "ComputerCore")
        {
            //   string CargoBayOwner = tpc.CargoBayOwner;
            // Get an adjacent area of opposing team.
            for (int i = 0; i < 5; i++)
            {
                if (found == false)
                {
                    int randomComparision = Random.Range(0, 2);
                    comparisions += 1;
                    if (randomComparision == 0)
                    {
                        if (tpc.CargoBayOwner != tpc.ComputerCoreOwner)
                        {
                            if (tpc.ComputerCoreOwner == "Red")
                            {
                                PhotonNetwork.LoadLevel("YELLOWCargoBayVsREDComputerCore");
                                found = true;
                            }
                            else
                            {
                                PhotonNetwork.LoadLevel("REDCargoBayVsYELLOWComputerCore");
                                found = true;
                            }
                        }
                    }
                    if (randomComparision == 1)
                    {
                        if (tpc.WasteDisposalOwner != tpc.ComputerCoreOwner)
                        {
                            if (tpc.ComputerCoreOwner == "Red")
                            {

                                PhotonNetwork.LoadLevel("YELLOWWasteDisposalVsREDComputerCore");
                                found = true;
                            }
                            else
                            {

                                PhotonNetwork.LoadLevel("REDWasteDisposalVsYELLOWComputerCore");
                                found = true;
                            }
                        }
                    }
                    if (randomComparision == 2)
                    {

                        if (tpc.VehicleBayOwner != tpc.ComputerCoreOwner)
                        {
                            if (tpc.ComputerCoreOwner == "Red")
                            {

                                PhotonNetwork.LoadLevel("REDComputerCoreVsYELLOWVehicleBay");
                                found = true;
                            }
                            else
                            {

                                PhotonNetwork.LoadLevel("YELLOWComputerCoreVsREDVehicleBay");
                                found = true;
                            }
                        }
                    }
                }
            }
        }


        if (Comparision1 == "WasteDisposal")
        {
            //   string CargoBayOwner = tpc.CargoBayOwner;
            // Get an adjacent area of opposing team.
            for (int i = 0; i < 5; i++)
            {
                if (found == false)
                {
                    int randomComparision = Random.Range(0, 2);
                    comparisions += 1;
                    if (randomComparision == 0)
                    {
                        if (tpc.CargoBayOwner != tpc.WasteDisposalOwner)
                        {
                            if (tpc.WasteDisposalOwner == "Red")
                            {

                                PhotonNetwork.LoadLevel("REDWasteDisposalVsYELLOWCargoBay");
                                found = true;
                            }
                            else
                            {

                                PhotonNetwork.LoadLevel("YELLOWWasteDisposalVsREDCargoBay");
                                found = true;
                            }
                        }
                    }
                    if (randomComparision == 1)
                    {
                        if (tpc.ComputerCoreOwner != tpc.WasteDisposalOwner)
                        {
                            if (tpc.WasteDisposalOwner == "Red")
                            {

                                PhotonNetwork.LoadLevel("REDWasteDisposalVsYELLOWComputerCore");
                                found = true;
                            }
                            else
                            {

                                PhotonNetwork.LoadLevel("YELLOWWasteDisposalVsREDComputerCore");
                                found = true;
                            }
                        }
                    }
                    if (randomComparision == 2)
                    {
                        if (tpc.VehicleBayOwner != tpc.WasteDisposalOwner)
                        {
                            if (tpc.WasteDisposalOwner == "Red")
                            {

                                PhotonNetwork.LoadLevel("REDWasteDisposalVsYELLOWVehicleBay");
                                found = true;
                            }
                            else
                            {

                                PhotonNetwork.LoadLevel("YELLOWWasteDisposalVsREDVehicleBay");
                                found = true;
                            }
                        }
                    }
                }
            }
        }

        if (Comparision1 == "VehicleBay")
        {
            //   string CargoBayOwner = tpc.CargoBayOwner;
            // Get an adjacent area of opposing team.
            for (int i = 0; i < 5; i++)
            {
                if (found == false)
                {
                    int randomComparision = Random.Range(0, 2);
                    comparisions += 1;
                    if (randomComparision == 0)
                    {
                        if (tpc.CargoBayOwner != tpc.VehicleBayOwner)
                        {
                            if (tpc.VehicleBayOwner == "Red")
                            {

                                PhotonNetwork.LoadLevel("YELLOWCargoBayVsREDVehicleBay");
                                found = true;
                            }
                            else
                            {

                                PhotonNetwork.LoadLevel("REDCargoBayVsYELLOWVehicleBay");
                                found = true;
                            }
                        }
                    }
                    if (randomComparision == 1)
                    {
                        if (tpc.ComputerCoreOwner != tpc.VehicleBayOwner)
                        {
                            if (tpc.VehicleBayOwner == "Red")
                            {

                                PhotonNetwork.LoadLevel("YELLOWComputerCoreVsREDVehicleBay");
                                found = true;
                            }
                            else
                            {

                                PhotonNetwork.LoadLevel("REDComputerCoreVsYELLOWVehicleBay");
                                found = true;
                            }
                        }
                    }
                    if (randomComparision == 2)
                    {
                        if (tpc.WasteDisposalOwner != tpc.VehicleBayOwner)
                        {
                            if (tpc.VehicleBayOwner == "Red")
                            {

                                PhotonNetwork.LoadLevel("YELLOWWasteDisposalVsREDVehicleBay");
                                found = true;
                            }
                            else
                            {

                                PhotonNetwork.LoadLevel("REDWasteDisposalVsYELLOWVehicleBay");
                                found = true;
                            }
                        }
                    }
                }
            }
        }





        if(found == false)
        {
            comparisions = 0;
            LoadNewScene();
        }

    }
    
}
