using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

namespace cowsins2D
{
    public class UIController : MonoBehaviour
    {
        public static InventorySlot currentInventorySlot = null, highlightedInventorySlot = null;

        [SerializeField] private WeaponController player;

        private PlayerMovement playerMovement;

        [System.Serializable]
        public enum HealthDisplayMethod
        {
            Numeric, Bar, Both
        }

        [SerializeField] private HealthDisplayMethod healthDisplayMethod;

        [SerializeField] private Image healthUI, shieldUI;

        [SerializeField] private float lerpBarValueSpeed;

        [SerializeField] private TextMeshProUGUI healthText, shieldText;

        [SerializeField] private TextMeshProUGUI bulletsUI, magazineUI;

        [SerializeField] private Image overheatUI;

        [SerializeField] private Image weaponDisplay;

        [SerializeField] private TextMeshProUGUI lowAmmoUI, ReloadUI;

        [Tooltip("Our Slider UI Object. Stamina will be shown here."), SerializeField]
        private Slider staminaSlider;

        [SerializeField] private TextMeshProUGUI coinsUI;

        [SerializeField] private Image healthVignette;

        [SerializeField] private GameObject interactUI;

        [SerializeField] private TextMeshProUGUI interactText;

        public Color hurtVignetteColor;

        public Color healVignetteColor;

        [SerializeField] private float lerpVignetteSpeed;

        public Transform inventoryContainer, hotbarContainer;

        [SerializeField] private GameObject inventorySlot;

        [SerializeField] private int inventoryLeftPadding = 0, inventoryItemsSpacing = 50;

        [SerializeField] private float inventorySlotSize = .4f;

        public Color hotbarSelected, hotbarDefault;

        [SerializeField] private Image xpImage;

        [SerializeField] private TextMeshProUGUI currentLevel, nextLevel;

        [SerializeField] private float lerpXpSpeed;


        [SerializeField] private GameObject dashIcon;

        [SerializeField] private Transform dashUIContainer;

        public delegate void AddXP();

        public static AddXP addXP;

        public delegate void UpdateWeaponInfo();

        public static UpdateWeaponInfo updateWeaponInfo;

        public delegate void UpdateHotbarSelection();

        public static UpdateHotbarSelection updateHotbarSelection;

        public delegate void OnSlotPointerUp();

        public static OnSlotPointerUp onSlotPointerUp;

        public delegate void OnUpdateHealth(float health, float shield);

        private float targetHealthValue, targetShieldValue;

        public OnUpdateHealth onUpdateHealth;

        public delegate void OnInteractAvailable(string text);

        public static OnInteractAvailable onInteractAvailable;

        public delegate void OnInteractionDisabled();

        public static OnInteractionDisabled onInteractionDisabled;

        public static UIController instance;

        private List<GameObject> dashes = new List<GameObject>();

        private void OnEnable()
        {
            // Ensure there is only 1 instance of UIController in the game
            if (instance == null) instance = this;
            else Destroy(this.gameObject);

            // Access the player
            playerMovement = player.GetComponent<PlayerMovement>();

            // Set up events
            updateWeaponInfo = UpdateWeaponInformation;

            switch (healthDisplayMethod)
            {
                case HealthDisplayMethod.Numeric:
                    onUpdateHealth = NumericHealth;
                    if (healthUI != null) Destroy(healthUI.transform.parent.gameObject);
                    if (shieldUI != null) Destroy(shieldUI.transform.parent.gameObject);
                    break;
                case HealthDisplayMethod.Bar:
                    onUpdateHealth = BarHealth;
                    if (healthText != null) Destroy(healthText.gameObject);
                    if (shieldText != null) Destroy(shieldText.gameObject);
                    break;
                case HealthDisplayMethod.Both:
                    onUpdateHealth = NumericHealth;
                    onUpdateHealth += BarHealth;
                    break;
            }

            onInteractAvailable = InteractionAvailable;
            onInteractionDisabled = InteractionDisabled;
            addXP = UpdateXP;
        }

        private void OnDisable()
        {
            onUpdateHealth = null;
        }

        private void Start()
        {
            // Reset coins
            UpdateCoins(0);

            // if the player uses stamina we do not want to display the stamina bar mid-game
            if (playerMovement.usesStamina) staminaSlider.gameObject.SetActive(false);
            updateHotbarSelection?.Invoke();

            // Set XP to the current value
            UpdateXP();

            // Initialize the procedurally generated UI for the dash
            InitializeDashUI();
        }
        private void Update()
        {
            // Check if the low ammo UI and player weapon are not null, and the current bullets are less than 1/3 of the magazine size, and the player is not reloading and has more than 0 bullets
            if (lowAmmoUI != null && player.weapon != null && player.id.currentBullets < player.weapon.magazineSize / 3 && !player.reloading && player.id.currentBullets > 0)
            {
                // Set the low ammo UI to active and the reload UI to inactive
                lowAmmoUI.gameObject.SetActive(true);
                ReloadUI.gameObject.SetActive(false);
            }

            // Check if the reload UI and player weapon are not null, and the current bullets are 0, and the weapon does not auto reload, and the player is not reloading
            if (ReloadUI != null && player.weapon != null && player.id.currentBullets <= 0 && !player.weapon.autoReload && !player.reloading)
            {
                // Set the low ammo UI to inactive and the reload UI to active
                lowAmmoUI.gameObject.SetActive(false);
                ReloadUI.gameObject.SetActive(true);
            }

            // Check if the low ammo UI, reload UI, and player weapon are not null, and the current bullets are greater than 1/3 of the magazine size, or the player weapon is null
            if (lowAmmoUI != null && ReloadUI != null && player.weapon != null && player.id.currentBullets > player.weapon.magazineSize / 3 || player.weapon == null)
            {
                // Set the low ammo UI and reload UI to inactive
                lowAmmoUI.gameObject.SetActive(false);
                ReloadUI.gameObject.SetActive(false);
            }

            // Calculate the target color for the health vignette
            Color targetColor = healthVignette.color * new Vector4(1, 1, 1, 0);

            // If the health vignette color is not the target color, lerp towards the target color
            if (healthVignette.color != targetColor) healthVignette.color = Vector4.Lerp(healthVignette.color, targetColor, Time.deltaTime * lerpVignetteSpeed);

            // Check if the health UI and shield UI are not null
            if (healthUI != null && shieldUI != null)
            {
                // Lerp the health UI fill amount towards the target health value
                healthUI.fillAmount = Mathf.Lerp(healthUI.fillAmount, targetHealthValue / playerMovement.GetComponent<PlayerStats>().MaxHealth, lerpBarValueSpeed * Time.deltaTime);

                // Lerp the shield UI fill amount towards the target shield value
                shieldUI.fillAmount = Mathf.Lerp(shieldUI.fillAmount, targetShieldValue / playerMovement.GetComponent<PlayerStats>().MaxShield, lerpBarValueSpeed * Time.deltaTime);
            }

            // Calculate the target XP value
            float targetXp = ExperienceManager.instance.GetCurrentExperience() / ExperienceManager.instance.experienceRequirements[ExperienceManager.instance.playerLevel];

            // Lerp the XP image fill amount towards the target XP value
            xpImage.fillAmount = Mathf.Lerp(xpImage.fillAmount, targetXp, lerpXpSpeed * Time.deltaTime);

            // Check if the stamina slider is not null and the player movement uses stamina
            if (staminaSlider == null || !playerMovement.usesStamina)
            {
                // Set the stamina slider object to inactive and return
                staminaSlider.gameObject.SetActive(false);
                return;
            }

            // Set the stamina slider max value to the player movement max stamina
            staminaSlider.maxValue = playerMovement.maxStamina;

            // Set the stamina slider value to the player movement stamina
            staminaSlider.value = playerMovement.stamina;
        }
        public void UpdateWeaponInformation()
        {
            // if weapon is null, remove UI Elements that represent weapon statistics.
            if (player.weapon == null)
            {
                weaponDisplay.gameObject.SetActive(false);
                bulletsUI.gameObject.SetActive(false);
                magazineUI.gameObject.SetActive(false);
                return; // Do not continue from here
            }

            // The weapon is not null, enable UI Elements that represent weapon statistics.
            weaponDisplay.gameObject.SetActive(true);
            bulletsUI.gameObject.SetActive(true);
            magazineUI.gameObject.SetActive(true);
            weaponDisplay.sprite = player.weapon.itemIcon;

            // Checks if the player's weapon is an Overheat reloading method.
            if (player.weapon.reloadingMethod == Weapon_SO.ReloadingMethod.Overheat)
            {
                bulletsUI.gameObject.SetActive(false);
                magazineUI.gameObject.SetActive(false);
                overheatUI.transform.parent.gameObject.SetActive(true);
                overheatUI.fillAmount = player.heatAmount / player.weapon.magazineSize;
                return;
            }
            // Checks if the player's weapon is a Melee shooting style.
            if (player.weapon.shootingStyle == Weapon_SO.ShootingStyle.Melee)
            {
                bulletsUI.gameObject.SetActive(false);
                magazineUI.gameObject.SetActive(false);
                return;
            }
            overheatUI.transform.parent.gameObject.SetActive(false);

            // Sets the bullets UI text to the player's current bullets, or the word "infinite" if the weapon has infinite bullets.
            bulletsUI.text = player.weapon.infiniteBullets ? "infinite" : player.id.currentBullets.ToString();

            // Sets the magazine UI text to the player's total bullets (if the weapon has limited magazines) or the weapon's magazine size (if the weapon has unlimited magazines).
            // If the weapon has infinite bullets, the magazine UI text is set to an empty string.
            magazineUI.text = player.weapon.infiniteBullets ? "" : player.weapon.limitedMagazines ? player.id.totalBullets.ToString() : player.weapon.magazineSize.ToString();
        }

        public void EnableStaminaSlider(bool condition)
        {
            staminaSlider.gameObject.SetActive(condition);
        }

        public void SetVignetteColor(Color color)
        {
            healthVignette.color = color;
        }

        public void UpdateCoins(int coins)
        {
            coinsUI.text = coins.ToString();
        }
        public List<GameObject> hotbarList = new List<GameObject>();
        public void InitializeHotBar(int hotbarSize, WeaponController controller)
        {
            // Iterate over the hotbar size.
            for (int i = 0; i < hotbarSize; i++)
            {
                GameObject slot = Instantiate(inventorySlot, hotbarContainer);
                slot.name = $"Hot Bar slot {i}";

                InventorySlot invSlot = slot.GetComponent<InventorySlot>();
                invSlot.controller = controller;
                invSlot.isHotBarSlot = true;
                invSlot.id = i;

                hotbarList.Add(slot);
            }
        }

        public List<GameObject> inventoryList = new List<GameObject>();
        public void InitializeFullInventory(int hotbarSize, int inventoryRowsAmount, int inventoryColumnsAmount, WeaponController controller)
        {
            // Iterate over the rows size.
            for (int i = 0; i < inventoryRowsAmount; i++)
            {
                // These rows contain each slot, that represent the columns of the full inventory
                GameObject rowObject = new GameObject();
                rowObject.name = $"Inventory Row {i}";
                HorizontalLayoutGroup layoutObject = rowObject.AddComponent<HorizontalLayoutGroup>();
                layoutObject.padding.left = inventoryLeftPadding;
                layoutObject.spacing = inventoryItemsSpacing;

                rowObject.transform.SetParent(inventoryContainer);
                rowObject.transform.localScale = Vector3.one;

                // Iterate over the columns size.
                for (int j = 0; j < inventoryColumnsAmount; j++)
                {
                    GameObject slot = Instantiate(inventorySlot, rowObject.transform);
                    slot.name = $"Inventory slot {i}{j}";

                    InventorySlot invSlot = slot.GetComponent<InventorySlot>();
                    invSlot.controller = controller;
                    inventoryList.Add(slot);
                    invSlot.id = inventoryList.IndexOf(slot) + hotbarSize;

                    invSlot.transform.GetChild(0).localScale = Vector3.one * inventorySlotSize;
                }
            }
        }
        // Switches the hotbar items between the current item slot and the highlighted item slot.
        // Returns if the current item is not a weapon and the highlighted item slot is a hotbar slot,
        // or if the current item slot is a hotbar slot and the highlighted item is not a weapon,
        // or if the controller is reloading and either the current item or highlighted item is a weapon.
        public void SwitchItems(WeaponController controller)
        {
            // Get the slot data for the current and highlighted item slots.
            InventorySlot.SlotData currentData = currentInventorySlot.slotData;
            InventorySlot.SlotData highlightData = highlightedInventorySlot.slotData;

            // Check if either item is not a weapon or the controller is reloading.
            if (!(currentData.inventoryItem is Weapon_SO) && highlightedInventorySlot.isHotBarSlot
                || currentInventorySlot.isHotBarSlot && highlightData.inventoryItem != null && !(highlightData.inventoryItem is Weapon_SO)
                || controller.reloading && (currentData.inventoryItem is Weapon_SO || highlightData.inventoryItem is Weapon_SO))
            {
                // Clear the current and highlighted item slots, hide the tooltip, and return.
                currentInventorySlot = null;
                highlightedInventorySlot = null;
                Tooltip.Instance.Hide();
                return;
            }

            // Swap the slot data between the current and highlighted item slots.
            highlightedInventorySlot.slotData = currentData;
            currentInventorySlot.slotData = highlightData;

            // Disable the controller's game object if it has one.
            if (controller.id != null) controller.id.gameObject.SetActive(false);


            onSlotPointerUp?.Invoke();


            // Clear the current and highlighted item slots, and hide the tooltip.
            currentInventorySlot = null;
            highlightedInventorySlot = null;

            Tooltip.Instance.Hide();
        }
        // Returns true if the inventory is full, false otherwise.
        public bool IsInventoryFull()
        {
            // Iterate over the inventory list.
            foreach (var slot in inventoryList)
            {
                // If the inventory slot is empty, return false.
                if (slot.GetComponent<InventorySlot>().slotData.inventoryItem == null) return false;
            }

            return true;
        }

        public void PopulateInventory(Item_SO item, int amount)
        {
            // Iterate over the inventory to populate it with a provided item.
            foreach (var slot in inventoryList)
            {
                InventorySlot _slot = slot.GetComponent<InventorySlot>();
                if (_slot.slotData.inventoryItem == null)
                {
                    _slot.slotData.inventoryItem = item;
                    _slot.slotData.amount = amount;
                    _slot.UpdateGraphics();
                    break;
                }
            }
        }

        public void PopulateArray(List<GameObject> list, Item_SO item, int amount)
        {
            foreach (var slot in list)
            {
                InventorySlot _slot = slot.GetComponent<InventorySlot>();
                if (_slot.slotData.inventoryItem == null)
                {
                    _slot.slotData.inventoryItem = item;
                    _slot.slotData.amount = amount;
                    _slot.UpdateGraphics();
                    break;
                }
            }
        }

        public void PopulateInventoryWithWeapon(Item_SO item, int amount, GameObject save_Weapon)
        {
            // Iterate over the inventory to populate it with a provided weapon.
            foreach (var slot in inventoryList)
            {
                InventorySlot _slot = slot.GetComponent<InventorySlot>();
                if (_slot.slotData.inventoryItem == null)
                {
                    _slot.slotData.inventoryItem = item;
                    _slot.slotData.amount = amount;
                    _slot.slotData.saveWeapon = save_Weapon;
                    _slot.UpdateGraphics();
                    break;
                }
            }
        }

        public void PopulateArrayWithWeapon(List<GameObject> list, Item_SO item, int amount, GameObject save_Weapon)
        {
            foreach (var slot in list)
            {
                InventorySlot _slot = slot.GetComponent<InventorySlot>();
                if (_slot.slotData.inventoryItem == null)
                {
                    _slot.slotData.inventoryItem = item;
                    _slot.slotData.amount = amount;
                    _slot.slotData.saveWeapon = save_Weapon;
                    _slot.UpdateGraphics();
                    break;
                }
            }
        }

        public int CheckForBullets(AmmoType_SO type)
        {
            // Auxiliar variable that will serve as an ammo counter
            int totalAmount = 0;

            foreach (var slot in inventoryList)
            {
                InventorySlot _slot = slot.GetComponent<InventorySlot>();

                if (_slot.slotData.inventoryItem == null || !(_slot.slotData.inventoryItem is AmmoType_SO) | _slot.slotData.inventoryItem != type)
                {
                    continue;
                }
                // Add to the total amount if it matches the ammo type in the inventory.
                totalAmount += _slot.slotData.amount;
            }

            return totalAmount;
        }

        public void ReduceInventoryAmount(Item_SO type, int amount)
        {
            // Iterate over the inventory list.
            foreach (var slot in inventoryList)
            {
                // Get the InventorySlot component from the inventory slot.
                InventorySlot _slot = slot.GetComponent<InventorySlot>();

                // Check if the inventory slot is empty or if the item in the slot is not the given item type.
                if (_slot.slotData.inventoryItem == null || !(_slot.slotData.inventoryItem is AmmoType_SO) || _slot.slotData.inventoryItem != type)
                {
                    continue;
                }

                // If the inventory slot contains enough of the item type, reduce the amount by the given amount and break out of the loop.
                if (_slot.slotData.amount >= amount)
                {
                    _slot.slotData.amount -= amount;
                    break;  // Reduction completed, exit the loop
                }
                // Otherwise, reduce the amount by the amount in the inventory slot and set the amount in the inventory slot to 0.
                else
                {
                    amount -= _slot.slotData.amount;
                    _slot.slotData.amount = 0;
                }
            }
        }


        private void NumericHealth(float health, float shield)
        {
            healthText.text = health.ToString("F0");
            shieldText.text = shield.ToString("F0");
        }
        private void BarHealth(float health, float shield)
        {
            targetHealthValue = health;
            targetShieldValue = shield;
        }
        public void InteractionAvailable(string message)
        {
            interactUI.SetActive(true);
            interactText.text = message;
        }
        public void InteractionDisabled()
        {
            interactUI.SetActive(false);
        }

        private void UpdateXP()
        {
            currentLevel.text = (ExperienceManager.instance.playerLevel + 1).ToString();
            nextLevel.text = (ExperienceManager.instance.playerLevel + 2).ToString();
        }

        public void RemoveDash()
        {
            var obj = dashes[dashes.Count - 1].gameObject;
            dashes.Remove(obj);
            Destroy(obj);
        }

        public void GainDash()
        {
            var obj = Instantiate(dashIcon, dashUIContainer);
            dashes.Add(obj);
        }

        private void InitializeDashUI()
        {
            for (int i = 0; i < playerMovement.AmountOfDashes; i++)
            {
                var obj = Instantiate(dashIcon, dashUIContainer);
                dashes.Add(obj);
            }
        }

    }

}