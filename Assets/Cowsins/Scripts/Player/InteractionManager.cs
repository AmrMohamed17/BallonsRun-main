using UnityEngine;
using UnityEngine.Events;
namespace cowsins2D
{
    public class InteractionManager : MonoBehaviour
    {
        [System.Serializable]
        public enum InventoryMethod
        {
            HotbarOnly, Full
        }

        public InventoryMethod inventoryMethod;

        [Min(1)] public int hotbarSize;

        [SerializeField, Tooltip("Reference to the inventory object.")] private GameObject inventory;

        [SerializeField, Tooltip("Number of rows and columns, the total number of items is rows*columns.")] private int inventoryRowsAmount, inventoryColumnsAmount;

        [SerializeField, Tooltip("Sounds played on opening & closing the inventory. ")] private AudioClip openInventorySFX, closeInventorySFX;

        [SerializeField, Tooltip("Sounds played on picking up an item. ")] private AudioClip pickUpItemSFX;

        [SerializeField, Tooltip("Time in seconds to hold the interaction key to successfully perform an interaction. ")] private float timeToInteract;

        public float TimeToInteract
        {
            get { return timeToInteract; }
        }

        [Tooltip("If enabled, the player will be able to drop items by pressing the drop key. ")] public bool canDrop;

        [SerializeField, Tooltip("Reference to the Generic Weapon Pickeable Prefab")] private WeaponPickUp genericWeaponPickeable;

        [SerializeField, Tooltip("Reference to the Generic Ammo Pickeable Prefab")] private AmmoPickUp genericAmmoPickeable;

        [SerializeField, Tooltip("Reference to the GenericPickeable Prefab")] private ItemPickUp genericPickeable;

        [SerializeField, Tooltip("Distance to drop the item from the player. ")] private float dropDistance;

        [SerializeField, Tooltip("Force to apply on the dropped item.")] private float dropImpulse;

        private IInteractable interactable;

        private float timeElapsed;

        public delegate void OpenInventory();

        public OpenInventory openInventory;

        PlayerStats playerStats;

        private WeaponController weaponController;

        [System.Serializable]
        public class Events
        {
            public UnityEvent onInteract,
                onDrop,
                onToggleInventory;
        }

        public Events events;

        private bool inventoryOpen = false;

        public bool InventoryOpen { get { return inventoryOpen; } }

        #region main
        private void Start()
        {
            // Initial settings
            weaponController = GetComponent<WeaponController>();
            playerStats = GetComponent<PlayerStats>();
            timeElapsed = timeToInteract;
            InitializeInventory();
        }

        private void Update()
        {
            if (!playerStats.isDead)
            {
                HandleInventory();
            }
            if (PlayerStats.controllable && !playerStats.isDead)
            {
                HandleDropping();
            }
            // if there is no interactable detected we should not handle interaction
            if (interactable == null || !PlayerStats.controllable) return;

            HandleInteraction();
        }

        #endregion

        #region Interaction

        private void HandleInteraction()
        {
            // handle timings when the player interacts
            if (InputManager.playerInputs.Interact) timeElapsed -= Time.deltaTime;
            else timeElapsed = timeToInteract;

            // Interaction occured
            if (timeElapsed <= 0 && (interactable is not WeaponPickUp && weaponController.reloading || !weaponController.reloading))
            {
                if (interactable is WeaponPickUp || interactable is ItemPickUp || interactable is AmmoPickUp) SoundManager.Instance.PlaySound(pickUpItemSFX, .5f);

                interactable.Interact(this);
                timeElapsed = timeToInteract;
                events.onInteract?.Invoke();
            }
        }

        private void HandleInventory()
        {
            // Handle inventory and hotbar UI Management
            if (InputManager.playerInputs.OpenInventory) openInventory?.Invoke();

            HandleHotBar();
        }

        #endregion

        #region Dropping
        private void HandleDropping()
        {
            // if the conditions are met and the correct inputs are pressed, handle dropping
            if (InputManager.playerInputs.Drop && canDrop && weaponController.weapon != null && !weaponController.weaponHidden && !weaponController.reloading) DropCurrentWeapon();
        }

        private void DropCurrentWeapon()
        {
            // Remove the current weapon, but save it for further operations
            Weapon_SO saveWeapon = weaponController.weapon;
            weaponController.SetWeapon(null);


            // Instantiate a Pickeable object
            // Calculate where to drop depending on where the player is looking at
            Vector2 dirToDrop = GetComponent<PlayerMovement>().facingRight ? transform.right : -transform.right;
            WeaponPickUp pickeable = Instantiate(genericWeaponPickeable, transform.position + (Vector3)dirToDrop * dropDistance, Quaternion.identity) as WeaponPickUp;
            pickeable.dropped = true;
            pickeable.currentBullets = weaponController.id.currentBullets;
            pickeable.totalBullets = weaponController.id.totalBullets;
            weaponController.id.currentBullets = 0;

            weaponController.ReleaseCurrentWeapon();

            UIController.updateWeaponInfo?.Invoke();

            // Assign the weapon we saved to the new pickeable
            pickeable.AssignWeapon(saveWeapon);

            // Grab Rigidbody2D component from the pickeable
            Rigidbody2D rb = pickeable.GetComponent<Rigidbody2D>();

            // Apply forces to the pickeable ( dropping effect )
            rb.AddForce(dirToDrop * dropImpulse, ForceMode2D.Impulse);

            UIController.instance.hotbarList[weaponController.currentWeapon].GetComponent<InventorySlot>().slotData.amount = 0;
            UIController.updateWeaponInfo?.Invoke();

            events.onDrop?.Invoke();
        }

        public void DropInventoryItem(InventorySlot.SlotData slot)
        {
            Vector2 dirToDrop = GetComponent<PlayerMovement>().facingRight ? transform.right : -transform.right;

            if (slot.inventoryItem is Weapon_SO)
            {
                if (weaponController.id != null && slot.saveWeapon.gameObject == weaponController.id.gameObject) DropCurrentWeapon();
                else
                {
                    WeaponPickUp pickeable = Instantiate(genericWeaponPickeable, transform.position + (Vector3)dirToDrop * dropDistance, Quaternion.identity) as WeaponPickUp;
                    pickeable.dropped = true;
                    pickeable.currentBullets = slot.saveWeapon.GetComponent<WeaponIdentification>().currentBullets;
                    pickeable.totalBullets = slot.saveWeapon.GetComponent<WeaponIdentification>().totalBullets;

                    // Assign the weapon we saved to the new pickeable
                    pickeable.AssignWeapon((Weapon_SO)slot.inventoryItem);

                    // Grab Rigidbody2D component from the pickeable
                    Rigidbody2D rb = pickeable.GetComponent<Rigidbody2D>();

                    // Apply forces to the pickeable ( dropping effect )
                    rb.AddForce(dirToDrop * dropImpulse, ForceMode2D.Impulse);
                }
            }
            else if (slot.inventoryItem is AmmoType_SO)
            {
                AmmoPickUp pickeable = Instantiate(genericAmmoPickeable, transform.position + (Vector3)dirToDrop * dropDistance, Quaternion.identity) as AmmoPickUp;
                pickeable.ammoAmount = slot.amount;
                pickeable.ammoType = (AmmoType_SO)slot.inventoryItem;


                // Grab Rigidbody2D component from the pickeable
                Rigidbody2D rb = pickeable.GetComponent<Rigidbody2D>();

                // Apply forces to the pickeable ( dropping effect )
                rb.AddForce(dirToDrop * dropImpulse, ForceMode2D.Impulse);
            }
            else
            {
                ItemPickUp pickeable = Instantiate(genericPickeable, transform.position + (Vector3)dirToDrop * dropDistance, Quaternion.identity) as ItemPickUp;
                pickeable.amount = slot.amount;
                pickeable.item = (Item_SO)slot.inventoryItem;


                // Grab Rigidbody2D component from the pickeable
                Rigidbody2D rb = pickeable.GetComponent<Rigidbody2D>();

                // Apply forces to the pickeable ( dropping effect )
                rb.AddForce(dirToDrop * dropImpulse, ForceMode2D.Impulse);
            }
        }
        #endregion

        #region others
        private void OnTriggerStay2D(Collider2D other)
        {
            // Check if there is any object to trigger
            if (other.GetComponent<ITriggerable>() != null) other.GetComponent<ITriggerable>().StayTrigger(this.gameObject);

            // Check if there is any object to interact with
            if (other.tag != "Interactable" || other.GetComponent<IInteractable>() == null || interactable != null) return;

            interactable = other.GetComponent<IInteractable>();
            UIController.onInteractAvailable?.Invoke(other.GetComponent<Interactable>().interactText);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            // Handles Trigger once ( on Enter )
            if (other.GetComponent<ITriggerable>() == null) return;

            other.GetComponent<ITriggerable>().Trigger(this.gameObject);
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            // Disable the interaction
            if (other.GetComponent<IInteractable>() != null)
            {
                interactable = null;
                UIController.onInteractionDisabled?.Invoke();
            }

            // Exits trigger
            if (other.GetComponent<ITriggerable>() == null) return;

            other.GetComponent<ITriggerable>().ExitTrigger(this.gameObject);
        }

        private void InitializeInventory()
        {
            // Checks what methods to use depending on the inventory method
            switch (inventoryMethod)
            {
                case InventoryMethod.HotbarOnly:
                    UIController.instance.InitializeHotBar(hotbarSize, GetComponent<WeaponController>());
                    break;
                case InventoryMethod.Full:
                    UIController.instance.InitializeHotBar(hotbarSize, GetComponent<WeaponController>());
                    UIController.instance.InitializeFullInventory(hotbarSize, inventoryRowsAmount, inventoryColumnsAmount, GetComponent<WeaponController>());
                    openInventory = ToggleInventory;
                    break;
            }

            inventory.SetActive(false);
        }
        private void HandleHotBar()
        {
            // You cannot select a weapon while reloading
            if (!weaponController || weaponController.reloading) return;

            // Based on the mouse wheel input, change the weapon
            if (InputManager.playerInputs.MouseWheel.y > 0 || InputManager.playerInputs.NextWeapon)
                if (weaponController.currentWeapon < hotbarSize - 1)
                {
                    weaponController.currentWeapon++;
                    if (UIController.instance.hotbarList[weaponController.currentWeapon].GetComponent<InventorySlot>().slotData.saveWeapon != null)
                        weaponController.inventory[weaponController.currentWeapon] = UIController.instance.hotbarList[weaponController.currentWeapon].GetComponent<InventorySlot>().slotData.saveWeapon.GetComponent<WeaponIdentification>();
                    weaponController.UnholsterWeapon();
                    UIController.updateHotbarSelection?.Invoke();
                }
            if (InputManager.playerInputs.MouseWheel.y < 0 || InputManager.playerInputs.PreviousWeapon)
                if (weaponController.currentWeapon > 0)
                {
                    weaponController.currentWeapon--;
                    if (UIController.instance.hotbarList[weaponController.currentWeapon].GetComponent<InventorySlot>().slotData.saveWeapon != null)
                        weaponController.inventory[weaponController.currentWeapon] = UIController.instance.hotbarList[weaponController.currentWeapon].GetComponent<InventorySlot>().slotData.saveWeapon.GetComponent<WeaponIdentification>();
                    weaponController.UnholsterWeapon();
                    UIController.updateHotbarSelection?.Invoke();
                }
        }

        private void ToggleInventory()
        {
            inventory.SetActive(!inventory.gameObject.activeSelf);

            if (inventory.activeSelf)
            {
                PlayerStats.LoseControl();
                SoundManager.Instance.PlaySound(openInventorySFX, 1);
                Cursor.visible = true;
                inventoryOpen = true;
            }
            else
            {
                inventoryOpen = false;
                GetComponent<PlayerStats>().CheckIfCanGrantControl();
                UIController.currentInventorySlot = null;
                UIController.highlightedInventorySlot = null;
                Tooltip.Instance.Hide();
                SoundManager.Instance.PlaySound(closeInventorySFX, 1);
                Cursor.visible = false;
            }

            events.onToggleInventory?.Invoke();
        }
        #endregion
    }

}