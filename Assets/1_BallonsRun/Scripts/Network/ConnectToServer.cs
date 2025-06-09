using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;

public class ConnectToServer : MonoBehaviourPunCallbacks
{
    public static ConnectToServer Instance;

    private const int MaxPlayersPerRoom = 4;
    private const int MaxRooms = 5;
   
    public int Room_Number;
  public  bool RoomAvailable = false;
    public bool RoomCreated = false;
    public bool connected_tolobby;
    TypedLobby lobby;
    void Start()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        PhotonNetwork.AutomaticallySyncScene = true;
        PhotonNetwork.ConnectUsingSettings();
       
    }

    public override void OnConnectedToMaster()
    {
        Debug.LogWarning("Connected to Photon Master Server.");
        lobby = new TypedLobby("default", LobbyType.SqlLobby);

        RoomCreated = false;
        PhotonNetwork.JoinLobby();
    }
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        Debug.Log($"{otherPlayer.NickName} has left the room.");
        if (otherPlayer.IsLocal)
        {
            if (PhotonNetwork.IsMasterClient && PhotonNetwork.CurrentRoom.PlayerCount <=3)
            {
               
                PhotonNetwork.CurrentRoom.IsOpen = true;
                PhotonNetwork.CurrentRoom.IsVisible = true;
            }
        }
         
    }
    public override void OnLeftRoom()
    {
        SceneManager.LoadScene(0);
    }
    public override void OnJoinedLobby()
    {
        Debug.LogWarning("Joined Lobby...");
        connected_tolobby = true;
    }
    public void Join__Room()
    {
        PhotonNetwork.JoinRandomRoom();
    }
   
    public void Creat_Room()
    {
        if (PhotonNetwork.IsConnectedAndReady)
        {
            int RoomNumber = Random.Range(0, 5000);
            Room_Number = RoomNumber;
            RoomOptions roomOptions = new RoomOptions
            {
                MaxPlayers = MaxPlayersPerRoom,
                IsVisible = true,
                IsOpen = true
            };
               roomOptions.CustomRoomProperties = new ExitGames.Client.Photon.Hashtable()
                 {
                        { "GameType", "Race" },
                            { "MapName", "nune" }
                 };

            PhotonNetwork.CreateRoom(RoomNumber.ToString(), roomOptions);
        }
        Debug.LogWarning("Created");
        RoomCreated = true;
    }
    //public void OnJoinClicked()
    //{
    //    if (!PhotonNetwork.LocalPlayer.IsMasterClient && PhotonNetwork.InLobby && RoomAvailable)
    //    {
    //        PhotonNetwork.JoinRoom(Room_Number.ToString());
    //        Debug.LogWarning("Joind Room number notmaster " + PhotonNetwork.CurrentRoom.Name);
    //    }
    //}
  
    public override void OnDisconnected(DisconnectCause cause)
    {

        Debug.LogWarning("Connection Failed: "/* + cause*/);
        HandleConnectionFail();
    }
 
    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log("No available rooms.Creating a new room 1");

        int RoomNumber= Random.Range(0, 5000);
        Room_Number = RoomNumber;
        RoomOptions roomOptions = new RoomOptions
        {
            MaxPlayers = MaxPlayersPerRoom,
            IsVisible = true,
            IsOpen = true,
             CleanupCacheOnLeave = false
        };

        PhotonNetwork.CreateRoom(RoomNumber.ToString(), roomOptions);
        //RoomCreated = true;

    }
   
    private void HandleConnectionFail()
    {
      

        PhotonNetwork.ReconnectAndRejoin();

    }
    public override void OnCreatedRoom()
    {
        Debug.Log("Room created successfully: " + PhotonNetwork.CurrentRoom.Name);
    }

    public override void OnJoinedRoom()
    {
        Debug.LogWarning("Joined Room: " + PhotonNetwork.CurrentRoom.Name);
        Debug.LogWarning("Players in room: " + PhotonNetwork.CurrentRoom.PlayerCount);

        if (PhotonNetwork.CurrentRoom.PlayerCount == MaxPlayersPerRoom)
        {
            PhotonNetwork.CurrentRoom.IsVisible = false;
            PhotonNetwork.CurrentRoom.IsOpen = false;
            Debug.LogWarning("Room is now full and closed.");
        }
        RoomCreated = true;
    }

    //public override void OnPlayerEnteredRoom(Player newPlayer)
    //{
    //    Debug.Log("New player joined: " + newPlayer.NickName);

    //    if (PhotonNetwork.CurrentRoom.PlayerCount == MaxPlayersPerRoom)
    //    {
    //        PhotonNetwork.CurrentRoom.IsVisible = false;
    //        PhotonNetwork.CurrentRoom.IsOpen = false;
    //        Debug.Log("Room is now full and closed.");
    //    }
    //}

    void OnApplicationPause(bool pause)
    {
        if (!pause && !PhotonNetwork.IsConnected)
        {
            Debug.Log("Reconnecting...");
            PhotonNetwork.ReconnectAndRejoin();
        }
    }
}



