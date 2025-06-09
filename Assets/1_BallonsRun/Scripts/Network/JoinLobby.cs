using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class JoinLobby : MonoBehaviour
{
    // Start is called before the first frame update
   

    public void JoinLobbyScene()
    {
        SceneManager.LoadScene("Lobby");
    }
}
