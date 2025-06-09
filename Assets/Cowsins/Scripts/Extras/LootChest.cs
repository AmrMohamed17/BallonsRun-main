using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.Events;

namespace cowsins2D
{
    public class LootChest : Interactable, ITriggerable
    {
        [System.Serializable]
        public class LootSlot
        {
            public Item_SO lootItem;
            public int amount;
        }
        [SerializeField, Tooltip("Object that contains the chest UI.")] private GameObject lootUI;

        [SerializeField, Range(0,5), Tooltip("Rows and columns to procedurally generate the chest UI Slots.")] private int rows, columns;

        [SerializeField, Tooltip("Object that contains the UI Slots.")] private Transform inventoryContainer;

        [SerializeField, Tooltip("Reference to the UI Inventory Slot.")] private InventorySlot inventorySlot;

        [SerializeField, Tooltip("Loot inside the chest, notice that you cannot have more elements than Row*Columns.")] private LootSlot[] loot;

        private InteractionManager interactionManager;

        private bool everOpened = false;

        private bool canClose = false;

        public UnityEvent onInteract;

        private void Awake()
        {
            lootUI.SetActive(false);
            if(loot.Length > rows * columns )
            {
                Debug.LogError("Loot items amount cannot be greater than the amount of available slots.");
                return;
            }
        }

        private void Update()
        {
            if (!lootUI.activeSelf || !canClose) return;
            // Because opening the inventory disables the control over the player, we need a way to close the loot chest without giving the player control permissions.
            // This simulates the interaction from the interaction manager inside the loot chest, and it is only applicable for this loot chest.
            // Subject to change for future versions of the asset.
            if (InputManager.playerInputs.Interact) Interact(interactionManager); 
        }

        public override void Interact(InteractionManager source)
        {
            canClose = false;

            // Set the interaction manager. This has been sent from the InteractionManager
            interactionManager = source;

            // Perform interaction behaviours
            // Enable the UI if its closed, and disable if its open.
            lootUI.SetActive(!lootUI.activeSelf);

            onInteract?.Invoke(); 

            source.openInventory?.Invoke();

            Invoke(nameof(CanClose), source.TimeToInteract); 

            // If this is the first time we opened the chest, then we need to initialize or generate the inventory
            if (everOpened) return;
            InitializeFullInventory(3, 3, source.GetComponent<WeaponController>());
        }

        private void CanClose() => canClose = true;


        public List<GameObject> inventoryList = new List<GameObject>();
        public void InitializeFullInventory(int inventoryRowsAmount, int inventoryColumnsAmount, WeaponController controller)
        {
            everOpened = true;

            // Handles Rows creation
            for (int i = 0; i < inventoryRowsAmount; i++)
            {
                // Creates a game object that will contain each item of the row.
                GameObject rowObject = new GameObject();
                rowObject.name = $"Inventory Row {i}";
                HorizontalLayoutGroup layoutObject = rowObject.AddComponent<HorizontalLayoutGroup>();
                layoutObject.padding.left = 41;
                layoutObject.spacing = 60;

                rowObject.transform.SetParent(inventoryContainer);

                // Populates the rows. These could be considered column items.
                for (int j = 0; j < inventoryColumnsAmount; j++)
                {
                    GameObject slot = Instantiate(inventorySlot, rowObject.transform).gameObject;
                    slot.name = $"Inventory slot {i}{j}";

                    InventorySlot invSlot = slot.GetComponent<InventorySlot>();
                    invSlot.controller = controller;
                    inventoryList.Add(slot);
                    invSlot.id = inventoryList.IndexOf(slot);
                    invSlot.isHotBarSlot = false;
                }
            }

            // if the chest has loot to spawn in, spawn them.
            if (loot.Length <= 0) return;

            // Foreach slot, populate in case we have to.
            foreach(var item in loot)
            { 
                if(item !=null)
                {
                   if(item.lootItem is Weapon_SO)
                    {
                        Weapon_SO weaponSO = item.lootItem as Weapon_SO;
                        WeaponIdentification weapon = Instantiate(weaponSO.weaponObject, controller.weaponHolder.position, controller.weaponHolder.rotation, controller.weaponHolder);
                        weapon.currentBullets = weaponSO.magazineSize;
                        if (weaponSO.limitedMagazines) weapon.totalBullets = weaponSO.magazineSize * weaponSO.amountOfMagazines;
                        else weapon.totalBullets = weaponSO.magazineSize;
                        weapon.gameObject.SetActive(false);
                        UIController.instance.PopulateArrayWithWeapon(inventoryList, item.lootItem, item.amount, weapon.gameObject);
                    }
                    else UIController.instance.PopulateArray(inventoryList, item.lootItem, item.amount);
                }
            }
        }

        public void Trigger(GameObject target)
        {

        }
        public void StayTrigger(GameObject target)
        {

        }
        public void ExitTrigger(GameObject target)
        {
            lootUI.SetActive(false);
        }

    }


}