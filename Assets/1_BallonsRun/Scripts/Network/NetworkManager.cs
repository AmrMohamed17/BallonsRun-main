using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Realtime;
using Photon.Pun;
using UnityEngine.Events;
public class NetworkManager : MonoBehaviourPunCallbacks
{
    private void Start()
    {
        PhotonNetwork.SendRate = 60;
        PhotonNetwork.SerializationRate =30;
    }


    public void Join_Room()
    {
        PhotonNetwork.JoinRandomRoom();
        Debug.LogWarning("trying to join a random room...");

    }
}

  

