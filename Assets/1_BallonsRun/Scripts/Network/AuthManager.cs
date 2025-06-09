using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Authentication;
using System.Threading.Tasks;
using TMPro;
using Photon.Pun;
public class AuthManager : MonoBehaviour
{
    // Start is called before the first frame update
    //public static AuthManager Instance { get; private set; }

    public string Player_Name="";
    public int Playernumber = 0;
    //private void Awake()
    //{
    //    if (Instance == null)
    //    {
    //        Instance = this;
    //        DontDestroyOnLoad(gameObject);
    //    }
    //    //else
    //    //{
    //    //    Destroy(gameObject);
    //    //}
    //}
    async void Start()
    {
        await UnityServices.InitializeAsync();
        StartCoroutine(PlayerName());
    }

    async void SignIn()
    {
        await SignInAnonymous();

    }
    async Task SignInAnonymous()
    {
        if (!AuthenticationService.Instance.IsSignedIn)
        {
            try
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
                Debug.LogWarning("Player ID " + AuthenticationService.Instance.PlayerId);
                Player_Name = AuthenticationService.Instance.PlayerId;
                PhotonNetwork.NickName = Player_Name;
                PlayerPrefs.SetString("Player_Name", Player_Name);
              
            }
            catch (AuthenticationException ex)
            {
                Debug.Log("Sign In Failed" + ex);
            }
        }
    }
    public IEnumerator PlayerName()
    {

        SignIn();
        yield return null;
    }

}
