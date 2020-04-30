using MoreMountains.Tools;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace MoreMountains.InventoryEngine
{
    public class Vendor : MonoBehaviour
    {

        [Header("Vendor Sell / Buy Settings")]
        [MMInformation("If unchecked vendor will have unlimited stock", MMInformationAttribute.InformationType.Info, false)]
        public bool limitedStock = false;

        [MMInformation("If checked player can buy items back he sold to vendor", MMInformationAttribute.InformationType.Info, false)]
        public bool canRollBackItems = true;

        [Header("Vendor Configuration")]

        public VendorDisplay vendorInventoryDisplay;
        public GameObject rollBackButton;
        public VendorDisplay vendorInventoryRollBackDisplay;
        public GameObject inventoryButtons;
        public Inventory mainInventory;
        public Inventory vendorInventory;
        public Inventory vendorRollBackInventory;
        public InventoryDisplay mainInventoryDisplay;
        public InventoryItem currencyItem;

        [MMInformation("If unchecked your vendor inventory name and vendor roll back invetory will be changed to hand given name. If checked your vendor inventory name will be automaticly generated", MMInformationAttribute.InformationType.Info, false)]
        public bool autoGenerateVendorInventoryName = true;

        [MMInformation("If Auto Generate Vendor Name is unchecked your vendor inventory will be changed to given name", MMInformationAttribute.InformationType.Info, false)]
        public string vendorInventoryName = "Vendor01";

        [MMInformation("If Auto Generate Vendor Name is unchecked your vendor inventory will be changed to given name", MMInformationAttribute.InformationType.Info, false)]
        public string vendorRollBackInventoryName = "Vendor01RollBack";

        [Header("Vendor Display Configuration")]
        [MMInformation("Number of vendor display columns", MMInformationAttribute.InformationType.Info, false)]
        public int vendorColumns = 2;

        [MMInformation("Number of vendor display rows", MMInformationAttribute.InformationType.Info, false)]
        public int vendorRows = 4;

        [MMInformation("Number of vendor roll back display columns", MMInformationAttribute.InformationType.Info, false)]
        public int vendorRollBackColumns = 2;

        [MMInformation("Number of vendor roll back display rows", MMInformationAttribute.InformationType.Info, false)]
        public int vendorRollBackRows = 4;

        [MMInformation("If checked vendor display will show title", MMInformationAttribute.InformationType.Info, false)]
        public bool useTitle = true;

        [MMInformation("Vendor display title", MMInformationAttribute.InformationType.Info, false)]
        public string vendorDisplayTitle = "Vendor";

        [MMInformation("Vendor roll back display title", MMInformationAttribute.InformationType.Info, false)]
        public string vendorRollBackDisplayTitle = "Vendor";

        public List<VendorContent> vendorContent = new List<VendorContent>();

        #region ----------Protected Section----------

        protected const string _saveFolderName = "InventoryEngine/";

        protected const string _saveFileExtension = ".inventory";

        protected string _inventoryName;

        protected string _inventoryRollBackName;

        protected InventoryInputManagerExtended _inputManager;

        protected InventoryDetailsExtended _itemDetails;

        [HideInInspector]
        public bool isOpen = false;

        [HideInInspector]
        public bool isVendorRollBackOpen = false;

        #endregion

        #region ----------Vendor Inventory Initial Methods----------

        /// <summary>
        /// Initialize all vendor data on load
        /// </summary>
        protected virtual void InitializeVendor()
        {
            if (canRollBackItems)
            {
                rollBackButton.SetActive(true);
            }
            else
            {
                rollBackButton.SetActive(false);
            }
            _itemDetails = Object.FindObjectOfType<InventoryDetailsExtended>();
            _inputManager = Object.FindObjectOfType<InventoryInputManagerExtended>();
            SetVendorInventoryName();
            vendorInventory.ResizeArray(vendorColumns * vendorRows);
            vendorRollBackInventory.ResizeArray(vendorRollBackColumns * vendorRollBackRows);
        }

        /// <summary>
        /// Set vendor inventory name based on given config
        /// </summary>
        protected virtual void SetVendorInventoryName()
        {
            //auto generation vendor inventory name section
            if (autoGenerateVendorInventoryName)
            {
                //in 99,99% times we can't put more then one vendor object at the same place so we gather it's position and scene name to generate unique inventory name 
                float x = transform.position.x;
                float y = transform.position.y;
                string scene = transform.gameObject.scene.name.ToString();
                _inventoryName = "Inv" + x.ToString() + y.ToString() + scene;
                _inventoryRollBackName = "InvRollBack" + x.ToString() + y.ToString() + scene;
            }
            //hand given vendor inventory name section
            else
            {
                _inventoryName = vendorInventoryName;
                _inventoryRollBackName = vendorRollBackInventoryName;
            }
            vendorInventory.gameObject.name = _inventoryName;
            vendorRollBackInventory.gameObject.name = _inventoryRollBackName;
        }

        /// <summary>
        /// Populate vendor main inventory based on bool limitedStock. Vendor stock could be unlimited or limited to given quantity
        /// </summary>
        protected virtual void PopulateVendorInventory()
        {
            //vendor has limited stock
            if (limitedStock)
            {
                int inventorySize = vendorInventory.Content.Length - 1;
                int i = 0;
                foreach (VendorContent element in vendorContent)
                {
                    if (i < inventorySize)
                    {
                        if (element.Quantity > element.Item.MaximumStack)
                        {
                            vendorInventory.Content[i] = element.Item;
                            vendorInventory.Content[i].Quantity = element.Item.MaximumStack;
                        }
                        else
                        {
                            vendorInventory.Content[i] = element.Item;
                            vendorInventory.Content[i].Quantity = element.Quantity;
                        }
                    }
                    else
                    {
                        return;
                    }
                    i++;
                }
            }
            //vendor has unlimited stock
            else
            {
                int inventorySize = vendorInventory.Content.Length - 1;
                int i = 0;
                foreach (VendorContent element in vendorContent)
                {
                    if (i < inventorySize)
                    {
                        vendorInventory.Content[i] = element.Item;
                        vendorInventory.Content[i].Quantity = 1;
                    }
                    else
                    {
                        return;
                    }
                    i++;
                }
            }
            MMInventoryEvent.Trigger(MMInventoryEventType.ContentChanged, null, vendorInventory.name, null, 0, 0);
        }

        #endregion

        #region ----------Vendor Inventory And Display Methods----------

        /// <summary>
        /// Get Player cash based on Currency Item
        /// </summary>
        /// <returns> Cash </returns>
        public virtual int GetPlayerCash()
        {
            int cash = 0;
            InventoryItem[] items = mainInventory.Content;
            foreach (InventoryItem item in items)
            {
                if (item != null && item.name == currencyItem.name)
                {
                    cash += item.Quantity;
                }
            }
            return cash;
        }

        /// <summary>
        /// Get all currency items in main inventory to get on which index they are and what is that item quantity
        /// </summary>
        /// <returns></returns>
        public virtual Dictionary<int, int> GetAllCurrencyItems()
        {
            Dictionary<int, int> currencyItems = new Dictionary<int, int>();
            InventoryItem[] items = mainInventory.Content;
            int i = 0;
            foreach (InventoryItem item in items)
            {
                if (item != null)
                {
                    if (item.name == currencyItem.name)
                    {
                        currencyItems.Add(i, item.Quantity);
                    }
                }
                i++;
            }
            return currencyItems;
        }

        /// <summary>
        /// Remove amount of currency item form main inventory even if items are in few places in inventory
        /// </summary>
        /// <param name="amount"></param>
        public virtual void PayForItem(int amount)
        {
            Dictionary<int, int> currencyItems = GetAllCurrencyItems();
            int restToPay = amount;
            for (int i = 0; i < currencyItems.Count; i++)
            {
                int index = currencyItems.ElementAt(i).Key;
                int quantity = currencyItems.ElementAt(i).Value;
                //we don't have enaugh money using i index item
                if ((restToPay - quantity) == 0)
                {
                    mainInventory.Content[index] = null;
                    MMInventoryEvent.Trigger(MMInventoryEventType.ContentChanged, null, mainInventory.name, null, 0, 0);
                    return;
                }
                //we can pay with 
                else if ((restToPay - quantity) > 0)
                {
                    restToPay -= quantity;
                    mainInventory.Content[index] = null;
                    MMInventoryEvent.Trigger(MMInventoryEventType.ContentChanged, null, mainInventory.name, null, 0, 0);
                }
                else if ((restToPay - quantity) < 0)
                {
                    int newQuantity = quantity - restToPay;
                    mainInventory.Content[index].Quantity = newQuantity;
                    MMInventoryEvent.Trigger(MMInventoryEventType.ContentChanged, null, mainInventory.name, null, 0, 0);
                    return;
                }
            }
        }

        /// <summary>
        /// Delete vendor inventory and vendor roll back inventory when player decides to remove vendor object from the scene. It only deletes inventory save not GameObject.
        /// </summary>
        public virtual void DeleteVendorInventory()
        {
            MMSaveLoadManager.DeleteSave(_inventoryName + _saveFileExtension, _saveFolderName);
            MMSaveLoadManager.DeleteSave(_inventoryRollBackName + _saveFileExtension, _saveFolderName);
        }

        /// <summary>
        /// Setup vendor display to fit vendor inventory size
        /// </summary>
        public virtual void SetupVendorDispaly()
        {
            vendorInventoryDisplay.TargetInventoryName = _inventoryName;
            vendorInventoryDisplay.NumberOfColumns = vendorColumns;
            vendorInventoryDisplay.NumberOfRows = vendorRows;
            vendorInventoryDisplay.SetupInventoryDisplay();
            if (useTitle)
            {
                vendorInventoryDisplay.ChangeDisplayTitle(vendorDisplayTitle);
            }
        }

        /// <summary>
        /// Setup vendor roll back display to fit vendor roll back inventory size
        /// </summary>
        public virtual void SetupVendorRollBackDispaly()
        {
            vendorInventoryRollBackDisplay.TargetInventoryName = _inventoryRollBackName;
            vendorInventoryRollBackDisplay.NumberOfColumns = vendorRollBackColumns;
            vendorInventoryRollBackDisplay.NumberOfRows = vendorRollBackRows;
            vendorInventoryRollBackDisplay.SetupInventoryDisplay();
            if (useTitle)
            {
                vendorInventoryRollBackDisplay.ChangeDisplayTitle(vendorRollBackDisplayTitle);
            }
        }

        /// <summary>
        /// Update vendor item display to hide / show buttons and data based on selected item in inventory
        /// </summary>
        public virtual void UpdateVendorItemDisplay()
        {
            if (isOpen)
            {
                if (_itemDetails != null)
                {
                    Inventory clickedInventory = _inputManager.CurrentlySelectedInventorySlot.ParentInventoryDisplay.TargetInventory;
                    InventorySlot slot = _inputManager.CurrentlySelectedInventorySlot;
                    int index = slot.Index;
                    InventoryItem item = clickedInventory.Content[index];
                    if (item != null)
                    {
                        _itemDetails.currencyImage.sprite = currencyItem.Icon;
                        //clicked item is in main inventory or vendor roll back inventory
                        if (clickedInventory == mainInventory)
                        {
                            _itemDetails.vendorPrice.GetComponent<Text>().text = item.Price.ToString();
                            _itemDetails.vendorQuantity.GetComponent<Text>().text = "1";
                            if (item.Quantity > 1)
                            {
                                _itemDetails.increaseButton.SetActive(true);
                                _itemDetails.decreaseButton.SetActive(true);
                            }
                            else
                            {
                                _itemDetails.increaseButton.SetActive(false);
                                _itemDetails.decreaseButton.SetActive(false);
                            }
                            _itemDetails.buyButton.SetActive(false);
                            _itemDetails.sellButton.SetActive(true);
                        }
                        else if (clickedInventory == vendorRollBackInventory)
                        {
                            _itemDetails.vendorPrice.GetComponent<Text>().text = item.Price.ToString();
                            _itemDetails.vendorQuantity.GetComponent<Text>().text = "1";
                            if (item.Quantity > 1)
                            {
                                _itemDetails.increaseButton.SetActive(true);
                                _itemDetails.decreaseButton.SetActive(true);
                            }
                            else
                            {
                                _itemDetails.increaseButton.SetActive(false);
                                _itemDetails.decreaseButton.SetActive(false);
                            }
                            _itemDetails.buyButton.SetActive(true);
                            _itemDetails.sellButton.SetActive(false);
                        }
                        //clicked item is in vendor inventory
                        else if (clickedInventory == vendorInventory)
                        {
                            _itemDetails.vendorPrice.GetComponent<Text>().text = vendorContent[index].Price.ToString();
                            _itemDetails.vendorQuantity.GetComponent<Text>().text = "1";
                            _itemDetails.buyButton.SetActive(true);
                            _itemDetails.sellButton.SetActive(false);
                            //vendo has unlimited stock
                            if (limitedStock == false)
                            {
                                if (item.MaximumStack > 1)
                                {
                                    _itemDetails.increaseButton.SetActive(true);
                                    _itemDetails.decreaseButton.SetActive(true);
                                }
                                else
                                {
                                    _itemDetails.increaseButton.SetActive(false);
                                    _itemDetails.decreaseButton.SetActive(false);
                                }

                            }
                            //vedor has limited stock
                            else
                            {
                                if (item.Quantity > 1)
                                {
                                    _itemDetails.increaseButton.SetActive(true);
                                    _itemDetails.decreaseButton.SetActive(true);
                                }
                                else
                                {
                                    _itemDetails.increaseButton.SetActive(false);
                                    _itemDetails.decreaseButton.SetActive(false);
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Add vendor inventory display to main inventory display as next and previous inventory to handle switching between vendor and inventory with keyboard / joypad
        /// </summary>
        public virtual void AddVendorDisplayToMainDisplay()
        {
            mainInventoryDisplay.NextInventory = vendorInventoryDisplay;
            mainInventoryDisplay.PreviousInventory = vendorInventoryDisplay;
        }

        /// <summary>
        /// Add vendor roll back inventory display to main inventory display as next and previous inventory to handle switching between vendor roll back and inventory with keyboard / joypad
        /// </summary>
        public virtual void AddVendorRollBackDisplayToMainDisplay()
        {
            mainInventoryDisplay.NextInventory = vendorInventoryRollBackDisplay;
            mainInventoryDisplay.PreviousInventory = vendorInventoryRollBackDisplay;
        }

        /// <summary>
        /// Remove vendor inventory display from main inventory display
        /// </summary>
        public virtual void RemoveVendorDisplayFromMainDisplay()
        {
            mainInventoryDisplay.NextInventory = null;
            mainInventoryDisplay.PreviousInventory = null;
        }

        #endregion

        /// <summary>
        /// Opens vendor inventory and display
        /// </summary>
        public virtual void OpenVendor()
        {
            InitializeVendor();
            _inputManager._inVendor = true;
            inventoryButtons.SetActive(false);
            isOpen = true;
            if (_itemDetails != null)
            {
                _itemDetails.vendorUI.SetActive(true);
            }
            UpdateVendorItemDisplay();
            if (!File.Exists(_saveFolderName + _saveFileExtension))
            {
                PopulateVendorInventory();
            }
            vendorInventory.LoadSavedInventory();
            vendorInventoryDisplay.gameObject.SetActive(true);
            SetupVendorDispaly();
            vendorInventoryDisplay.SetTargetInventory(vendorInventory);
            MMInventoryEvent.Trigger(MMInventoryEventType.Redraw, null, vendorInventory.name, null, 0, 0);
            AddVendorDisplayToMainDisplay();
            _inputManager.ToggleInventory();
        }

        /// <summary>
        /// Close vendor inventory and display
        /// </summary>
        public virtual void CloseVendor()
        {
            inventoryButtons.SetActive(true);
            _inputManager._inVendor = false;
            isOpen = false;
            if (_itemDetails != null)
            {
                _itemDetails.vendorUI.SetActive(false);
            }
            vendorInventory.SaveInventory();
            vendorRollBackInventory.SaveInventory();
            mainInventory.SaveInventory();
            vendorInventoryDisplay.gameObject.SetActive(false);
            vendorInventoryRollBackDisplay.gameObject.SetActive(false);
            _inputManager.CloseInventory();
            isVendorRollBackOpen = false;
        }
    }
}

