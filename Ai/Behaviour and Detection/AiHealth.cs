using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AiHealth : Health {

	float cooldown = 3f;
	float cooldownRemaining = 2f;
	public string aiName = "Rob";
	public float painChance = 0.5f;
	public float painDamageThreshold = 10f;
	public AudioClip[] painShouts;
	public GameObject pickupDrop;
	public bool applyForce = true;
	public float force = 100f;
    public GameObject specialDialogueSpawn;
    public GameObject specialDialogueQuarterWay;
    public GameObject specialDialogueThirdWay;
    public GameObject specialDialogueHalfWay;
    public GameObject specialDialogueTwoThirdWay;
    public GameObject specialDialogueThreeQuarterWay;
    bool quarterFired = false;
    bool halfwayFired = false;
    bool threeQuarterFired = false;
    bool thirdFired = false;
    bool twothirdFired = false;
    public bool playHitSound = true;
    public bool playKillDialogue = true;
    public GameObject specialDialogueOnKill;
    public bool boolSpecialDialogueOnKill = false;

    void Start () {
		if (GetComponent<PhotonView>().isMine)
		{
			currentHitPoints = hitPoints;

		}
        if (PhotonNetwork.isMasterClient)
        {
            GetComponent<PhotonView>().RPC("startDialogue", PhotonTargets.All);
        }
	}
	public void setStuff()
	{
		currentHitPoints = hitPoints;
	}

	void Update(){
		cooldownRemaining -= Time.deltaTime;
		if (cooldownRemaining <= 0) {
			cooldownRemaining = cooldown;
			if (currentHitPoints < 0 && PhotonNetwork.isMasterClient) {
				JustBloodyDie ();
			}
		}
	}

	[PunRPC]
	public void TakeDamage(float amt, string LHP, string LHT)
	{

		lastHitPlayer = LHP;
		lastHitType = LHT;
		amt = Mathf.Round(amt);
		currentHitPoints -= amt;

		if (currentHitPoints <= 0)
		{
			Die(LHT, LHP);
		}
		if (PhotonNetwork.isMasterClient) {
			if (playHitSound && amt >= painDamageThreshold && (painChance >= Random.Range(0f, 1f))) {
				GetComponent<PhotonView>().RPC("Pain", PhotonTargets.All);
			}
            if (quarterFired == false && currentHitPoints < hitPoints / 1.5 && specialDialogueQuarterWay != null)
            {
                quarterFired = true;
                GetComponent<PhotonView>().RPC("instantiateQuarterWayDialogue", PhotonTargets.All);
            }
            if (thirdFired == false && currentHitPoints < hitPoints / 1.75 && specialDialogueThirdWay != null)
            {
                thirdFired = true;
                GetComponent<PhotonView>().RPC("instantiateThirdWayDialogue", PhotonTargets.All);
            }
            if (halfwayFired == false && currentHitPoints < hitPoints /2 && specialDialogueHalfWay != null)
            {
                halfwayFired = true;
                GetComponent<PhotonView>().RPC("instantiateHalfwayDialogue", PhotonTargets.All);
            }
            if (twothirdFired == false && currentHitPoints < hitPoints / 3 && specialDialogueTwoThirdWay != null)
            {
                twothirdFired = true;
                GetComponent<PhotonView>().RPC("instantiateTwoThirdWayDialogue", PhotonTargets.All);
            }
            if (threeQuarterFired == false && currentHitPoints < hitPoints / 4 && specialDialogueThreeQuarterWay != null)
            {
                threeQuarterFired = true;
                GetComponent<PhotonView>().RPC("instantiateThreeQuarterWayDialogue", PhotonTargets.All);
            }
        }
        if (GetComponent<AiDetection>() != null)
        {
            GetComponent<AiDetection>().alertIfShot();
        }

    }
    [PunRPC]
    void instantiateQuarterWayDialogue()
    {
        if (GameObject.FindObjectOfType<SpeechIndicator>() == null)
        {
            Instantiate(specialDialogueQuarterWay);
        }
        else
        {
            Invoke("instantiateQuarterWayDialogue", 0.5f);
        }

    }
    [PunRPC]
    void instantiateThirdWayDialogue()
    {
        if (GameObject.FindObjectOfType<SpeechIndicator>() == null)
        {
            Instantiate(specialDialogueThirdWay);
        }
        else
        {
            Invoke("instantiateThirdWayDialogue", 0.5f);
        }

    }
    [PunRPC]
    void instantiateHalfwayDialogue()
    {
        if(GameObject.FindObjectOfType<SpeechIndicator>() == null)
        {
            Instantiate(specialDialogueHalfWay);
        }
        else
        {
            Invoke("instantiateHalfwayDialogue", 0.5f);
        }
 
    }
    [PunRPC]
    void instantiateTwoThirdWayDialogue()
    {
        if (GameObject.FindObjectOfType<SpeechIndicator>() == null)
        {
            Instantiate(specialDialogueTwoThirdWay);
        }
        else
        {
            Invoke("instantiateTwoThirdWayDialogue", 0.5f);
        }

    }
    [PunRPC]
    void instantiateThreeQuarterWayDialogue()
    {
        if (GameObject.FindObjectOfType<SpeechIndicator>() == null)
        {
            Instantiate(specialDialogueThreeQuarterWay);
        }
        else
        {
            Invoke("instantiateThreeQuarterWayDialogue", 0.5f);
        }

    }

    [PunRPC]
    void killDialogue()
    {
        if (specialDialogueOnKill != null)
        {
            Instantiate(specialDialogueOnKill);
        }
    }
    [PunRPC]
    void startDialogue()
    {
        if (specialDialogueSpawn != null)
        {
            Instantiate(specialDialogueSpawn);
        }
    }
    [PunRPC]
	public void Pain(){
		GetComponent<AiBehaviour> ().applyPain ();
		int clipNumber = Random.Range(0, painShouts.Length -1);
		GetComponent<AudioSource>().clip = painShouts[clipNumber];
		GetComponent<AudioSource>().Play();
	}

	//
	[PunRPC]
	void findKillerPlayer(string playerNickname, string LHT) //Finds the appropriate quote to shout
	{



        if (PhotonNetwork.player.NickName == playerNickname)
		{
			SelfNetworkManagerII snm2 = (SelfNetworkManagerII)FindObjectOfType(typeof(SelfNetworkManagerII));
            if(specialDialogueOnKill == null)
            {
                if (playKillDialogue) snm2.myPlayer.GetComponent<DynamicShouting>().CharacterShoutKill(LHT);

            }

			
		}
	}
	[PunRPC]
	void instantiateRagdoll(string lastHitType)
	{
		if (lastHitType == "Normal")
		{
			GameObject ragdoll = Instantiate(ragdollNormal, transform.position, transform.rotation);
			if (applyForce == true) {
				//ragdoll.GetComponent<Rigidbody> ().AddForce(ragdoll.transform.forward * force, ForceMode.Impulse);
			}
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
	}




	void Die(string LHT, string LHP)
	{
		if (PhotonNetwork.isMasterClient && IsDying == false){
				IsDying = true;
            
				GetComponent<AiBehaviour> ().unclaimCover ();
            
            if (pickupDrop != null) {
					PhotonNetwork.Instantiate (pickupDrop.name, transform.position, transform.rotation, 0);
				}
            GetComponent<PhotonView>().RPC("killDialogue", PhotonTargets.All);
            GetComponent<PhotonView>().RPC("instantiateRagdoll", PhotonTargets.All, LHT);
				if (PhotonNetwork.isMasterClient == true)
				{
					GetComponent<PhotonView>().RPC("findKillerPlayer", PhotonTargets.All, LHP, LHT);
				}
				if (GetComponent<PhotonView>().instantiationId == 0)
				{
					Destroy(gameObject);
				}
				else
				{
					PhotonNetwork.Destroy(gameObject);
						//  Invoke("AddDeath", 0.05f);
						// Invoke("Dest", 0.1f);
				}
			}
	}

	void JustBloodyDie(){
        
    
        GetComponent<PhotonView>().RPC("killDialogue", PhotonTargets.All);
        GetComponent<PhotonView>().RPC("instantiateRagdoll", PhotonTargets.All, lastHitType);
        if (PhotonNetwork.isMasterClient == true)
		{
			GetComponent<PhotonView>().RPC("findKillerPlayer", PhotonTargets.All, lastHitPlayer, lastHitType);
            if (pickupDrop != null)
            {
                PhotonNetwork.Instantiate(pickupDrop.name, transform.position, transform.rotation, 0);
            }
        }
		if (GetComponent<PhotonView>().instantiationId == 0)
		{
			PhotonNetwork.Destroy(gameObject);
		}

		else
		{
            GetComponent<AiBehaviour>().unclaimCover();
            PhotonNetwork.Destroy(gameObject);
			//  Invoke("AddDeath", 0.05f);
			// Invoke("Dest", 0.1f);
		}
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

	void AddDeath()
	{

	}
	void Dest()
	{

	}

}
