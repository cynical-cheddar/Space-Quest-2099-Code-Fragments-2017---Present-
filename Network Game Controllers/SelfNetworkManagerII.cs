using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System.IO;
public class SelfNetworkManagerII : MonoBehaviour {

    SpawnPoint[] spawnPoints;
    public bool campaignScene = false;
    public bool allowTeamAndCharChange = true;
    public bool offlineMode = false;
    public bool TC = true;
    public bool RR = false;
    public bool Campaign = false;
    bool connecting = false;

    List<string> chatMessages;
    int maxChatMessages = 5;
    bool hasPickedTeam = false;
    public int teamIDSaved = 0;
    public float playerRespawnTimer = 0f;
    public float playerRespawnTime = 4f;
    public GameObject standbyCamera;

    public int DebugTeamID;
    public Transform top;

    public GameObject TeamSelect;
    public GameObject CharacterSelect;
    public bool IsDead = true;
    public string SaveCharName;
    public string CapturePointPrefabName;

    public GameObject myPlayer;
    GameObject pauseMenu;
    // UI
    public GameObject PlayerUI;
    public Text UIClipAmmo;
    public Text UIReserveAmmo;
    public Text UIHealth;
    public string myName;
    CursorLockMode lockmode;

    public bool automaticallyFindGuiStuff = true;
    void Start()
    {
        if (offlineMode == false)
        {
            spawnPoints = GameObject.FindObjectsOfType<SpawnPoint>();
            // PhotonNetwork.player.name = PlayerPrefs.GetString("Username", "Player");
            chatMessages = new List<string>();
            standbyCamera = GameObject.Find("StandbyCamera");
            PlayerUI = GameObject.Find("_UserHUD");
            PlayerUI.GetComponent<Canvas>().enabled = false;
            if (automaticallyFindGuiStuff == true)
            {
                UIClipAmmo = PlayerUI.transform.Find("ClipWeapon").Find("TxtClipAmmo").gameObject.GetComponent<Text>();
                UIReserveAmmo = PlayerUI.transform.Find("ClipWeapon").Find("TxtReserveAmmo").gameObject.GetComponent<Text>();
                UIHealth = PlayerUI.transform.Find("Health+Armor").Find("Health").gameObject.GetComponent<Text>();
            }
            if (PhotonNetwork.isMasterClient == true && TC == true)
            {
                CapturePointInstantiation();
                LiftInstantiation();
            }
            if (GetComponent<PhotonView>().isMine == true)
            {
                myName = PhotonNetwork.player.name;
            }
            Cursor.lockState = CursorLockMode.None;
            Cursor.lockState = CursorLockMode.Confined;
            Cursor.visible = true;
            pauseMenu = Instantiate(Resources.Load("PauseMenu", typeof(GameObject))) as GameObject;
            pauseMenu.transform.Find("MasterMenu").gameObject.SetActive(false);
            
            if (GetComponent<PersistWhiteLevel>() != null)
            {
                pauseMenu.AddComponent<PersistWhiteLevel>();
                pauseMenu.GetComponent<PersistWhiteLevel>().whiteLevels = new List<string>();
                foreach (string level in GetComponent<PersistWhiteLevel>().whiteLevels)
                {
                    if (level != null)
                    {
                        pauseMenu.GetComponent<PersistWhiteLevel>().whiteLevels.Add(level);
                    }
                }
            }
            
        }
        else {
            PhotonNetwork.offlineMode = true;
            standbyCamera = GameObject.Find("StandbyCamera");
            PlayerUI = GameObject.Find("_UserHUD");
           // PlayerUI.GetComponent<Canvas>().enabled = false;
            if (automaticallyFindGuiStuff == true)
            {
                UIClipAmmo = PlayerUI.transform.Find("ClipWeapon").Find("TxtClipAmmo").gameObject.GetComponent<Text>();
                UIReserveAmmo = PlayerUI.transform.Find("ClipWeapon").Find("TxtReserveAmmo").gameObject.GetComponent<Text>();
                UIHealth = PlayerUI.transform.Find("Health+Armor").Find("Health").gameObject.GetComponent<Text>();
            }
            GameObject.FindGameObjectWithTag("Player").GetComponent<Health>().setStuff();
        }
        if(RR == true) 
        {
            playerRespawnTime = GetComponent<FreeForAllManager>().respawnTime;
        }
    }
    void OnDestroy()
    {
        Debug.Log("SelfNetworkManagerII has been destroyed!");
    }
    void CapturePointInstantiation()
    {
        GameObject[] capturePointPositions = GameObject.FindGameObjectsWithTag("CapturePointPosition");
        foreach(GameObject capturePointPosition in capturePointPositions)
        {
           int pointID = capturePointPosition.GetComponent<CapturePointTeamID>().PointTeamID;
            string pointName = capturePointPosition.GetComponent<CapturePointTeamID>().pointName;
            GameObject CapturePoint = PhotonNetwork.InstantiateSceneObject(pointName, capturePointPosition.transform.position, capturePointPosition.transform.rotation, 0, null);
        }
    }
    void LiftInstantiation()
    {
        GameObject[] liftPositions = GameObject.FindGameObjectsWithTag("LiftPosition");
        foreach (GameObject liftPosition in liftPositions)
        {
            string liftName = liftPosition.GetComponent<LiftID>().liftName;
            GameObject Lift = PhotonNetwork.InstantiateSceneObject(liftName, liftPosition.transform.position, liftPosition.transform.rotation, 0, null);
        }
    }

    void Connect()
    {
        PhotonNetwork.ConnectUsingSettings("v001");

    }
    public void AddChatMessage(string m)
    {
        GetComponent<PhotonView>().RPC("AddChatMessage_RPC", PhotonTargets.AllBuffered, m);
    }
    [PunRPC]
    void AddChatMessage_RPC(string m)
    {
        while (chatMessages.Count >= maxChatMessages)
        {
            chatMessages.RemoveAt(0);
        }
        chatMessages.Add(m);
    }

    void OnGUI()
    {
        if (offlineMode == false)
        {
            GUILayout.Label(PhotonNetwork.connectionStateDetailed.ToString());

            if (PhotonNetwork.connected == true && connecting == false)
            {
                if (hasPickedTeam)
                {
                    //Fully connected so display chat
                    GUILayout.BeginArea(new Rect(0, 0, Screen.width, Screen.height));
                    GUILayout.BeginVertical();
                    GUILayout.FlexibleSpace();

                    foreach (string msg in chatMessages)
                    {
                        GUILayout.Label(msg);
                    }

                    GUILayout.EndVertical();
                    GUILayout.EndArea();

                }
                else
                {
                    //Player has not selected team
                    // If the teampersistGameObject doesn't exist, or if it exists and has firstround as true
                    if (GameObject.FindWithTag("TeamPersist") == null) // if it doesn't exist
                    {
                        if (allowTeamAndCharChange)
                        {
                            teamIDSaved = 0;
                            DisplayTeamSelect();
                        }
                    }
                    else if (GameObject.FindWithTag("TeamPersist").GetComponent<TeamPersistClient>().firstRound == true) // if it's the first round
                    {
                        //teamIDSaved = GameObject.FindWithTag("TeamPersist").GetComponent<TeamPersistClient>().lastTeam;
                        if (allowTeamAndCharChange)
                        {
                            DisplayTeamSelect();
                        }
                    }

                    else // If it exists and it's not the first round
                    {
                        hasPickedTeam = true;
                        teamIDSaved = GameObject.FindWithTag("TeamPersist").GetComponent<TeamPersistClient>().lastTeam;
                        if(allowTeamAndCharChange)
                        {
                            DisplayCharacterSelect();
                        }
                        
                    }

                }
            }
        }
    }
                                                         // SELECT TEAM
    public void RenegadeTeam()
    {
        hasPickedTeam = true;
        teamIDSaved = 0;
        if (myPlayer != null)
        {
            myPlayer.GetComponent<Phaser>().endBeam();
            myPlayer.GetComponent<LanceBeam>().endBeam();
            PhotonNetwork.Destroy(myPlayer);
        }
        DisplayCharacterSelect();
    }
    public void RedTeam()
    {

        hasPickedTeam = true;
        teamIDSaved = 1;
        if (myPlayer != null)
        {
            myPlayer.GetComponent<Phaser>().endBeam();
            myPlayer.GetComponent<LanceBeam>().endBeam();
            PhotonNetwork.Destroy(myPlayer);
        }
        DisplayCharacterSelect();
    }
    public void BlueTeam()
    {

        hasPickedTeam = true;
        teamIDSaved = 2;
        if (myPlayer != null)
        {
            myPlayer.GetComponent<Phaser>().endBeam();
            myPlayer.GetComponent<LanceBeam>().endBeam();
            PhotonNetwork.Destroy(myPlayer);
        }
        DisplayCharacterSelect();
    }
    public void YellowTeam()
    {

        hasPickedTeam = true;
        teamIDSaved = 3;
        if (myPlayer != null)
        {
            myPlayer.GetComponent<Phaser>().endBeam();
            myPlayer.GetComponent<LanceBeam>().endBeam();
            PhotonNetwork.Destroy(myPlayer);
        }
        DisplayCharacterSelect();
    }
                                                        // END SELECT TEAM
                                                        // INITIALISE GUI
    void DisplayTeamSelect()
    {
        if(myPlayer != null)
        {
            myPlayer.GetComponent<CanFire>().CanShoot = false;
        }
        if (standbyCamera != null)
        {
            standbyCamera.SetActive(false);
        }
        TeamSelect.SetActive(true);
        CharacterSelect.SetActive(false);
    }
    void DisplayCharacterSelect()
    {
        if (myPlayer != null)
        {
            myPlayer.GetComponent<CanFire>().CanShoot = false;
        }
        if (standbyCamera != null)
        {
            standbyCamera.SetActive(false);
        }
        TeamSelect.SetActive(false);
        CharacterSelect.SetActive(true);
    }// Add the ability to close menu
                                                        // END INITIALISE GUI
                                                        // SELECT CHARACTER
    public void SelectBoris()
    {
        CharacterSelect.SetActive(false);

        SaveCharName = "FPSBorisQuake";
        if (campaignScene)
        {
            SaveCharName = "FPSBorisQuakeCoop";
        }
        SpawnMyPlayer(teamIDSaved);

    }
    public void SelectDave()
    {
        CharacterSelect.SetActive(false);

        SaveCharName = "FPSDaveQuake";
        if (campaignScene)
        {
            SaveCharName = "FPSDaveQuakeCoop";
        }
        SpawnMyPlayer(teamIDSaved);

    }
    public void SelectGeddon()
    {
        CharacterSelect.SetActive(false);

        SaveCharName = "FPSGeddonQuake";
        if (campaignScene)
        {
            SaveCharName = "FPSGeddonQuakeCoop";
        }
        SpawnMyPlayer(teamIDSaved);

    }

    public void SelectVlad()
    {
        CharacterSelect.SetActive(false);

        SaveCharName = "FPSRobQuake"; //REMOVE QUAKE
        SpawnMyPlayer(teamIDSaved);

    }
    // END SELECT CHARACTER
    void OnJoinedLobby()
    {
        Debug.Log("Joined");
        PhotonNetwork.JoinRandomRoom();
    }
    void OnConnectedToMaster()
    {
        Debug.Log("Joined");
        PhotonNetwork.JoinRandomRoom();
    }
    void OnPhotonRandomJoinFailed()
    {
        PhotonNetwork.CreateRoom(null);
    }
    void OnJoinedRoom()
    {
        connecting = false;
    }
            //     if(PhotonNetwork.isMasterClient == true) // If you're the master when you join a room, then spawn the capture points.
            //     {
            //         CapturePointInstantiation();
            //     }
    void Update()
    {
        if (playerRespawnTimer > 0)
        {
            PlayerUI.GetComponent<Canvas>().enabled = false;
            playerRespawnTimer -= Time.deltaTime;
            if (playerRespawnTimer <= 0)
            {
                //Respawn player
                SpawnMyPlayer(teamIDSaved);

            }
        }
        if (Input.GetButtonDown("CharacterChange") && !campaignScene)
        {
            DisplayCharacterSelect();
            Cursor.lockState = CursorLockMode.None;
            Cursor.lockState = CursorLockMode.Confined;
            Cursor.visible = true;


            PlayerUI.GetComponent<Canvas>().enabled = false;
        }
        if (Input.GetButtonDown("TeamChange") && !campaignScene)
        {
            DisplayTeamSelect();
            PlayerUI.GetComponent<Canvas>().enabled = false;
            Cursor.lockState = CursorLockMode.None;
            Cursor.lockState = CursorLockMode.Confined;
            Cursor.visible = true;
        }

        if (Input.GetButtonDown("Menu"))
        {
            myPlayer.GetComponent<CanFire>().CanShoot = false;
            myPlayer.GetComponent<CanFire>().CanSwitch = false;
           // myPlayer.GetComponentInChildren<EnhancedMouseLook>().enabled = false;
            pauseMenu.transform.Find("MasterMenu").gameObject.SetActive(true);
            PlayerUI.GetComponent<Canvas>().enabled = false;
            Cursor.lockState = CursorLockMode.None;
            Cursor.lockState = CursorLockMode.Confined;
            Cursor.visible = true;
        }

        if (Input.GetButtonDown("Scoreboard"))
        {
            PlayerUI.transform.Find("Scoreboard").GetComponent<RectTransform>().localScale = new Vector3 (1,1,1);
        }
        if (Input.GetButtonUp("Scoreboard"))
        {
            PlayerUI.transform.Find("Scoreboard").GetComponent<RectTransform>().localScale = new Vector3(0, 0, 0);
        }
    }
    void SpawnMyPlayer(int teamID)
    {
        if (offlineMode == false)
        {
            if (myPlayer != null)
            {
                PhotonNetwork.Destroy(myPlayer);
            }
            if (GameObject.FindWithTag("TeamPersist") != null)
            {
                GameObject.FindWithTag("TeamPersist").GetComponent<TeamPersistClient>().lastTeam = teamIDSaved;
            }

            teamID = teamIDSaved;



            AddChatMessage("Spawning player: " + PhotonNetwork.player.name);
            if (standbyCamera != null)
            {
                standbyCamera.SetActive(false);
            }
            SpawnPoint mySpawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
            while (mySpawnPoint.teamID != teamID)
            {
                mySpawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
            }
            GameObject myPlayerGO = PhotonNetwork.Instantiate(SaveCharName, mySpawnPoint.transform.position, mySpawnPoint.transform.rotation, 0);
            myPlayer = myPlayerGO;

       //     myPlayerGO.GetComponentInChildren<EnhancedMouseLook>().enabled = true;
            GameObject myPlayerGOcam = myPlayerGO.transform.Find("FirstPersonCharacter").gameObject;
            myPlayerGOcam.GetComponent<Camera>().enabled = true;
            myPlayerGOcam.GetComponent<AudioListener>().enabled = true;
            myPlayerGOcam.GetComponent<FlareLayer>().enabled = true;
            myPlayerGO.GetComponent<PlayerMovement>().enabled = true;
            myPlayerGO.GetComponent<Phaser>().enabled = true;
            myPlayerGO.GetComponent<WeaponSwitch>().enabled = true;
            myPlayerGO.GetComponent<CPMPlayer>().enabled = true; // CHANGE THIS BACK TO ENHANCED FPS CONTROLLER IF QUAKE FAILS
            Transform viewModelCamera = myPlayerGO.transform.Find("FirstPersonCharacter/ViewModelCamera");

            viewModelCamera.gameObject.GetComponent<Camera>().enabled = true;

            PlayerUI.GetComponent<Canvas>().enabled = true;

            Transform playerModel = myPlayerGO.transform.Find("PlayerModel");
            playerModel.gameObject.layer = 9;
            Transform[] playerModelChildren = playerModel.GetComponentsInChildren<Transform>();
            Debug.Log("About to make invisible on local end");
            foreach (Transform child in playerModelChildren)
            {
                child.gameObject.layer = 9;
            }
            Transform WeaponModelParent = myPlayerGO.transform.Find("PlayerModel/mixamorig:Hips/mixamorig:Spine/mixamorig:Spine1/mixamorig:Spine2/mixamorig:LeftShoulder/mixamorig:LeftArm/mixamorig:LeftForeArm/mixamorig:LeftHand/GunPlacement");
            Transform[] WeaponModels = WeaponModelParent.GetComponentsInChildren<Transform>();
            foreach (Transform child in playerModelChildren)
            {
                child.gameObject.SetActive(false);
            }
            Transform viewModels = myPlayerGO.transform.Find("FirstPersonCharacter/ViewModels");
            viewModels.gameObject.SetActive(true);
            myPlayerGO.GetComponent<PhotonView>().RPC("SetTeamID", PhotonTargets.AllBuffered, teamID);
            playerModel.gameObject.SetActive(true);
            IsDead = false;

            GameObject.FindObjectOfType<ScoreboardControllerClient>().GetComponent<PhotonView>().RPC("assignTeamToPlayer", PhotonTargets.AllBuffered, PhotonNetwork.player.name, teamID); // We call the RPC on the scoreboard manager to make it aware of our team change.

            Cursor.lockState = CursorLockMode.None;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            pauseMenu.GetComponent<PauseMenu>().myPlayer = myPlayer;
            string FOV;
            if (File.Exists(Application.persistentDataPath + " settings.txt"))
            {
                string[] strings = File.ReadAllLines(Application.persistentDataPath + " settings.txt");
             //   myPlayer.transform.Find("FirstPersonCharacter").GetComponent<EnhancedMouseLook>().sensitivity = new Vector2(float.Parse(strings[strings.Length - 3]), float.Parse(strings[strings.Length - 3]));
                myPlayer.transform.Find("FirstPersonCharacter").gameObject.GetComponent<Camera>().fieldOfView = float.Parse(strings[strings.Length - 2]);
                myPlayer.transform.Find("FirstPersonCharacter").Find("ViewModelCamera").gameObject.GetComponent<Camera>().fieldOfView = float.Parse(strings[strings.Length - 1]);
            }
        }
    }

}
