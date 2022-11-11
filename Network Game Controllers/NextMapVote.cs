using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NextMapVote : MonoBehaviour {

    public float votingTime = 10f;
    public int map1Votes = 0;
    public int map2Votes = 0;
    public int map3Votes = 0;
    public SelfNetworkManagerII snm;
    public string map1Name;
    public string map2Name;
    public string map3Name;

    public Sprite map1Image;
    public Sprite map2Image;
    public Sprite map3Image;

    public GameObject map1Icon;
    public GameObject map2Icon;
    public GameObject map3Icon;

    public GameObject map1Text;
    public GameObject map2Text;
    public GameObject map3Text;

    public GameObject map1VotesText;
    public GameObject map2VotesText;
    public GameObject map3VotesText;

    public Sprite[] mapImages;
    public string[] mapNames;
    bool voted = false;
    [PunRPC]
    void AddVoteMap1()
    {
        map1Votes += 1;
        map1VotesText.GetComponent<Text>().text = map1Votes.ToString();
    }
    [PunRPC]
     void AddVoteMap2()
    {
        map2Votes += 1;
        map2VotesText.GetComponent<Text>().text = map2Votes.ToString();
    }
    [PunRPC]
     void AddVoteMap3()
    {
        map3Votes += 1;
        map3VotesText.GetComponent<Text>().text = map3Votes.ToString();
    }
    [PunRPC]
    public void displayGUIInformation(int map1RandomID, int map2RandomID, int map3RandomID)
    {
        if (GameObject.FindObjectOfType<SelfNetworkManagerII>() != null)
        {
            snm = GameObject.FindObjectOfType<SelfNetworkManagerII>();
            snm.playerRespawnTimer = 10000f;
            if (snm.myPlayer != null)
            {
                snm.myPlayer.GetComponent<PhotonView>().RPC("TakeDamage", PhotonTargets.AllBuffered, 1000f, "The Environment", "Normal");
            }
        }
        map1Name = mapNames[map1RandomID];
        map2Name = mapNames[map2RandomID];
        map3Name = mapNames[map3RandomID];

        map1Image = mapImages[map1RandomID];
        map2Image = mapImages[map2RandomID];
        map3Image = mapImages[map3RandomID];

        map1Icon.GetComponent<Image>().sprite = map1Image;
        map2Icon.GetComponent<Image>().sprite = map2Image;
        map3Icon.GetComponent<Image>().sprite = map3Image;

        map1Text.GetComponent<Text>().text = map1Name;
        map2Text.GetComponent<Text>().text = map2Name;
        map3Text.GetComponent<Text>().text = map3Name;

        map1VotesText.GetComponent<Text>().text = "0";
        map2VotesText.GetComponent<Text>().text = "0";
        map3VotesText.GetComponent<Text>().text = "0";
    }
    public void clickVote1()
    {
        if (voted == false)
        {
            voted = true;
            GetComponent<PhotonView>().RPC("AddVoteMap1", PhotonTargets.All);
        }
    }
    public void clickVote2()
    {
        if (voted == false)
        {
            voted = true;
            GetComponent<PhotonView>().RPC("AddVoteMap2", PhotonTargets.All);
        }
    }
    public void clickVote3()
    {
        if (voted == false)
        {
            voted = true;
            GetComponent<PhotonView>().RPC("AddVoteMap3", PhotonTargets.All);
        }
    }
     void Update()
    {
        if (PhotonNetwork.isMasterClient == true)
        {
            if (votingTime > 0)
            {
                votingTime -= Time.deltaTime;
                if (votingTime <= 0)
                {
                    if(map1Votes > map2Votes && map1Votes > map3Votes)
                    {
                        GetComponent<PhotonView>().RPC("loadLevel", PhotonTargets.AllBufferedViaServer, map1Name);
                    }
                    else if (map2Votes > map1Votes && map2Votes > map3Votes)
                    {
                        GetComponent<PhotonView>().RPC("loadLevel", PhotonTargets.AllBufferedViaServer, map2Name);
                    }
                    else if (map3Votes > map1Votes && map3Votes > map1Votes)
                    {
                        GetComponent<PhotonView>().RPC("loadLevel", PhotonTargets.AllBufferedViaServer, map3Name);
                    }
                    else //just load the first one
                    {
                        GetComponent<PhotonView>().RPC("loadLevel", PhotonTargets.AllBufferedViaServer, map1Name);
                    }
                }
            }
        }
    }
    [PunRPC]
    void loadLevel(string name)
    {
        PhotonNetwork.LoadLevelAsync(name);
    }






    void Start()
    {

        if (PhotonNetwork.isMasterClient == true)
        {
            int map1RandomID = Random.Range(0, mapNames.Length);
            int map2RandomID = Random.Range(0, mapNames.Length);
            int map3RandomID = Random.Range(0, mapNames.Length);

            while (map2RandomID == map1RandomID)
            {
                map2RandomID = Random.Range(0, mapNames.Length);
            }

            while (map3RandomID == map1RandomID || map3RandomID == map2RandomID)
            {
                map3RandomID = Random.Range(0, mapNames.Length);
            }

            this.GetComponent<PhotonView>().RPC("displayGUIInformation", PhotonTargets.All, map1RandomID, map2RandomID, map3RandomID);
        }

    }
}
