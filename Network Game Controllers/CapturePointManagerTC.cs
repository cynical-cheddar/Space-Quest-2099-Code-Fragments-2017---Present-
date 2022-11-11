using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CapturePointManagerTC : MonoBehaviour {

    public int CapPointAmount = 0;
    public List<GameObject> CapturePoints = new List<GameObject>();
    int MyTeamID;
    public bool restartMap;
    string SceneNameGame;
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
        GameObject pointToDisable;
        GameObject pointToEnable;
        // Loop through all cap points, if they are all one team, we have won
        bool won = true;
        CapturePoint[] points = FindObjectsOfType(typeof(CapturePoint)) as CapturePoint[];
        foreach (CapturePoint point in points)
        {
            if (point.teamID == NewTeamID)
            {
                
            }
            else
            {
                won = false;
            }
        }




            if (won == true) //The game has been won!
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
    void RealTime()
    {
        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;
        if (restartMap == true)
        {
            PhotonNetwork.LoadLevel(SceneNameGame); //LOADS THE MAP
        }

    }
}
