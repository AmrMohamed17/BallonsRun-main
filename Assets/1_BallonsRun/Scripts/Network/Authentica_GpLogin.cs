//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using GooglePlayGames;
//using GooglePlayGames.BasicApi;
//public class AuthenticarionLogin : MonoBehaviour
//{
//    // Start is called before the first frame update
//    public string PlayerName;
//    void Start()
//    {
//        //SighnIn();
//    }

//    // Update is called once per frame
//   public void SighnIn()
//    {
//        PlayGamesPlatform.Instance.Authenticate(ProcessAuthentication);
//    }
//    internal void ProcessAuthentication(SignInStatus status)
//    {
//        if (status == SignInStatus.Success)
//        {
//            PlayerName = PlayGamesPlatform.Instance.GetUserDisplayName();
//        }
//        else
//        {
//            // Disable your integration with Play Games Services or show a login button
//            // to ask users to sign-in. Clicking it should call
//            // PlayGamesPlatform.Instance.ManuallyAuthenticate(ProcessAuthentication).
//        }
//    }
//}
