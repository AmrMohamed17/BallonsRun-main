using UnityEngine;
using UnityEngine.Events;
namespace cowsins2D
{
    public class AmmoPickUp : Interactable
    {
        [Tooltip("Amount of bullets to pick up.")] public int ammoAmount;

        [Tooltip("AmmoType_SO to pick up.")] public AmmoType_SO ammoType;

        [Tooltip("Sprite Renderer to display the item icon. ")] [SerializeField] private Transform graphics;

        public UnityEvent onInteract;
        private void Start()
        {
            SetPickeableGraphics();
        }
        public override void Interact(InteractionManager source)
        {
            // Perform Basic or Full operations depending on the Player´s current inventory system selected.

            // Get a reference to the player WeaponController.
            WeaponController wc = source.GetComponent<WeaponController>();

            switch (source.inventoryMethod)
            {
                case InteractionManager.InventoryMethod.HotbarOnly:
                    BasicInteraction(wc);
                    break;
                case InteractionManager.InventoryMethod.Full:
                    FullInteraction(wc);
                    break;
            }


            //Perform custom interaction operations.
            onInteract?.Invoke();
        }

        private void BasicInteraction(WeaponController wc)
        {
            // Checks if the weapon has limited magazines and if the ammo type matches the weapon's ammo type.
            if (wc.weapon == null || !wc.weapon.limitedMagazines)
            {
                Debug.LogError("Can´t pick up bullets for a weapon that does not have limited bullets or if weapon is null.");
                return;
            }

            // Checks if the ammo type matches the weapon's ammo type.
            if (wc.weapon.ammoType != ammoType)
            {
                Debug.LogError("Can´t pick up bullets for a weapon that does not use the same type of bullets.");
                return;
            }

            wc.id.totalBullets += ammoAmount;
            UIController.updateWeaponInfo?.Invoke();
            Destroy(this.gameObject);
        }

        private void FullInteraction(WeaponController wc)
        {
            // Checks if the inventory is full. If it is, the ammo will not be picked up.
            if (UIController.instance.IsInventoryFull()) return;

            // Calculates the remaining ammo after picking up the ammo.
            int remainingAmmo = ammoAmount;

            // While there is still remaining ammo, add it to the inventory.
            while (remainingAmmo > 0)
            {
                int ammoToAdd = Mathf.Min(remainingAmmo, ammoType.maxStack);
                UIController.instance.PopulateInventory(ammoType, ammoToAdd);
                remainingAmmo -= ammoToAdd;
            }

            // Checks if the player has any ammo in the weapon and the inventory.
            wc.CheckForBullets();

            Destroy(this.gameObject);
        }
        public void SetPickeableGraphics()
        {
            if (ammoType == null || graphics == null)
            {
                Debug.LogError(this.name + " ´s variable is null.");
                Debug.Break();
                return;
            }
            if (graphics != null && graphics.GetComponent<SpriteRenderer>() != null)
                graphics.GetComponent<SpriteRenderer>().sprite = ammoType.itemIcon;
            else
            {
                GameObject obj = Instantiate(ammoType.item3DObject, transform.position, Quaternion.identity);
                obj.transform.SetParent(graphics.transform);
            }
        }


    }

}