using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;
using Photon.Realtime;
using TMPro;

public class MapChoosing : MonoBehaviourPunCallbacks
{
    [SerializeField] TextMeshProUGUI map1_tex;
    [SerializeField] TextMeshProUGUI map2_tex;
    //[SerializeField] TextMeshProUGUI timer_text;
    [SerializeField] Toggle Map1;
    [SerializeField] Toggle Map2;
    float timer =3;
    int map1_Voting = 0;
    int map2_Voting = 0;
    bool votedForMap1 = false;
    bool votedForMap2 = false;

    int test =2;
    public string CurrenPlayer;

    private PhotonView photonView;
   
  
    private void Awake()
    {
        photonView = GetComponent<PhotonView>();
    }

    private void Start()
    {
        StartCoroutine(Timer());
    }
  
    public void Choosed_Map()
    {
        if (map1_Voting >= map2_Voting)
        {
            PlayerPrefs.SetInt("Map", 1);
        }
        else
        {
            PlayerPrefs.SetInt("Map", 2);
        }
    }

    public void Map1Voting()
    {
        if (!votedForMap1) 
        {
            if (votedForMap2) 
            {
                
                map2_Voting -= 1;
                if (map2_Voting < 0)
                    map2_Voting = 0;
                votedForMap2 = false; 
            }

            map1_Voting += 1; 
            votedForMap1 = true; 
        }
        else 
        {
            
            map1_Voting -= 1;
            if (map1_Voting < 0)
                map1_Voting = 0;
            votedForMap1 = false; 
        }

     
        photonView.RPC("UpdateVoting", RpcTarget.All, map1_Voting, map2_Voting);
    }
    public override void OnMasterClientSwitched(Player newMasterClient)
    {



        if (PhotonNetwork.IsMasterClient)
        {

            if (PhotonNetwork.CurrentRoom.PlayerCount == 1)
            {
                PhotonNetwork.LeaveRoom();
                PhotonNetwork.LoadLevel(0);
            }
           
        }
    }
    public void Map2Voting()
    {
        if (!votedForMap2)
        {
            if (votedForMap1) 
            {
                map1_Voting -= 1;
                if (map1_Voting < 0)
                    map1_Voting = 0;
                votedForMap1 = false; 
            }

            map2_Voting += 1;
            votedForMap2 = true; 
        }
        else 
        {
            map2_Voting -= 1;
            if (map2_Voting < 0)
                map2_Voting = 0;
            votedForMap2 = false; 
        }

       
        photonView.RPC("UpdateVoting", RpcTarget.All, map1_Voting, map2_Voting);
    }

    [PunRPC]
    void UpdateVoting(int map1Votes, int map2Votes)
    {
        map1_Voting = map1Votes;
        map2_Voting = map2Votes;

    
        map1_tex.text = map1_Voting.ToString();
        map2_tex.text = map2_Voting.ToString();
    }

    public IEnumerator Timer()
    {
        while (timer >= 0)
        {
            //timer_text.text = timer.ToString();
            yield return new WaitForSeconds(1f);
            timer--;
        }

        if (map1_Voting >= map2_Voting)
        {
            if (PhotonNetwork.CurrentRoom.PlayerCount == test)
                PhotonNetwork.LoadLevel(2);
        }
        else
        {
            if (PhotonNetwork.CurrentRoom.PlayerCount == test)
                PhotonNetwork.LoadLevel(2);
        }
        yield return null;
    }
}