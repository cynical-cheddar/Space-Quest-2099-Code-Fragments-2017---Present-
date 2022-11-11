using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Health : MonoBehaviour {

	public float hitPoints = 100f;
	public float currentHitPoints;
    public float respawnTime = 1f;
    SelfNetworkManagerII snm;
    public string lastHitPlayer;
    public string lastHitType;
    public bool IsDying = false;
    public GameObject killedText;
    public GameObject fragText;
    public GameObject ragdollNormal;
    public GameObject ragdollGib;
    public GameObject ragdollVaporise;
    public GameObject ragdollIncapacitated;
    public Camera incapacitatedCamera;
    public AudioClip[] painSounds;
    GameObject incapacitatedRagdoll;
    // Use this for initialization
    void Start () {
        if (GetComponent<PhotonView>().isMine)
        {
            currentHitPoints = hitPoints;
            snm = GameObject.FindObjectOfType<SelfNetworkManagerII>();
            snm.UIHealth.text = currentHitPoints.ToString();
        }

    }
    public void setStuff()
    {
        currentHitPoints = hitPoints;
        snm = GameObject.FindObjectOfType<SelfNetworkManagerII>();
        snm.UIHealth.text = currentHitPoints.ToString();
    }

    [PunRPC]
    public void TakeDamage(float amt, string LHP, string LHT)
    {
        lastHitPlayer = LHP;
        lastHitType = LHT;
        amt = Mathf.Round(amt);
        currentHitPoints -= amt;

		if (GetComponent<PhotonView>().isMine && snm.UIHealth.text != null)
        {
            snm.UIHealth.text = currentHitPoints.ToString();
            if(GetComponent<CPMPlayer>() != null){
                GetComponent<CPMPlayer>().shakeCamera(0.1f, 0.5f + amt/hitPoints);
            }
            if(GetComponent<AudioSource>() != null){
                if(GetComponent<AudioSource>().isPlaying == false && painSounds.Length != 0){
                    GetComponent<AudioSource>().clip = painSounds[Random.Range(0, painSounds.Length - 1)];
                    GetComponent<AudioSource>().Play();
                }
            }
        }
        if (currentHitPoints <= 0 && !IsDying)
        {
            Die(LHT, LHP);
        }
    }
    //
    [PunRPC]
    void findKillerPlayer(string playerNickname, string LHT) //Finds the appropriate quote to shout
    {
        if(PhotonNetwork.player.NickName == playerNickname)
        {
            SelfNetworkManagerII snm2 = (SelfNetworkManagerII)FindObjectOfType(typeof(SelfNetworkManagerII));
            snm2.myPlayer.GetComponent<DynamicShouting>().CharacterShoutKill(LHT);
        }
    }
    [PunRPC]
    void instantiateRagdoll(string lastHitType)
    {
        if (lastHitType == "Normal")
        {
            Instantiate(ragdollNormal, transform.position, transform.rotation);
        }
        if (lastHitType == "Explosive")
        {
            Instantiate(ragdollGib, transform.position, transform.rotation);
        }
        if (lastHitType == "Shotgun")
        {
            Instantiate(ragdollNormal, transform.position, transform.rotation);
        }
        if (lastHitType == "Tesla")
        {
            Instantiate(ragdollNormal, transform.position, transform.rotation);
        }
        if (lastHitType == "Vaporise")
        {
            Instantiate(ragdollVaporise, transform.position, transform.rotation);
        }
        if (lastHitType == "Incapacitated")
        {
            incapacitatedRagdoll = Instantiate(ragdollIncapacitated, transform.position, transform.rotation);
            // This ragdoll will be used to ressurrect the player if revived
            incapacitatedRagdoll.GetComponent<IncapacitatedPlayer>().originalPlayer = gameObject;

        }
    }

    public void AddHealth(float amount)
    {
        currentHitPoints += amount;
        if(currentHitPoints > hitPoints)
        {
            currentHitPoints = hitPoints;
        }
    }


    void Die(string LHT, string LHP)
    {

        if (snm.offlineMode == false && !snm.campaignScene)
        {
            if (IsDying == false)
            {
                IsDying = true;
                GetComponent<PhotonView>().RPC("instantiateRagdoll", PhotonTargets.All, LHT);
                if (GetComponent<PhotonView>().isMine == true)
                {
                    GetComponent<PhotonView>().RPC("findKillerPlayer", PhotonTargets.All, LHP, LHT);
                }
                if (GetComponent<PhotonView>().instantiationId == 0)
                {
                    Destroy(gameObject);
                }
                else
                {
                    if (GetComponent<PhotonView>().isMine)
                    {
                        if (gameObject.tag == "Player")
                        {

                            // Give ourselves a death stat
                            //  string nam = snm.myName;
                            if (PhotonNetwork.player.name != lastHitPlayer)
                            {
                                GameObject.FindObjectOfType<ScoreboardControllerClient>().GetComponent<PhotonView>().RPC("addKillToPlayer", PhotonTargets.AllBufferedViaServer, lastHitPlayer);
                            }

                            GameObject.FindObjectOfType<ScoreboardControllerClient>().GetComponent<PhotonView>().RPC("addDeathToPlayer", PhotonTargets.AllBufferedViaServer, PhotonNetwork.player.name);
                            GameObject killedTextInstance = Instantiate(killedText);
                            killedTextInstance.GetComponentInChildren<Text>().text = ("You were fragged by " + lastHitPlayer);
                            if (currentHitPoints < -100)
                            {
                                Destroy(killedTextInstance);
                            }
                            if (lastHitPlayer != "The Environment")
                            {
                                GetComponent<PhotonView>().RPC("DisplayFragText", PhotonTargets.Others, lastHitPlayer, PhotonNetwork.playerName.ToString());
                            }

                            snm.IsDead = true;
                            snm.standbyCamera.SetActive(true);
                            if (snm.playerRespawnTimer < 99)
                            {
                                snm.playerRespawnTimer = snm.playerRespawnTime; //Normal respawning
                            }
                            this.GetComponent<Phaser>().endBeam();
                            this.GetComponent<LanceBeam>().endBeam();
                        }
                        PhotonNetwork.Destroy(gameObject);
                        //  Invoke("AddDeath", 0.05f);
                        // Invoke("Dest", 0.1f);

                    }
                }
            }
        }
        else if (snm.offlineMode == false && snm.campaignScene)
        {
            if (IsDying == false)
            {

                
                incapacitatedCamera.enabled = true;
                IsDying = true;
                // Disable Weapon

                // Tell all ais to stop shooting at you
               // GetComponent<PhotonView>().RPC("StopAITargeting", PhotonTargets.All);
              //  GetComponent<PhotonView>().RPC("RemovePlayerTag", PhotonTargets.All);

                // Disable movement
                GetComponent<CPMPlayer>().enabled = false;
                // Disable camera
                transform.Find("FirstPersonCharacter").gameObject.GetComponent<Camera>().enabled = false;
                transform.Find("FirstPersonCharacter/ViewModelCamera").gameObject.GetComponent<Camera>().enabled = false;
                // Ragdoll self
                GetComponent<PhotonView>().RPC("instantiateRagdoll", PhotonTargets.All, "Incapacitated");
                GetComponent<PhotonView>().RPC("disablePlayer", PhotonTargets.All);
                // Create new camera to look at self on floor
                StartCoroutine("WaitForRevival");


            }
        }
    }

    IEnumerator WaitForRevival()
    {
        Debug.Log("WaitForRevival called");

        // wait a few seconds in loop

        // check if only player alive
        // Is the number of incapacitated ragdolls equal to the number of players?
        
        IncapacitatedPlayer[] incapacitatedPlayers = GameObject.FindObjectsOfType<IncapacitatedPlayer>();
        Debug.Log("Incapacitated Players:" + incapacitatedPlayers.Length);
        //CPMPlayer[] playersInGame = GameObject.FindObjectsOfType<CPMPlayer>();
        // if so, then die and fail.
        if (incapacitatedPlayers.Length >= PhotonNetwork.room.PlayerCount)
        {
            if (PhotonNetwork.isMasterClient)
            {
                // Signal to the players that the game has ended
                Debug.Log("ALL PLAYERS ARE DEAD! YOU HAVE BLOODY FAILED!");
                snm.gameObject.GetComponent<PhotonView>().RPC("missionFailed", PhotonTargets.All);
            }
        }
        else             // otherwise keep waiting for revival
        {
            yield return new WaitForSeconds(1f);
        }
    }

    [PunRPC]
    void disablePlayer()
    {
        GetComponent<CharacterController>().detectCollisions = false;
        transform.Find("PlayerModel").gameObject.SetActive(false);
        // Disable player graphics
    }
    [PunRPC]
    void revivePlayer()
    {

        IsDying = false;
        
        if (GetComponent<PhotonView>().isMine)
            {
                
                GetComponent<CharacterController>().detectCollisions = true;
                GetComponent<CPMPlayer>().enabled = true;
                transform.Find("FirstPersonCharacter").gameObject.GetComponent<Camera>().enabled = true;
                transform.Find("FirstPersonCharacter/ViewModelCamera").gameObject.GetComponent<Camera>().enabled = true;

                incapacitatedCamera.enabled = false;
  
                

            }
        transform.Find("PlayerModel").gameObject.SetActive(true);
        currentHitPoints = 25f;
        updateHUD();
        //transform.position = incapacitatedRagdoll.transform.position;
        Destroy(incapacitatedRagdoll);
    }
    [PunRPC]
    void StopAITargeting()
    {
        AiBehaviour[] aiBehaviours = GameObject.FindObjectsOfType<AiBehaviour>();
        foreach (AiBehaviour aiB in aiBehaviours)
        {
            if (aiB.gameObject.GetComponent<AiDetection>() != null) aiB.gameObject.GetComponent<AiDetection>().clearLists();

            aiB.removeTarget();
            aiB.setHasTarget(false);

  
        }
    }
    [PunRPC]
    void RemovePlayerTag()
    {
        gameObject.tag = "Untagged";
    }
    [PunRPC]
    void DisplayFragText(string fraggerName, string fraggedName)
    {
        if(fraggerName == PhotonNetwork.playerName.ToString())
        {
            GameObject killTextInstance = Instantiate(fragText);
            killTextInstance.GetComponentInChildren<Text>().text = ("You just fragged " + fraggedName);
        }
    }
    void EndDyke()
    {
        Time.timeScale = 1f;
        Application.LoadLevel(0);
    }
    public void updateHUD()
    {
        snm.UIHealth.text = currentHitPoints.ToString();
    }
    void AddDeath()
    {

    }
    void Dest()
    {

    }
}
