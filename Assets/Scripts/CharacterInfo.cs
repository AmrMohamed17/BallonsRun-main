using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CharacterInfo : MonoBehaviour
{
    [Header("Characters Scroll")]
    [SerializeField]
    private RectTransform characterSliderContentParent;
    [SerializeField]
    private RectTransform characterSliderViewport;
    [SerializeField]
    private ScrollRect characterScrollRect;
    [SerializeField]
    private GameObject characterPrefab;
    [SerializeField]
    private float fadeDistance = 200f;

    [Header("Preview")]
    [SerializeField]
    private RawImage selectedCharacterPreview;
    [SerializeField]
    private TMP_Text selectedCharacterName;
    [SerializeField]
    private Slider selectedCharacterSpeed_Slider;
    [SerializeField]
    private Slider selectedCharacterJump_Slider;
    [SerializeField]
    private Slider selectedCharacterHP_Slider;

    // snapping
    public float snapSpeed = 10f;
    public float snapDelay = 0.1f;
    public float dragThreshold = 10f;


    private bool isDragging = false;
    private Vector2 mouseStartPos;
    private Vector2 currentMousePos;

    private Coroutine snapCoroutine;

    void Start()
    {
        LoadCharacters();
        UpdateCharacterImage(ShopManager.Instance.GetSelectedCharactersData());
    }

    void LoadCharacters()
    {
        CharacterItem[] characterItems = ShopManager.Instance.GetCharactersData();

        if (characterItems.Length == 0) { return; }

        VerticalLayoutGroup layoutGroup = characterSliderContentParent.GetComponent<VerticalLayoutGroup>();
        float halfViewportHeight = characterSliderViewport.rect.height / 2f;

        layoutGroup.padding.top = Mathf.RoundToInt(halfViewportHeight);
        layoutGroup.padding.bottom = Mathf.RoundToInt(halfViewportHeight);
        LayoutRebuilder.ForceRebuildLayoutImmediate(characterSliderContentParent);

        foreach (CharacterItem characterItem in characterItems)
        {
            GameObject newItem = Instantiate(characterPrefab, characterSliderContentParent);
            newItem.GetComponent<RawImage>().texture = characterItem.Icon;
            newItem.GetComponentInChildren<TMP_Text>().text = characterItem.Name;

            Button btn = newItem.GetComponent<Button>();
            if (btn != null)
            {
                btn.onClick.AddListener(() => UpdateCharacterImage(characterItem));
            }
        }
    }

    void Update()
    {
        if (!IsMouseDragging() && snapCoroutine == null)
        {
            snapCoroutine = StartCoroutine(SnapToClosestItem());
        }

        UpdateCharactersFading();
    }

    void UpdateCharacterImage(CharacterItem NewCharacter)
    {
        selectedCharacterPreview.texture = NewCharacter.Texture; // check animations

        selectedCharacterName.text = NewCharacter.Name;
        // update sliders
        selectedCharacterSpeed_Slider.value = 1f - ((float)NewCharacter.Speed / 10f);
        selectedCharacterJump_Slider.value = 1f - ((float)NewCharacter.Jump / 10f);
        selectedCharacterHP_Slider.value = 1f - ((float)NewCharacter.HP / 100f);

        // update shop
        ShopManager.Instance.ChangeCharacter(NewCharacter);

        // animation
         selectedCharacterPreview.GetComponent<Animator>().runtimeAnimatorController = NewCharacter.AnimatorController;
    }

    void UpdateCharactersFading()
    {
        foreach (Transform child in characterSliderContentParent)
        {
            CanvasGroup cg = child.GetComponent<CanvasGroup>();
            if (cg == null) continue;

            Vector3 worldPos = child.position;
            Vector3 localPos = characterSliderViewport.InverseTransformPoint(worldPos);

            float distanceFromCenter = Mathf.Abs(localPos.y);

            float alpha = Mathf.Clamp01(1f - (distanceFromCenter / fadeDistance));
            cg.alpha = alpha;
            cg.interactable = cg.alpha > 0.9f;
        }
    }

    IEnumerator SnapToClosestItem()
    {
        yield return new WaitForSeconds(snapDelay);

        float closestDistance = float.MaxValue;
        RectTransform closestItem = null;

        foreach (Transform child in characterSliderContentParent)
        {
            float distance = Mathf.Abs(characterSliderViewport.InverseTransformPoint(child.position).y);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestItem = child as RectTransform;
            }
        }

        if (closestItem != null)
        {
            Vector2 viewportCenter = characterSliderViewport.rect.center;
            Vector3 worldCenter = characterSliderViewport.TransformPoint(viewportCenter);

            Vector3 difference = worldCenter - closestItem.position;
            Vector2 newAnchoredPos = characterSliderContentParent.anchoredPosition + new Vector2(0, difference.y);

            // Smooth snapping
            float elapsed = 0f;
            Vector2 startPos = characterSliderContentParent.anchoredPosition;

            while (elapsed < 1f)
            {
                elapsed += Time.deltaTime * snapSpeed;
                characterSliderContentParent.anchoredPosition = Vector2.Lerp(startPos, newAnchoredPos, Mathf.SmoothStep(0f, 1f, elapsed));
                yield return null;
            }

            characterSliderContentParent.anchoredPosition = newAnchoredPos;
        }

        snapCoroutine = null;
    }

    bool IsMouseDragging()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (RectTransformUtility.RectangleContainsScreenPoint(characterSliderContentParent, Input.mousePosition))
            {
                mouseStartPos = Input.mousePosition;
                return false;
            }
        }

        if (Input.GetMouseButton(0))
        {
            if (RectTransformUtility.RectangleContainsScreenPoint(characterSliderContentParent, Input.mousePosition))
            {
                currentMousePos = Input.mousePosition;
                float distance = Vector2.Distance(mouseStartPos, currentMousePos);

                if (distance > dragThreshold)
                {
                    return true;
                }
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            return false;
        }

        return false;
    }
}
