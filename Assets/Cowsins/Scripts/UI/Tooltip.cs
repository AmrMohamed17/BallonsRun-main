using UnityEngine;
using UnityEngine.UI; 
using TMPro;
namespace cowsins2D
{
    public class Tooltip : MonoBehaviour
    {
        [SerializeField, Tooltip("Image that displays the dragged icon.")] private Image itemIconDisplay;
        [SerializeField, Tooltip("Text that displays the item name on hover.")] private TMP_Text textComponent;
        [SerializeField, Tooltip("Background.")] private Image imageComponent;
        [SerializeField, Tooltip("Margin for the background to give space to the text.")] private float horizontalMarginSize, verticalMarginSize;

        public static Tooltip Instance;

        private void Awake()
        {
            if (Tooltip.Instance == null) Tooltip.Instance = this;
            else Destroy(this.gameObject);

            Hide();
        }

        private void Update()
        {
            ScaleImageToFitText();
            FollowCursor();
            UpdateInventorySlotGraphics();
        }

        private void ScaleImageToFitText()
        {
            float textWidth = textComponent.preferredWidth;
            float textHeight = textComponent.preferredHeight;

            Vector2 imageSize = new Vector2(textWidth + horizontalMarginSize, textHeight + verticalMarginSize);
            imageComponent.rectTransform.sizeDelta = imageSize;
        }

        private void FollowCursor()
        {
            Vector2 mousePosition = DeviceDetection.Instance.mode == DeviceDetection.InputMode.Controller
                ?
                VirtualCursor.Instance.transform.position :
                UnityEngine.InputSystem.Mouse.current.position.ReadValue();
            mousePosition = new Vector2(mousePosition.x + imageComponent.rectTransform.sizeDelta.x + horizontalMarginSize, mousePosition.y + imageComponent.rectTransform.sizeDelta.y + verticalMarginSize);
            transform.position = mousePosition;
        }

        private void UpdateInventorySlotGraphics()
        {
            if (UIController.currentInventorySlot == null)
            {
                itemIconDisplay.gameObject.SetActive(false);
                return;
            }
            itemIconDisplay.gameObject.SetActive(true);
            itemIconDisplay.sprite = UIController.currentInventorySlot.slotData.inventoryItem == null ? null : UIController.currentInventorySlot.slotData.inventoryItem.itemIcon;
        }
        public void Show(string text)
        {
            imageComponent.gameObject.SetActive(true);
            textComponent.text = text;
        }

        public void Hide()
        {
            imageComponent.gameObject.SetActive(false);
        }
    }

}