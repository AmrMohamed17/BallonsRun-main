using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

namespace cowsins2D
{
    public class InventorySlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
    {
        [System.Serializable]
        public class SlotData
        {
            public Item_SO inventoryItem;
            public int amount;
            public GameObject saveWeapon;
        }

        [HideInInspector] public SlotData slotData = new SlotData();

        [SerializeField] private Image image, background;

        [SerializeField, Tooltip("TMPro texts that displays the number of items the player currently has on this slot.")] private TextMeshProUGUI amountText;

        [SerializeField, Tooltip("Scale of the slot when hovered.")] private float hoverSize, hoverSpeed;

        [SerializeField, Tooltip("Sound to display on hover.")] private AudioClip hoverSFX;

        private Vector3 initialSize;

        [HideInInspector] public WeaponController controller;

        public int id;

        [HideInInspector] public bool isHotBarSlot = false;

        private Vector3 targetScale;

        private void OnEnable()
        {
            UIController.updateWeaponInfo += UpdateWeaponInfo;
            UIController.updateHotbarSelection += UpdateSelection;

            UIController.updateWeaponInfo?.Invoke();
        }
        private void OnDisable()
        {
            UIController.updateWeaponInfo -= UpdateWeaponInfo;
            UIController.updateHotbarSelection -= UpdateSelection;
        }
        private void Awake()
        {
            initialSize = transform.localScale;
            targetScale = initialSize;
            UpdateGraphics();
        }

        private void Update()
        {
            transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * hoverSpeed);
        }
        public void OnPointerEnter(PointerEventData eventData)
        {
            targetScale = Vector3.one * hoverSize;
            SoundManager.Instance.PlaySound(hoverSFX, .25f);
            UIController.highlightedInventorySlot = this.GetComponent<InventorySlot>();
            if (slotData.inventoryItem == null || UIController.highlightedInventorySlot.slotData.inventoryItem != null && UIController.currentInventorySlot != null) return;
            Tooltip.Instance.Show(slotData.inventoryItem.itemName);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            targetScale = initialSize;

            UIController.highlightedInventorySlot = null;
            Tooltip.Instance.Hide();
        }
        public void OnPointerDown(PointerEventData eventData)
        {
            if (slotData.inventoryItem == null) return;
            if (eventData.button == PointerEventData.InputButton.Right)
            {
                OnUse();
                return;
            }

            if (eventData.button == PointerEventData.InputButton.Middle)
            {
                OnDrop();
                return;
            }
            OnSelect();
        }
        public void OnUse()
        {
            if (slotData.inventoryItem == null) return;
            if (slotData.inventoryItem.Use(controller))
            {
                slotData.amount--;
                UIController.updateWeaponInfo?.Invoke();
            }
        }
        public void OnDrop()
        {
            if (slotData.inventoryItem == null) return;
            controller.GetComponent<InteractionManager>().DropInventoryItem(slotData);
            Destroy(slotData.saveWeapon);
            slotData = new SlotData()
            {
                inventoryItem = null,
                amount = 0,
                saveWeapon = null
            };
            controller.CheckForBullets();
            UIController.updateWeaponInfo?.Invoke();
            UIController.updateHotbarSelection?.Invoke();
        }
        public void OnSelect()
        {
            if (slotData.inventoryItem == null) return;
            UIController.currentInventorySlot = this.GetComponent<InventorySlot>();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (UIController.currentInventorySlot == null || UIController.highlightedInventorySlot == null ||
                UIController.currentInventorySlot.gameObject == UIController.highlightedInventorySlot.gameObject)
            {
                Tooltip.Instance.Hide();
                UIController.currentInventorySlot = null;
                return;
            }


            UIController.instance.SwitchItems(controller);

            UIController.updateHotbarSelection?.Invoke();
            UIController.updateWeaponInfo?.Invoke();
            if (UIController.instance.hotbarList[controller.currentWeapon].GetComponent<InventorySlot>().slotData.saveWeapon != null)
            {
                controller.inventory[controller.currentWeapon].gameObject.SetActive(false);
                controller.inventory[controller.currentWeapon] = UIController.instance.hotbarList[controller.currentWeapon].GetComponent<InventorySlot>().slotData.saveWeapon.GetComponent<WeaponIdentification>();
            }
            controller.UnholsterWeapon();
            UIController.updateHotbarSelection?.Invoke();
        }


        private void UpdateWeaponInfo()
        {
            UpdateGraphics();
        }

        private void UpdateSelection()
        {
            if (isHotBarSlot)
                background.color = controller.currentWeapon == id ? UIController.instance.hotbarSelected : UIController.instance.hotbarDefault;
        }
        public void UpdateGraphics()
        {
            if (slotData.amount <= 0)
            {
                slotData = new SlotData()
                {
                    inventoryItem = null,
                    amount = 0,
                    saveWeapon = null
                };
            }
            image.sprite = slotData.inventoryItem == null ? null : slotData.inventoryItem.itemIcon;
            amountText.text = slotData.amount > 1 && slotData.inventoryItem != null ? slotData.amount.ToString() : "";
        }
    }

}