using System.Collections;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;
using Photon.Realtime;
using TMPro;
using UnityEngine.SceneManagement;

public class PlayerConnecting : MonoBehaviourPunCallbacks
{
    public Image[] playersprit;
    int index = 0;
  
    [SerializeField] Button startButton;
    [SerializeField] TextMeshProUGUI timer_text;
    [SerializeField] TextMeshProUGUI TimeTest;
    //[SerializeField] Slider slider;
    //[SerializeField] Slider slider2;
    //[SerializeField] Slider slider3;
    //float timer = 0;
    public float speed = 1f;
    //[SerializeField] TMP_Text PlayerName;
    [SerializeField] TMP_Text Player_Name;
    [SerializeField] RawImage Player_Image;
    [SerializeField] Slider Player_Level_Slider;
    int test =2;
    bool StartTheGame = false;
    bool FalgeForThelobbyScene = false;
    PhotonView View;
    bool flage = false;
    int playerID=0;
    int actorNum=0;
   [SerializeField] GameObject loading_forConnection;
   //[SerializeField] GameObject forConnection2;
    //[SerializeField] Scrollbar scrollbar;
    //[SerializeField] Scrollbar scrollbar2;
    public bool StartTheGame1
    {
        get => StartTheGame; set
        {
            StartTheGame = value;

        
        }
    }

    private void Start()
    {
        if (!PhotonNetwork.IsConnected)
        {
            PhotonNetwork.ConnectUsingSettings();
        }
        View = GetComponent<PhotonView>();
       
        //flage = true;
        StartCoroutine(SettingPlayerData());
        ConnectionSet();
        //forConnection2.SetActive(true);
    }

    private void Update()
    {
        //slider.value += speed * Time.deltaTime;

        //if (slider.value >= slider.maxValue)
        //{
        //    slider.value = slider.minValue;
        //}
        //slider2.value += speed * Time.deltaTime;

        //if (slider2.value >= slider2.maxValue)
        //{
        //    slider2.value = slider2.minValue;
        //}
        //slider3.value += speed * Time.deltaTime;

        //if (slider3.value >= slider3.maxValue)
        //{
        //    slider3.value = slider3.minValue;
        //}
        if (!ConnectToServer.Instance.RoomCreated )
        {

            ConnectionSet();
          
        }
        else if (ConnectToServer.Instance.RoomCreated)
        {
            //if (scrollbar.value != 1)
            //{
            //    scrollbar.value += speed * Time.deltaTime;
            //    scrollbar.size += speed * Time.deltaTime;
            //    scrollbar.numberOfSteps = 3;
            //}
            //else
            //{
            //    scrollbar.value = 1;
            //    scrollbar.size = 1;
                ConnectionSetCancle();
            //}
           
          
        }
        if (!ConnectToServer.Instance.connected_tolobby)
        {
            //scrollbar2.value += 0.5f * Time.deltaTime;
            //scrollbar2.size += 0.5f * Time.deltaTime;
            //scrollbar2.numberOfSteps = 2;
            //if (scrollbar2.value >= 0.99f)
            //    scrollbar.value = 0.80f;
            //forConnection2.SetActive(true);
        }
        else
        {
            //forConnection2.SetActive(false);
        }
        

    }
    public void ConnectionSet()
    {
        //slider2.gameObject.SetActive(true);
        //scrollbar.gameObject.SetActive(true);
        loading_forConnection.SetActive(true);
      
    }
    public void ConnectionSetCancle()
    {
        loading_forConnection.SetActive(false);
       
    }
    
    public override void OnJoinedRoom()
    {
        ConnectionSetCancle();
        
      
        if (PhotonNetwork.IsMasterClient)
        {
            StartCounting();
        }
        CopyPlayer();
        actorNum = PhotonNetwork.LocalPlayer.ActorNumber ;
      
        flage = true;
        //PhotonNetwork.NickName = AuthManager.Instance.Player_Name;
        Debug.LogWarning("rooom " + PhotonNetwork.CurrentRoom.Name);
    }

    [PunRPC]
    private void CopyPlayerRPC(string avatarName)
    {
        GameObject playerAvatar = Resources.Load<GameObject>(avatarName);

        if (playerAvatar != null)
        {
            if (index <=3)
            {

                playersprit[index].sprite = playerAvatar.GetComponentInChildren<SpriteRenderer>().sprite;
                playersprit[index].color = new Vector4(1, 1, 1, 1);
                playersprit[index].name = actorNum.ToString();
               //TimeTest.text = "Index = : " + index + "  :" + PhotonNetwork.CurrentRoom.PlayerCount.ToString()+ " "+ ConnectToServer.Instance.RoomNumber+" "+flage;
            }
        }
        else
        {
            Debug.LogError($"Player avatar not found for {avatarName}");
        }
        View.RPC("updateIndex", RpcTarget.AllBuffered);
    }
    private void CopyPlayer()
    {
        string selectedAvatar = ShopManager.Instance.Selected_Player;
        View.RPC("CopyPlayerRPC", RpcTarget.AllBuffered, selectedAvatar);

    }

    public void UpdatePlayer()
    {
        View.RPC("RefreshIndex", RpcTarget.AllBuffered);
        CopyPlayer();
        UpdatePlayerData();
    }
  
    
    private IEnumerator EnsureSync()
    {
      
        yield return new WaitForSeconds(0.1f);
      
    }
    #region connection
    //public override void OnDisconnected(DisconnectCause cause)
    //{


    //    HandleConnectionFail();
    //}


    //public void OnConnectionFail(DisconnectCause cause)
    //{
    //    //PhotonNetwork.LoadLevel(0);
    //    PhotonNetwork.ReconnectAndRejoin();
    //}
    public override void OnConnectedToMaster()
    {
        startButton.interactable =true;
    }
    private void HandleConnectionFail()
    {
        //StartCoroutine(ReloadSceneAfterDelay());
    }
    
    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        if (PhotonNetwork.IsMasterClient)
        {
         
            StartCounting();
        }
    }
    #endregion
    Image[] Newimage= new Image[4];
    [PunRPC]
    private void UpdatePlayerSprites()
    {
        for(int i =0;i<index;i++)
        {
            playersprit[i] = Newimage[i];
        }
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
      for(int i=0;i<4;i++)
        {
            if(otherPlayer.ActorNumber.ToString()==playersprit[i].name)
            {
                playerID = i;
                break;
            }
        }
        playersprit[playerID].color = new Vector4(0, 0, 0);
        View.RPC("updateIndexAfterplayer", RpcTarget.AllBuffered);
        View.RPC("UpdatePlayerSprites", RpcTarget.AllBuffered);
    }
    [PunRPC]
    void updateIndexAfterplayer()
    {
        index = 0; 
        for (int i = 0; i < playersprit.Length; i++)
        {
            if (playersprit[i].color.a == 1)
            {
                index++;
            }
        }
        for(int i =0;i<index;i++)
        {
            if (playersprit[i].color.a == 1)
                Newimage[i] = playersprit[i];
        }
    }
   public IEnumerator Timer()
    {
        Debug.LogWarning("Timer!!");
        int timer = 0;
        while (true)
        {
          
            if (PhotonNetwork.CurrentRoom.PlayerCount == test)
            {
                FalgeForThelobbyScene = true;
                break;
            }
            {
                float elapsedTime = timer;
               
                photonView.RPC("UpdateTime", RpcTarget.AllBuffered, elapsedTime);
                yield return new WaitForSeconds(1f);
                timer++;
            }

           
           
            if (timer >= 20)
            {
                break;
            }

         
        }

        if (FalgeForThelobbyScene)
        {
            yield return new WaitForSeconds(1f);
            PhotonNetwork.LoadLevel(1);
            StopCoroutine(Timer());
        }
        if(test!=PhotonNetwork.CurrentRoom.PlayerCount)
             View.RPC("UpdateTextAfterTime", RpcTarget.AllBuffered);
        yield return new WaitForSeconds(2f);

        View.RPC("RestartTheGame", RpcTarget.AllBuffered);
        StopCoroutine(Timer());
    }

    [PunRPC]
    void updateIndex()
    {
        index++;
    }

    [PunRPC]
    void RefreshIndex()
    {
        index--;
    }

    public void StartCounting()
    {
        if (PhotonNetwork.IsMasterClient)
        {
           
            StartCoroutine(Timer());
        }
    }

    public IEnumerator SettingPlayerData()
    {
        yield return new WaitForSeconds(1f);
        //PlayerName.text = PlayerPrefs.GetString("Player_Name");
        UpdatePlayerData();
    }

    public void UpdatePlayerData()
    {
        Player_Name.text = PlayerPrefs.GetString("Player_Name");
        Player_Image.texture = ShopManager.Instance.GetSelectedCharactersData().Icon;
        Player_Level_Slider.value = 1f - ((float)PlayerPrefs.GetInt("CurrentPlayerLevel") / (float)PlayerPrefs.GetInt("MaxLevels"));
        Player_Level_Slider.GetComponentInChildren<TMP_Text>().text = PlayerPrefs.GetInt("CurrentPlayerLevel").ToString();
    }

    [PunRPC]
    void UpdateTime(float time)
    {
        timer_text.text = "Waiting for other players " + time.ToString();
    }

    [PunRPC]
    void UpdateTextAfterTime()
    {
        //slider.gameObject.SetActive(false);
        timer_text.text = "Sorry, there are no current players.";
    }

    [PunRPC]
    void RestartTheGame()
    {

        PhotonNetwork.CurrentRoom.IsOpen = false;
        PhotonNetwork.CurrentRoom.IsVisible = false;
        PhotonNetwork.LeaveRoom();
       
        //photonView.RPC("RPC_LeaveRoom", RpcTarget.All);
        StartCoroutine(Disconnecting());

      
    }
    IEnumerator Disconnecting()
    {
        while (PhotonNetwork.InRoom)
            yield return null;
        SceneManager.LoadScene(0);
    }
    [PunRPC]
    private void RPC_LeaveRoom()
    {
        if (PhotonNetwork.InRoom)
        {
            Debug.Log("Leaving room...");
            PhotonNetwork.LeaveRoom();
        }
    }

    public override void OnLeftRoom()
    {
        
        PhotonNetwork.Disconnect();
    }
    public IEnumerator TimerForClint(float time)
    {
        while (time <= 25)
        {
            yield return new WaitForSeconds(1f);
            time++;
            if (time >= 20)
                break;
            View.RPC("UpdateTime", RpcTarget.All, time);
        }
        yield return new WaitForSeconds(2f);
        View.RPC("UpdateTextAfterTime", RpcTarget.All);
    }
}
