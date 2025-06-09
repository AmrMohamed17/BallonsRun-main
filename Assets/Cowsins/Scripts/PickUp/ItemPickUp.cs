using UnityEngine;
using UnityEngine.Events;

namespace cowsins2D
{
    public class ItemPickUp : Interactable
    {
        [Tooltip("Amount to pick up, these will be stored in the Inventory in case you use a full inventory system.")] public int amount;

        [Tooltip("Item_SO to pick up.")] public Item_SO item;

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
            // Use the item on interact.
            item.Use(wc);
            Destroy(this.gameObject);
        }

        private void FullInteraction(WeaponController wc)
        {
            // Checks if the inventory is full. If it is, the item will not be picked up.
            if (UIController.instance.IsInventoryFull()) return;

            // Calculates the remaining amount of the item after picking it up.
            int remainingAmount = amount;

            // While there is still remaining amount, add it to the inventory.
            while (remainingAmount > 0)
            {
                int ammoToAdd = Mathf.Min(remainingAmount, item.maxStack);
                UIController.instance.PopulateInventory(item, ammoToAdd);
                remainingAmount -= ammoToAdd;
            }

            Destroy(this.gameObject);
        }

        public void SetPickeableGraphics()
        {
            if (item == null || graphics == null)
            {
                Debug.LogError(this.name + " ´s variable is null.");
                Debug.Break();
                return;
            }
            if (graphics != null && graphics.GetComponent<SpriteRenderer>() != null)
                graphics.GetComponent<SpriteRenderer>().sprite = item.itemIcon;
            else
            {
                GameObject obj = Instantiate(item.item3DObject, transform.position, Quaternion.identity);
                obj.transform.SetParent(graphics.transform);
            }
        }


    }

}