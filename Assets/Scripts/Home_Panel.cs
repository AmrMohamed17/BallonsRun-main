using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Home_Panel : MonoBehaviour
{

    public CanvasGroup SelectCharacterCanvasGroup;
    public RectTransform SelectCharacterRectTransform;

    public bool OpenCharacterSelectionScreen()
    {
        if(PlayerPrefs.HasKey("SelectedPlayer"))
        {
            ShopManager.Instance.SelectCharacter(PlayerPrefs.GetString("SelectedPlayer"));
            return true;
        }

        return false;
    }
}
