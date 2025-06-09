using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;
using System.Linq;

public class RaceManager : MonoBehaviourPunCallbacks
{

    public static RaceManager Instance;
     Transform finishLine; 
   public List<PlayerData> finishers = new List<PlayerData>();
    PhotonView view;
    public bool raceFinished = false;
    bool playerIsMine = false;
    GameObject myPlayer;
    Animator anim;
    int test =2;
    int count = 0;
    AuthManager auth;
    int index=1;
    PhotonView photonView;
    Player[] players;
    [HideInInspector]
   public List<string> Names= new List<string>();
    [HideInInspector]
   public  List<float> FinishTime= new List<float>();
    double timeWheneSceneStart;
    private List<PlayerScore> playerScores = new List<PlayerScore>();
    int score = 0;
    float Timer=0;
    bool RaceEnding=false;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    private void Start()
    {
       
        timeWheneSceneStart = Time.timeSinceLevelLoadAsDouble;
        players = PhotonNetwork.PlayerList;
        photonView = GetComponent<PhotonView>();
        StartCoroutine(RaceTime());
    }
    private void Update()
    {
        if (finishers.Count == PhotonNetwork.CurrentRoom.PlayerCount && !raceFinished)
        {
            raceFinished = true;
            AnnounceWinners();
        }
    }

    public void PlayerReachedFinish(GameObject player)
    {
        if (finishers.Count <= 4)
        {
            //double finishTime = PhotonNetwork.Time - timeWheneSceneStart;
            finishers.Add(new PlayerData(player, Timer));
            count++;
        }
        
    }

    private void AnnounceWinners()
    {
        RaceEnding = true;
        StopCoroutine(RaceTime());
   
        finishers.Sort((a, b) => a.FinishTime.CompareTo(b.FinishTime));
        for (int i = 0; i < finishers.Count; i++)
        {
            PlayerPrefs.SetString("PlayerTime" + (i + 1).ToString(), finishers[i].FinishTime.ToString());
            //photonView = finishers[i].Player.GetComponent<PhotonView>();
            //PlayerPrefs.SetString("PlayerAuthName" + i.ToString(), photonView.Owner.NickName);
            //Debug.LogWarning("PLayer__" + PhotonNetwork.NickName);


        }
            Debug.Log("Winners:");
        for (int i = 0; i < finishers.Count; i++)
        {
            Debug.LogWarning($"{i + 1}: {finishers[i].Player.name} - Time: {finishers[i].FinishTime:F2} seconds");
            PlayerPrefs.SetString("Player" + (i + 1).ToString(), finishers[i].Player.name);
            Debug.LogWarning(PlayerPrefs.GetString("Player" + (i + 1).ToString()));
        }
        for(int i =0;i< count; i++)
        {
            view = finishers[i].Player.GetComponent<PhotonView>();
            if (view.IsMine)
            {
                playerIsMine = true;
                myPlayer = finishers[i].Player;
            }
        }
        int x = 0;
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            playerScores.Add(new PlayerScore(player.NickName, finishers[x].FinishTime));
            x++;
        }
        playerScores = playerScores.OrderByDescending(p => p.score).ToList();
       for(int i =0;i<finishers.Count;i++)
        {
            Names.Add(playerScores[i].playerName);
           FinishTime.Add(finishers[i].FinishTime);
            PlayerPrefs.SetString("PlayerAuthName" + (i + 1).ToString(), playerScores[i].playerName);
        }
        if (playerIsMine)
        {
            //PhotonNetwork.LoadLevel(3);
            SceneManager.LoadScene(3);
        }
      
        else
        {
            anim = myPlayer.GetComponentInChildren<Animator>();
            anim.SetTrigger("lose");
            Invoke("ReloadScene", 3f);
        }
    }
    public void ReloadScene()
    {
        PhotonNetwork.LeaveRoom();
      
    }
    public override void OnLeftRoom()
    {

        //PhotonNetwork.LoadLevel(0);
        SceneManager.LoadScene(0);
    }
    public  IEnumerator RaceTime()
    {
        while(!RaceEnding)
        {
        yield return new WaitForSeconds(1f);
            Timer++;

        }
    }
    private void OnTriggerEnter2D  (Collider2D other)
    {
        if (other.CompareTag("Player") && !finishers.Exists(f => f.Player == other.gameObject))
        {
            PlayerReachedFinish(other.gameObject);

           
        }
    }
  
    public class PlayerData
    {
        public GameObject Player { get; private set; }
        public float FinishTime { get; private set; }

        public PlayerData(GameObject player,float finishTime)
        {
            Player = player;
            FinishTime = finishTime;
        }
    }
    public class PlayerScore
    {
        public string playerName { get; private set; }
        public double score { get; private set; }

        public PlayerScore(string name, double playerScore)
        {
            playerName = name;
            score = playerScore;
        }
    }
}
