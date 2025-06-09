using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using TMPro;
using System;

public class ShopManager : MonoBehaviour
{
    // Singleton
    public static ShopManager Instance { get; private set; }
    private void Awake()
    {
        Instance = this;
    }

    // Store References
    [SerializeField] private CharacterItem[] Characters;
    [SerializeField] private GameObject[] ShopPlayers;
    private CharacterItem Char;
    public int index;

    // Store UI
    [SerializeField] private GameObject LockPanel;
    [SerializeField] private TextMeshProUGUI CharNameTxt;
    [SerializeField] private TextMeshProUGUI CharCoinTxt;
    //[SerializeField] private TextMeshProUGUI SpeedTxt;
    //[SerializeField] private TextMeshProUGUI JumpTxt;
    //[SerializeField] private TextMeshProUGUI HpTxt;
    [SerializeField] private Button BuyButton;
    [SerializeField] private Button SelectButton;
    [HideInInspector]
    public string Selected_Player;
    private void Start()
    {
        AssignPlayers();
        UpdateUI();
    }

    private void AssignPlayers()
    {
        // Assing Character Unlock
        foreach (CharacterItem character in Characters)
        {
            if (character.Price == 0)
            {
                character.Unlocked = true;
                Selected_Player = character.Name;
                PlayerPrefs.SetString("CurrentPlayerPreph", Selected_Player);
            }
            else
            {
                if (PlayerPrefs.GetInt(character.Name, 0) == 1)
                {
                    character.Unlocked = true;
                    Selected_Player = character.Name;
                    //Selected_Player = character.Name;
                    PlayerPrefs.SetString("CurrentPlayerPreph", Selected_Player);
                }
                else
                {
                    character.Unlocked = false;
                }
            }
        }

        ActiveCharacter();
    }

    private void ActiveCharacter()
    {
        // Get Current Player
        string selectedChar = PlayerPrefs.GetString("SelectedPlayer");

        // Find index of the character with the same name
        index = Array.FindIndex(Characters, character => character.Name == selectedChar);

        // character not found
        if (index == -1)
        {
            index = 0;
            PlayerPrefs.SetString("SelectedPlayer", Characters[index].Name);
            Selected_Player = Characters[index].Name;
        }

        Char = Characters[index];
        Selected_Player = Char.Name;
        // Deactivate all shop players
        foreach (GameObject fighter in ShopPlayers)
        {
            fighter.SetActive(false);
        }

        // Activate selected character
        ShopPlayers[index].SetActive(true);
    }

    //public void NextCharacter()
    //{
    //    ShopPlayers[this.index].SetActive(false);

    //    index++;
    //    if (index >= Characters.Length)
    //    {
    //        index = 0;
    //    }
    //    Char = Characters[index];

    //    ShopPlayers[index].SetActive(true);

    //    // Player Voice Sound
    //    ShopPlayers[index].GetComponent<AudioSource>().Play();

    //    UpdateUI();
    //}
    //public void PreviousCharacter()
    //{
    //    ShopPlayers[this.index].SetActive(false);

    //    index--;
    //    if (index < 0)
    //    {
    //        index = Characters.Length - 1;
    //    }

    //    Char = Characters[index];
    //    ShopPlayers[index].SetActive(true);

    //    // Player Voice Sound
    //    ShopPlayers[index].GetComponent<AudioSource>().Play();

    //    UpdateUI();
    //}
    public void SelectCharacter()
    {
        if (Char.Unlocked)
        {
            PlayerPrefs.SetString("SelectedPlayer", Char.Name);
            SelectButton.interactable = false;
            Selected_Player = Char.Name;
        }
        PlayerPrefs.SetString("CurrentPlayerPreph", Char.Name);
    }

    public void ChangeCharacter(CharacterItem NewSelectedCharacter)
    {
        Char = NewSelectedCharacter;
        UpdateUI();
    }

    public void SelectCharacter(string selectedCharacterName)
    {
        Selected_Player = selectedCharacterName;
        PlayerPrefs.SetString("SelectedPlayer", selectedCharacterName);
    }

    public void BuyCharacter()
    {
        Char.Unlocked = true;
        PlayerPrefs.SetInt(Char.Name, 1);
        PlayerPrefs.SetString("SelectedPlayer", Char.Name);
        CurrencyManager.Instance.AddValue(-Char.Price);
    }

    public void UpdateUI()
    {
        // Unlock Panels
        int index = Characters.Length;
        for (int i = 0; i < index; i++)
        {
            // if (Characters[i].Unlocked)
            // {
            //     UnlockPanels[i].SetActive(false);
            // }
            // else
            // {
            //     UnlockPanels[i].SetActive(true);
            // }
        }

        // Buy Button
        string UnlockedCharacter = PlayerPrefs.GetString("SelectedPlayer");
        Selected_Player = UnlockedCharacter;
        if (Char.Unlocked)
        {
            //BuyButton.gameObject.SetActive(false);
            SelectButton.gameObject.SetActive(true);

            // Update Lock Panel
            LockPanel.SetActive(false);

            if (Char.Name == UnlockedCharacter)
                SelectButton.interactable = false;
            else
                SelectButton.interactable = true;
        }
        else
        {
            // Update Shop Buttons
            //BuyButton.gameObject.SetActive(true);
            SelectButton.gameObject.SetActive(true);

            // Character Coins Text
            CharCoinTxt.text = Char.Price + "$";

            // Update Lock Panel
            LockPanel.SetActive(true);

            if (Char.Price > PlayerPrefs.GetInt("Coins"))
            {
                //BuyButton.interactable = false;
            }
            else
            {
                //BuyButton.interactable = true;
            }
        }

        // Display Player Data
        CharNameTxt.text = Char.Name.ToString();
        //SpeedTxt.text = Char.Speed.ToString();
        //JumpTxt.text = Char.Jump.ToString();
        //HpTxt.text = Char.HP.ToString();
    }

    public void QuitShop()
    {
        string SelectedPlayer = PlayerPrefs.GetString("SelectedPlayer");
        PlayerPrefs.SetString("CurrentPlayerPreph", SelectedPlayer);
        Selected_Player = SelectedPlayer;
        if (Char.Name != SelectedPlayer)
        {
            ActiveCharacter();
            UpdateUI();
        }
    }

    public CharacterItem[] GetCharactersData()
    {
        return Characters;
    }

    public CharacterItem GetSelectedCharactersData()
    {
        return Array.Find<CharacterItem>(Characters, character => character.Name == PlayerPrefs.GetString("SelectedPlayer"));
    }
}
