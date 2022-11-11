using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveController : MonoBehaviour
{
    public bool wavesBegun = false;
    public GameObject[] waveObjects;
    public int currentWave = -1;
    float cooldown = 1f;
    float cooldownMax = 7f;
    public bool ignoreOtherEnemies = false;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    void spawnWave()
    {
        foreach (Transform child in waveObjects[currentWave].transform)
        {
            if (child.GetComponent<SpawnAI>() != null)
            {
                child.GetComponent<SpawnAI>().spawnEnemy();
            }
            if (child.GetComponent<NetworkDoor>() != null)
            {
                child.GetComponent<PhotonView>().RPC("openDoor", PhotonTargets.AllBuffered);
            }
            if (child.GetComponent<NetworkDialogueInstantiator>() != null)
            {
                child.GetComponent<PhotonView>().RPC("startDialogue", PhotonTargets.All);
            }
        }
        

        
    }

    // Update is called once per frame
    void Update()
    {
        if (wavesBegun && PhotonNetwork.isMasterClient)
        {
            cooldown -= Time.deltaTime;
            if (cooldown <= 0)
            {

                NetworkAi[] networkCharacters = GameObject.FindObjectsOfType<NetworkAi>();
                NetworkAiRobot[] networkRobots = GameObject.FindObjectsOfType<NetworkAiRobot>();
                if (networkCharacters.Length == 0 && networkRobots.Length == 0 && currentWave < waveObjects.Length)
                {
                    currentWave += 1;
                    spawnWave();
                    
                }
                else if(ignoreOtherEnemies && currentWave < waveObjects.Length)
                {
                    currentWave += 1;
                    spawnWave();
                }
                else if(networkCharacters.Length == 0 && currentWave == waveObjects.Length)
                {
                    Debug.Log("You've completed the bloody round, I should probably enable the exit");
                }
                cooldown = cooldownMax;
            }
        }
        // Every few seconds or so, get the amount of enemies in the scene
        // If there are no baddies, spawn the next wave.
    }
}
