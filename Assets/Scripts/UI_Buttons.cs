using UnityEngine;
using DG.Tweening;

public class UI_Buttons : MonoBehaviour
{
    public float fadeTime = 1f;
    public CanvasGroup canvasGroup;
    public RectTransform rectTransform;

    public void PanelFadeIn()
    {
        Home_Panel homePanel = GetComponent<Home_Panel>();
        if (homePanel && homePanel.OpenCharacterSelectionScreen() == false)
        {
            canvasGroup = homePanel.SelectCharacterCanvasGroup;
            rectTransform = homePanel.SelectCharacterRectTransform;
        }

        canvasGroup.alpha = 0;
        rectTransform.transform.localPosition = new Vector3(0f, -1000f, 0f);
        rectTransform.DOAnchorPos(new Vector2(0f, 0f), fadeTime, false).SetEase(Ease.InOutExpo);
        canvasGroup.DOFade(1, fadeTime);
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
    }

    public void PanelFadeout()
    {
        canvasGroup.alpha = 1;
        rectTransform.transform.localPosition = new Vector3(0f, 0f, 0f);
        rectTransform.DOAnchorPos(new Vector2(0f, -1000f), fadeTime, false).SetEase(Ease.InOutQuint);
        canvasGroup.DOFade(0, fadeTime);
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }
}