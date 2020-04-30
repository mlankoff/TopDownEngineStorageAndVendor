using UnityEngine;
using MoreMountains.Tools;
using System.IO;
using System.Collections.Generic;

namespace MoreMountains.InventoryEngine
{
    public class Storage : MonoBehaviour
    {

        #region ----------Animator Configuration----------

        [Header("Animator Configuration")]
        [MMInformation("If checked Animator will be used", MMInformationAttribute.InformationType.Info, false)]
        public bool useAnimator = true;

        [MMInformation("If 'Use Animator' is checked we will use 'Animator Parameter' to Open - Close storage. This is CASE SENSITIVE", MMInformationAttribute.InformationType.Info, false)]
        public string animatorParameter = "Open";

        [MMInformation("Your storage animator", MMInformationAttribute.InformationType.Info, false)]
        public Animator animator;

        #endregion

        #region ----------Storage Configuration----------


        [Header("Storage Configuration")]
        [MMInformation("Your main inventory inventory", MMInformationAttribute.InformationType.Info, false)]
        public Inventory mainInventory;

        [MMInformation("Your storage inventory display from canvas", MMInformationAttribute.InformationType.Info, false)]
        public StorageDisplay storageInventoryDisplay;

        [MMInformation("Your main inventory display from canvas", MMInformationAttribute.InformationType.Info, false)]
        public InventoryDisplay mainInventoryDisplay;

        [MMInformation("If unchecked your storage inventory name will be changed to Storage Inventory Name. If checked your storage inventory name will be automaticly generated", MMInformationAttribute.InformationType.Info, false)]
        public bool autoGenerateStorageName = true;

        [MMInformation("If Auto Generate Storage Name is set to <false> your storage inventory will be changed to given name", MMInformationAttribute.InformationType.Info, false)]
        public string storageInventoryName = "Storage01";

        [Header("Storage Display Configuration")]
        [MMInformation("Number of storage display columns", MMInformationAttribute.InformationType.Info, false)]
        public int storageColumns = 2;

        [MMInformation("Number of storage display rows", MMInformationAttribute.InformationType.Info, false)]
        public int storageRows = 4;

        [MMInformation("If checked storage display will show title", MMInformationAttribute.InformationType.Info, false)]
        public bool useTitle = true;

        [MMInformation("Storage display title", MMInformationAttribute.InformationType.Info, false)]
        public string storageDisplayTitle = "Storage";

        [Header("Loot Configuration")]
        [MMInformation("If checked, loot will spawn on first storage open", MMInformationAttribute.InformationType.Info, false)]
        public bool useLoot = true;

        [MMInformation("If checked, storage will be populated with random items and random quantities from list. Use Loot must be checked.", MMInformationAttribute.InformationType.Info, false)]
        public bool randomLoot = true;

        [MMInformation("Max number of items generated on random loot. Can't be less than 1.", MMInformationAttribute.InformationType.Info, false)]
        public int randomLootMaxItems = 2;

        [MMInformation("If 'Random Loot' is unchecked, items will be populated from top one to bottom one. If given quantity is higher than max stack it will create only one stack with max quantity", MMInformationAttribute.InformationType.Info, false)]
        public List<Loot> lootbox = new List<Loot>();

        #endregion

        #region ----------Protected Section----------

        protected const string _saveFolderName = "InventoryEngine/";

        protected const string _saveFileExtension = ".inventory";

        protected string _inventoryName;

        protected InventoryInputManagerExtended _inputManager;

        protected Inventory _storageInventory;

        [HideInInspector]
        public bool isOpen = false;

        #endregion

        #region ----------Animator Methods----------

        /// <summary>
        /// If Use Animator is checked and we have animator we play close animator
        /// </summary>
        protected virtual void PlayCloseAnimation()
        {
            //Use Animator is checked
            if (useAnimator)
            {
                //we have assigned animator
                if (animator != null)
                {
                    //we change in animator bool with the name we give in animatorParameter
                    animator.SetBool(animatorParameter, false);
                }
            }
        }

        /// <summary>
        /// If Use Animator is checked and we have animator we play open animator
        /// </summary>
        protected virtual void PlayOpenAnimation()
        {
            //Use Animator is checked
            if (useAnimator)
            {
                //we have assigned animator
                if (animator != null)
                {
                    //we change in animator bool with the name we give in animatorParameter
                    animator.SetBool(animatorParameter, true);
                }
            }
        }

        #endregion

        #region ----------Storage Display Methods----------

        /// <summary>
        /// Setup storage display to fit storage inventory size
        /// </summary>
        protected virtual void SetupStorageDispaly()
        {
            storageInventoryDisplay.TargetInventoryName = _inventoryName;
            storageInventoryDisplay.NumberOfColumns = storageColumns;
            storageInventoryDisplay.NumberOfRows = storageRows;
            storageInventoryDisplay.SetupInventoryDisplay();
            if (useTitle)
            {
                storageInventoryDisplay.ChangeDisplayTitle(storageDisplayTitle);
            }
        }

        /// <summary>
        /// Add storage inventory display to main inventory display as next and previous inventory to handle switching between storage and inventory with keyboard / joypad
        /// </summary>
        protected virtual void AddStorageDisplayToMainDisplay()
        {
            mainInventoryDisplay.NextInventory = storageInventoryDisplay;
            mainInventoryDisplay.PreviousInventory = storageInventoryDisplay;
        }

        /// <summary>
        /// Remove storage inventory display from main inventory display
        /// </summary>
        protected virtual void RemoveStorageDisplayFromMainDisplay()
        {
            mainInventoryDisplay.NextInventory = null;
            mainInventoryDisplay.PreviousInventory = null;
        }

        #endregion

        #region ----------Storage Inventory Methods----------

        /// <summary>
        /// Fill storage with items based on given config
        /// </summary>
        protected virtual void PopulateStorageInventory()
        {
            //we check if we want to use loot
            if (useLoot)
            {
                //we check if loot must be generated randomly
                if (randomLoot)
                {
                    int slot = 0;
                    int lootItems = Random.Range(min: 1, max: randomLootMaxItems);
                    for (int i = 0; i <= lootItems; i++)
                    {
                        // Select a random item from the table
                        int randomItem = Random.Range(0, lootbox.Count - 1);

                        // dice a chance from 0 to 100
                        int randomChance = Random.Range(0, 100);

                        if (randomChance <= lootbox[randomItem].dropChance)
                        {
                            _storageInventory.Content[slot] = lootbox[i].item;
                            if (lootbox[i].quantity > 1)
                            {
                                int rolledQuantity = Random.Range(1, lootbox[i].maxQuantity);
                                if (rolledQuantity > lootbox[i].item.MaximumStack)
                                {
                                    rolledQuantity = lootbox[i].item.MaximumStack;
                                    _storageInventory.Content[slot].Quantity = lootbox[i].quantity;
                                }
                            }
                            slot++;
                        }
                    }
                }
                else
                {
                    int inventorySize = _storageInventory.Content.Length - 1;
                    int i = 0;
                    foreach (Loot element in lootbox)
                    {
                        if (i < inventorySize)
                        {
                            if (element.quantity > element.item.MaximumStack)
                            {
                                _storageInventory.Content[i] = element.item;
                                _storageInventory.Content[i].Quantity = element.item.MaximumStack;
                            }
                            else
                            {
                                _storageInventory.Content[i] = element.item;
                                _storageInventory.Content[i].Quantity = element.quantity;
                            }
                        }
                        else
                        {
                            return;
                        }
                        i++;
                    }
                }
                MMInventoryEvent.Trigger(MMInventoryEventType.ContentChanged, null, _storageInventory.name, null, 0, 0);
            }
        }


        /// <summary>
        /// Set storage inventory name based on given config
        /// </summary>
        protected virtual void SetStorageInventoryName()
        {
            //auto generation storage inventory name section
            if (autoGenerateStorageName)
            {
                //in 99,99% times we can't put more then one storage object at the same place so we gather it's position and scene name to generate unique inventory name 
                float x = transform.position.x;
                float y = transform.position.y;
                string scene = transform.gameObject.scene.name.ToString();
                _inventoryName = "Inv" + x.ToString() + y.ToString() + scene;
            }
            //hand given storage inventory name section
            else
            {
                _inventoryName = storageInventoryName;
            }
            _storageInventory.gameObject.name = _inventoryName;
        }

        /// <summary>
        /// Delete storage inventory when player decides to remove storage object from the scene. It only deletes inventory save not GameObject.
        /// </summary>
        public virtual void DeleteStorageInventory()
        {
            MMSaveLoadManager.DeleteSave(_inventoryName + _saveFileExtension, _saveFolderName);
        }

        #endregion

        /// <summary>
        /// Initialize all data to run storage
        /// </summary>
        protected virtual void InitializeStorageData()
        {
            _inputManager = Object.FindObjectOfType<InventoryInputManagerExtended>();
            GameObject parent = transform.parent.gameObject;
            _storageInventory = parent.GetComponentInChildren<Inventory>();
            SetStorageInventoryName();
            _storageInventory.ResizeArray(storageColumns * storageRows);
        }

        /// <summary>
        /// Opens storage inventory and display
        /// </summary>
        public virtual void OpenStorage()
        {
            InitializeStorageData();
            isOpen = true;
            if (!File.Exists(_saveFolderName + _saveFileExtension))
            {
                PopulateStorageInventory();
            }
            _storageInventory.LoadSavedInventory();
            storageInventoryDisplay.gameObject.SetActive(true);
            AddStorageDisplayToMainDisplay();
            SetupStorageDispaly();
            PlayOpenAnimation();
            storageInventoryDisplay.SetTargetInventory(_storageInventory);
            MMInventoryEvent.Trigger(MMInventoryEventType.Redraw, null, _storageInventory.name, null, 0, 0);
            _inputManager.ToggleInventory();
        }

        /// <summary>
        /// Close storage inventory and display
        /// </summary>
        public virtual void CloseStorage()
        {
            isOpen = false;
            PlayCloseAnimation();
            _storageInventory.SaveInventory();
            mainInventory.SaveInventory();
            RemoveStorageDisplayFromMainDisplay();
            _storageInventory = null;
            storageInventoryDisplay.gameObject.SetActive(false);
            _inputManager.CloseInventory();
        }

    }
}