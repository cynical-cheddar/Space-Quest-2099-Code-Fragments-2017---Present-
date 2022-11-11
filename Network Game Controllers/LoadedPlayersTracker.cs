using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadedPlayersTracker : MonoBehaviour
{
    public int playersReady = 0;
    public int players = 0;


    private void Start()
    {
        players = PhotonNetwork.room.PlayerCount;
    }
    [PunRPC]
    public void addReady()
    {
        playersReady += 1;
    }

    [PunRPC]
    public void addPlayer()
    {
        players += 1;
    }
}
