using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;

public class UiManager : MonoBehaviour
{
    // UiManager Singleton
    public static UiManager Instance;
    private void Awake()
    {
        Instance = this;
    }

    public void OpenMenu()
    {
        // Open Menu Logic
        /*
        Omar Code
        */
        // Button SFX
        GlobalAudioPlayer.PlaySFX("ButtonSFX");
    }

    public void LoadGame()
    {
        SceneManager.LoadScene(1);
    }
}
