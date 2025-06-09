using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public int TargetRate = 120;
    public int MaxLevels = 100;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
            Destroy(gameObject);

        Application.targetFrameRate = TargetRate;
    }

    private void Start()
    {
        PlayerPrefs.SetInt("MaxLevels", MaxLevels);
        PlayerPrefs.SetInt("CurrentPlayerLevel", MaxLevels/2); //TODO: for testing should change using code
    }
}
