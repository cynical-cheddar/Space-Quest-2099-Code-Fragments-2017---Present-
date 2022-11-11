using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class FreeForAllManager : MonoBehaviour {

    public int fragLimit = 25;
    public float respawnTime = 5f;
    //public string winnerName = "Rob";
    public string winnerPhrase = "is the victor!";
    public string mapVoteName = "";
    SelfNetworkManagerII snm;
   public GameObject VictoryUI;
    GameObject VictoryUIInstance;
    // Use this for initialization
    void Start() {

    }

    // Update is called once per frame
    [PunRPC]
    public void victory(string winnerName)
    {
        snm = GameObject.FindObjectOfType<SelfNetworkManagerII>();
        snm.playerRespawnTimer = 1000f;
        Debug.Log("Congrats, you won!");
        Time.timeScale = 0.1f;
        Time.fixedDeltaTime = 0.1f * 0.02f;
        VictoryUIInstance = Instantiate(VictoryUI);
        VictoryUIInstance.GetComponent<Canvas>().enabled = true;
        VictoryUIInstance.transform.Find("VICTORY").gameObject.GetComponent<Text>().text = (winnerName + " " + winnerPhrase);
        VictoryUIInstance.transform.Find("VICTORY").gameObject.SetActive(true);
        // Invoke("ensureCanvasOn", 0.1f);
        Invoke("destroyVictoryUI", 0.95f);
        Invoke("RealTime", 1f);
    }
    void destroyVictoryUI()
    {
        Destroy(VictoryUIInstance);
    }
    void RealTime()
    {
        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;
        loadNewLevel();
    }
    void loadNewLevel()
    {
        if (PhotonNetwork.isMasterClient == true)
        {
            if (GameObject.FindWithTag("MapVote") == null)
            {
                PhotonNetwork.InstantiateSceneObject(mapVoteName, transform.position, transform.rotation, 0, null);
            }
        }
    }
}
