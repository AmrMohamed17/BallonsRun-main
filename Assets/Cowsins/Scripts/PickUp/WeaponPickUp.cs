using UnityEngine;
using UnityEngine.Events;
namespace cowsins2D
{
    public class WeaponPickUp : Interactable
    {
        [Tooltip("Weapon_SO to pick up. If using hotbar only inventory, this will go to your hotbar. " +
            "In case you are using the full inventory and your hotbar is full, it will be placed inside your inventory ( if there is still space ). " +
            "In case the hotbar is not full it will be placed in a free slot. ")]
        [SerializeField] private Weapon_SO weapon;

        [Tooltip("SpriteRenderer to display the item icon.")] [SerializeField] private Transform graphics;

        [HideInInspector] public int currentBullets, totalBullets;

        [HideInInspector] public bool dropped;

        public UnityEvent onPickUp;

        private void Start()
        {
            // Initial settings
            interactText = weapon.itemName;
            SetPickeableGraphics();

            // This gets called only if the player did not drop this pick up.
            if (dropped) return;
            currentBullets = weapon.magazineSize;
            totalBullets = weapon.amountOfMagazines * weapon.magazineSize;
        }

        public override void Interact(InteractionManager source)
        {
            // Do not forget to always pass InteractionManager as it is required
            if (source == null) return;

            onPickUp?.Invoke();

            if (source.inventoryMethod == InteractionManager.InventoryMethod.HotbarOnly)
                BasicInventory(source);
            else FullInventory(source);
        }

        #region BASIC_INVENTORY
        private void BasicInventory(InteractionManager source)
        {
            if (source.GetComponent<WeaponController>() == null) return;

            WeaponController weaponController = source.GetComponent<WeaponController>();

            if (!HotbarIsFull(weaponController, source))
            {
                // The hotbar is not full, so there is no need to keep this pickeable, destroy it
                Destroy(this.gameObject);
                return;
            }

            // Inventory is full
            // Instantiate new weapon and destroy the current one
            var wp = Instantiate(weapon.weaponObject, weaponController.weaponHolder.position, weaponController.weaponHolder.rotation, weaponController.weaponHolder);
            Destroy(weaponController.inventory[weaponController.currentWeapon].gameObject);

            // Assign this new weapon
            weaponController.inventory[weaponController.currentWeapon] = wp;
            Weapon_SO saveWeapon = weaponController.weapon;
            weaponController.SetWeapon(weapon);
            weapon = saveWeapon;

            int saveCurrentBullets = weaponController.id.currentBullets;
            int saveTotalBullets = weaponController.id.totalBullets;

            // Unholster the weapon to perform basic initial operations
            weaponController.UnholsterWeapon();

            wp.currentBullets = currentBullets;
            wp.totalBullets = totalBullets;
            totalBullets = saveTotalBullets;
            currentBullets = saveCurrentBullets;

            UIController.updateWeaponInfo?.Invoke();

            // Because the inventory is full, we have to override the graphics of this weapon pickeable
            SetPickeableGraphics();
        }

        private void FullInventory(InteractionManager source)
        {
            if (source.GetComponent<WeaponController>() == null) return;

            WeaponController weaponController = source.GetComponent<WeaponController>();

            if (!HotbarIsFull(weaponController, source))
            {
                // The hotbar is not full, so there is no need to keep this pickeable, destroy it
                Destroy(this.gameObject);
                return;
            }

            // Hotbar is full, check if inventory is full.
            if (UIController.instance.IsInventoryFull())
            {
                return;
            }

            var wp = Instantiate(weapon.weaponObject, weaponController.weaponHolder.position, weaponController.weaponHolder.rotation, weaponController.weaponHolder);

            wp.currentBullets = currentBullets;
            wp.totalBullets = totalBullets;
            wp.gameObject.SetActive(false);

            UIController.instance.PopulateInventoryWithWeapon(weapon, 1, wp.gameObject);

            Destroy(this.gameObject);
        }

        private bool HotbarIsFull(WeaponController weaponController, InteractionManager source)
        {
            // Check for any empty slot
            for (int i = 0; i < source.hotbarSize; i++)
            {
                // Is this an empty slot? 
                if (weaponController.inventory[i] == null)
                {
                    var wp = Instantiate(weapon.weaponObject, weaponController.weaponHolder.position, weaponController.weaponHolder.rotation, weaponController.weaponHolder);

                    weaponController.inventory[i] = wp;
                    weaponController.SetWeapon(weapon);
                    weaponController.UnholsterWeapon();

                    wp.currentBullets = currentBullets;
                    if (!weapon.limitedMagazines) wp.totalBullets = totalBullets;


                    UIController.instance.hotbarList[i].GetComponent<InventorySlot>().slotData = new InventorySlot.SlotData
                    {
                        inventoryItem = weapon,
                        amount = 1,
                        saveWeapon = wp.gameObject
                    };

                    UIController.updateWeaponInfo?.Invoke();

                    return false;
                }

            }
            // There are no empty slots.        
            return true;
        }

        #endregion

        #region OTHERS

        public void SetPickeableGraphics()
        {
            // Verify that there are no missing references before setting the image to the item icon.
            if (weapon == null || graphics == null)
            {
                Debug.LogError(this.name + " ´s variable is null.");
                Debug.Break();
                return;
            }
            if (graphics != null && graphics.GetComponent<SpriteRenderer>() != null)
                graphics.GetComponent<SpriteRenderer>().sprite = weapon.itemIcon;
            else
            {
                GameObject obj = Instantiate(weapon.item3DObject, transform.position, Quaternion.identity);
                obj.transform.SetParent(graphics.transform);
            }
        }

        public void AssignWeapon(Weapon_SO newWeapon)
        {
            weapon = newWeapon;
        }

        #endregion
    }

}