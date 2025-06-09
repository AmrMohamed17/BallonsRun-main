using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    private void Awake()
    {
        SpawnCharacter();
    }

    private void SpawnCharacter()
    {
        string PlayerName = PlayerPrefs.GetString("SelectedPlayer");
        GameObject SelectedPlayer = Resources.Load(PlayerName) as GameObject;
        GameObject Player = Instantiate(SelectedPlayer, transform);

        if (Player != null)
        {
            Debug.Log("Player Spawned Success! " + Player.gameObject.name);
        }
        else
        {
            Debug.LogError("Player Not Found");
        }
    }
}
