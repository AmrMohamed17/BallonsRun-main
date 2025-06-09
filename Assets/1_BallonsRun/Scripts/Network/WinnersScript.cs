
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;
using UnityEngine.UI;
using Photon.Realtime;

using UnityEngine.SceneManagement;

public class WinnersScript : MonoBehaviourPunCallbacks
{
    public Transform[] PlayersPos;
   public string[] playerNames = new string[3];
    string[] playerAuthNames = new string[3];
    GameObject[] playerPrefab = new GameObject[3];
    public Text[] time;
    public TextMeshProUGUI[] PlayerName;
    Rigidbody2D rig;
    Animator anim;
    PhotonView view;
    PhotonView photonView;
    int totalPlayers = 0;
    Player[] players;
    void Start()
    {
        photonView = GetComponent<PhotonView>();
      
        for (int i = 0; i < PhotonNetwork.CurrentRoom.PlayerCount; i++)
        {
            playerAuthNames[i] = PlayerPrefs.GetString("PlayerAuthName" + (i + 1).ToString());
                  playerNames[i] = PlayerPrefs.GetString("Player" + (i + 1).ToString());
            //totalPlayers++;
            playerAuthNames[i] = PlayerPrefs.GetString("PlayerAuthName" + (i + 1).ToString());
            if (!string.IsNullOrEmpty(playerNames[i]))
            {
                playerNames[i] = playerNames[i].Replace("(Clone)", "");
            }
            else
            {
                Debug.LogError($"Player {i + 1} name is empty or not found in PlayerPrefs.");
            }
        }

        SetPositionsAndDisplay();
    }
   
    public void SetPositionsAndDisplay()
    {
        
        for (int i = 0; i <PhotonNetwork.CurrentRoom.PlayerCount; i++)
        {
            Debug.LogWarning("Player_Name "+playerNames[i]);
           
            GameObject player = PhotonNetwork.Instantiate(playerNames[i], PlayersPos[i].position, PlayersPos[i].transform.rotation);
            player.transform.localScale = new Vector3(60f, 60f, 0.0f);

            if (player.GetComponent<Rigidbody2D>() == null)
                player.AddComponent<Rigidbody>();

            rig = player.GetComponent<Rigidbody2D>();
            rig.constraints = RigidbodyConstraints2D.FreezeRotation;
            rig.constraints = RigidbodyConstraints2D.FreezePositionY;
            rig.constraints = RigidbodyConstraints2D.FreezePositionX;
            Destroy(rig);
            anim = player.GetComponentInChildren<Animator>();
            anim.SetTrigger("win");

           

            view = player.GetComponent<PhotonView>();
            
            int min =( int)(RaceManager.Instance.FinishTime[i]/60);
            
            if (min == 0)
                time[i].text = "00 : " + RaceManager.Instance.FinishTime[i].ToString();
            else
            {
                float sec = RaceManager.Instance.FinishTime[i] % 60;
                time[i].text = min.ToString() + ":" + sec.ToString();
            }

            PlayerName[i].text = RaceManager.Instance.Names[i];
                
        }
    }

  
    public void QuitGame()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            photonView.RPC("ExitRoomRPC", RpcTarget.AllBuffered);
        }
        else
        {
            PhotonNetwork.LeaveRoom();
        }
    }

    public void HomeButton()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            photonView.RPC("ExitRoomRPC", RpcTarget.AllBuffered);
        }
        else
        {
            PhotonNetwork.LeaveRoom();
        }
    }
    public override void OnLeftRoom()
    {

        //PhotonNetwork.LoadLevel(0);
        SceneManager.LoadScene(0);
    }

    [PunRPC]
    public void ExitRoomRPC()
    {
        PhotonNetwork.LeaveRoom();
    }

}

