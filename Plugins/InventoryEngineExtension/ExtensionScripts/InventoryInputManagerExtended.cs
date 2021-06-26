using System;
using System.Linq;
using MoreMountains.Tools;
using UnityEngine;
using UnityEngine.UI;

namespace MoreMountains.InventoryEngine
{
    public class InventoryInputManagerExtended : InventoryInputManager
    {
        /// the key used to split an item
        public string SplitKey = "/";
        /// the alt key used to split an item
        public string SplitAltKey = "/";
        /// the key used to increase item quantity in vendor
        public string IncreaseQuantityKey = "+";
        /// the alt key to increase item quantity in vendor
        public string IncreaseQuantityAltKey = "+";
        /// the key to decrease item quantity in vendor
        public string DecreaseQuantityKey = "-";
        /// the alt key to decrease item quantity in vendor
        public string DecreaseQuantityAltKey = "-";
        /// the key used to sell an item
        public string SellKey = ">";
        /// the alt key used to sell an item
        public string SellAltKey = ">";
        /// the key used to buy an item
        public string BuyKey = "<";
        /// the alt key used to buy an item
        public string BuyAltKey = "<";
        /// the key used to toggle between vendor and vendor roll back display
        public string ToggleVendorKey = "?";
        /// the alt key used to toggle between vendor and vendor roll back display
        public string ToggleVendorAltKey = "?";

        #region ----------Protected Variables----------

        protected Sprite _oldSprite;

        protected Inventory _inventoryFrom;

        protected Inventory _inventoryTo;

        protected InventorySlot _slotFrom;

        protected InventorySlot _slotTo;

        protected int _indexFrom;

        protected int _indexTo;

        protected int _quantityFrom;

        protected int _quantityTo;

        protected InventoryItem _inventoryItemFrom;

        protected InventoryItem _inventoryItemTo;

        protected InventoryDetailsExtended _itemDetails;

        protected bool _isMoving = false;

        protected bool _useKeyboardJoypad = true;

        [HideInInspector]
        public bool _isPressed = false;

        [HideInInspector]
        public bool _inVendor = false;

        #endregion

        #region ----------Inventory Input Manager Methods----------

        protected override void Update()
        {
            base.Update();
            CheckForSecondClick();
        }

        protected virtual void Awake()
        {
            _itemDetails = FindObjectOfType<InventoryDetailsExtended>();
            CurrentlySelectedInventorySlot = TargetInventoryDisplay.GetComponentInChildren<InventorySlot>();
        }

        /// <summary>
        /// Nullify default Move method
        /// </summary>
        public override void Move() { }

        /// <summary>
        /// Handles the inventory related inputs and acts on them.
        /// </summary>
        protected override void HandleInventoryInput()
        {
            // if the user presses the 'toggle inventory' key
            if (Input.GetKeyDown(ToggleInventoryKey) || Input.GetKeyDown(ToggleInventoryAltKey))
            {
                // if the inventory is not open
                if (!InventoryIsOpen)
                {
                    _isPressed = false;
                    OpenInventory();
                }
                // if it's open
                else
                {
                    _isPressed = true;
                    CloseInventory();
                    _isPressed = false;
                }
            }

            // if we've only authorized input when open, and if the inventory is currently closed, we do nothing and exit
            if (InputOnlyWhenOpen && !InventoryIsOpen)
            {
                return;
            }

            // vendor increase quantity
            if (Input.GetKeyDown(IncreaseQuantityKey) || Input.GetKeyDown(IncreaseQuantityAltKey))
            {
                IncreaseQuantity();
            }

            // vendor decrease quantity
            if (Input.GetKeyDown(DecreaseQuantityKey) || Input.GetKeyDown(DecreaseQuantityAltKey))
            {
                DecreaseQuantity();
            }

            // vendor sell
            if (Input.GetKeyDown(SellKey) || Input.GetKeyDown(SellAltKey))
            {
                SellItem();
            }

            // vendor buy
            if (Input.GetKeyDown(BuyKey) || Input.GetKeyDown(BuyAltKey))
            {
                BuyItem();
            }

            // vendor toggle inventory
            if (Input.GetKeyDown(ToggleVendorKey) || Input.GetKeyDown(ToggleVendorAltKey))
            {
                Vendor openedVendor = null;
                Vendor[] vendors = FindObjectsOfType<Vendor>();
                foreach (Vendor vendor in vendors)
                {
                    if (vendor.enabled && vendor.isOpen)
                    {
                        openedVendor = vendor;
                    }
                }
                if (openedVendor != null && openedVendor.canRollBackItems)
                {
                    if (openedVendor.isVendorRollBackOpen)
                    {
                        OpenVendorInventory();
                    }
                    else
                    {
                        OpenRollBackVendorInventory();
                    }
                }
            }

            // previous inventory panel
            if (Input.GetKeyDown(PrevInvKey) || Input.GetKeyDown(PrevInvAltKey))
            {
                if (_currentInventoryDisplay.GoToInventory(-1) != null)
                {
                    _currentInventoryDisplay = _currentInventoryDisplay.GoToInventory(-1);
                }
            }

            // next inventory panel
            if (Input.GetKeyDown(NextInvKey) || Input.GetKeyDown(NextInvAltKey))
            {
                if (_currentInventoryDisplay.GoToInventory(1) != null)
                {
                    _currentInventoryDisplay = _currentInventoryDisplay.GoToInventory(1);
                }
            }

            // move
            if (Input.GetKeyDown(MoveKey) || Input.GetKeyDown(MoveAltKey))
            {
                if (!_inVendor && CurrentlySelectedInventorySlot != null)
                {
                    MoveWithKeyboardJoypad();
                }
            }

            // equip or use
            if (Input.GetKeyDown(EquipOrUseKey) || Input.GetKeyDown(EquipOrUseAltKey))
            {
                if (!_inVendor && CurrentlySelectedInventorySlot != null)
                {
                    EquipOrUse();
                }
            }

            // split
            if (Input.GetKeyDown(SplitKey) || Input.GetKeyDown(SplitAltKey))
            {
                if (!_inVendor && CurrentlySelectedInventorySlot != null)
                {
                    SplitItemStack();
                }
            }

            // equip
            if (Input.GetKeyDown(EquipKey) || Input.GetKeyDown(EquipAltKey))
            {
                if (!_inVendor && CurrentlySelectedInventorySlot != null)
                {
                    CurrentlySelectedInventorySlot.Equip();
                }
            }

            // use
            if (Input.GetKeyDown(UseKey) || Input.GetKeyDown(UseAltKey))
            {
                if (!_inVendor && CurrentlySelectedInventorySlot != null)
                {
                    CurrentlySelectedInventorySlot.Use();
                }
            }

            // drop
            if (Input.GetKeyDown(DropKey) || Input.GetKeyDown(DropAltKey))
            {
                if (!_inVendor && CurrentlySelectedInventorySlot != null)
                {
                    CurrentlySelectedInventorySlot.Drop();
                }
            }
        }

        /// <summary>
        /// Close inventory 
        /// </summary>
        public override void CloseInventory()
        {
            base.CloseInventory();

            Storage[] storages = FindObjectsOfType<Storage>();
            foreach (Storage storage in storages)
            {
                if (storage.enabled && storage.isOpen)
                {
                    storage.CloseStorage();
                }
            }

            Vendor[] vendors = FindObjectsOfType<Vendor>();
            foreach (Vendor vendor in vendors)
            {
                if (vendor.enabled && vendor.isOpen)
                {
                    vendor.CloseVendor();
                }
            }
            _inVendor = false;
        }

        #endregion

        #region ----------Storage Move And Split Methods----------

        /// <summary>
        /// Find first free slot in inventory
        /// </summary>
        /// <param name="inventory"> inventory where we search for first free slot</param>
        /// <returns></returns>
        public virtual int FindFirstFreeSlot(Inventory inventory)
        {
            int freeSlot = -1;
            int i = 0;
            foreach (InventoryItem item in inventory.Content)
            {
                if (item == null)
                {
                    freeSlot = i;
                    break;
                }
                i++;
            }
            return freeSlot;
        }

        /// <summary>
        /// Split item stack in half
        /// </summary>
        public virtual void SplitItemStack()
        {
            if (!_inVendor && !_isMoving)
            {
                Inventory _inventory = CurrentlySelectedInventorySlot.ParentInventoryDisplay.TargetInventory;
                int _freeSlots = _inventory.NumberOfFreeSlots;
                if (_freeSlots < 1)
                {
                    return;
                }
                else
                {
                    _slotFrom = CurrentlySelectedInventorySlot;
                    _indexFrom = _slotFrom.Index;
                    _inventoryItemFrom = _inventory.Content[_indexFrom];
                    if (_inventoryItemFrom != null)
                    {
                        _quantityFrom = _inventoryItemFrom.Quantity;
                        if (_quantityFrom > 1)
                        {
                            int low = (int)Mathf.Floor(_quantityFrom / 2.0f);
                            int high = (int)Mathf.Ceil(_quantityFrom / 2.0f);
                            InventoryItem tmpItemLow = _inventoryItemFrom.Copy();
                            tmpItemLow.Quantity = low;
                            InventoryItem tmpItemHigh = _inventoryItemFrom.Copy();
                            tmpItemHigh.Quantity = high;
                            int freeSlot = FindFirstFreeSlot(_inventory);
                            _inventory.Content[_indexFrom] = tmpItemHigh;
                            _inventory.Content[freeSlot] = tmpItemLow;
                            MMInventoryEvent.Trigger(MMInventoryEventType.ContentChanged, null, _inventory.name, null, 0, 0);
                            MMInventoryEvent.Trigger(MMInventoryEventType.ContentChanged, null, _inventory.name, null, 0, 0);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Listening for second InventorySlot click
        /// </summary>
        protected virtual void CheckForSecondClick()
        {
            if (!_useKeyboardJoypad && _isMoving)
            {
                Inventory _inventoryTo = CurrentlySelectedInventorySlot.ParentInventoryDisplay.TargetInventory;
                if (_inventoryFrom == _inventoryTo)
                {
                    int _indexTo = CurrentlySelectedInventorySlot.Index;
                    if (_indexFrom != _indexTo)
                    {
                        MultiInventoryMove(false);
                    }
                }
                else
                {
                    MultiInventoryMove(false);
                }
            }
        }

        /// <summary>
        /// Move method supporting move between inventories
        /// </summary>
        protected virtual void MultiInventoryMove(bool keyboard)
        {
            //We have selected first slot and hit Move button
            if (_isMoving == false)
            {
                _slotFrom = CurrentlySelectedInventorySlot;
                _inventoryFrom = _slotFrom.ParentInventoryDisplay.TargetInventory;
                _indexFrom = _slotFrom.Index;
                _inventoryItemFrom = _inventoryFrom.Content[_indexFrom];
                //selected slot has an item in it so we gather rest of item data like item quantity and change sprite to Moved so we can see what happened
                if (_inventoryItemFrom != null)
                {
                    _quantityFrom = _inventoryItemFrom.Quantity;
                    //now we store none selected slot sprite to change it after moving
                    _oldSprite = CurrentlySelectedInventorySlot.GetComponent<Image>().sprite;
                    //now we change the slot sprite to Moved
                    _slotFrom.GetComponent<Image>().sprite = _slotFrom.MovedSprite;
                    //and by setting _isMoving to "true" we can wait for next click on slot
                    _isMoving = true;
                    if (keyboard)
                    {
                        _useKeyboardJoypad = true;
                    }
                }
            }
            //We have selected second slot
            else
            {
                _slotTo = CurrentlySelectedInventorySlot;
                _inventoryTo = _slotTo.ParentInventoryDisplay.TargetInventory;
                _indexTo = _slotTo.Index;
                _inventoryItemTo = _inventoryTo.Content[_indexTo];
                if (_inventoryTo.name == _inventoryFrom.name)
                {
                    if (_indexTo == _indexFrom)
                    {
                        return;
                    }
                }
                //second clicked slot is empty so we add item to this inventory, move to desired slot and remove it from first one
                if (_inventoryItemTo == null)
                {
                    //we change first selected slot sprite back to old sprite
                    _slotFrom.GetComponent<Image>().sprite = _oldSprite;
                    //we add item to selected inventory
                    _inventoryTo.Content[_indexTo] = _inventoryItemFrom.Copy();
                    //we remove item from first inventory
                    _inventoryFrom.DestroyItem(_indexFrom);
                    _inventoryFrom.Content[_indexFrom] = null;
                }
                else
                {
                    _quantityTo = _inventoryItemTo.Quantity;
                    //we check if item is the same as we try to move
                    if (_inventoryItemFrom.ItemName == _inventoryItemTo.ItemName)
                    {
                        //we check what is the rest free quantity to close the full sack
                        int _restStack = _inventoryItemTo.MaximumStack - _quantityTo;
                        //if quantity of our item we try to move in less or equal than free quantity
                        if (_quantityFrom <= _restStack)
                        {
                            //we update quantity of our item
                            _inventoryItemTo.Quantity = _inventoryItemTo.Quantity + _quantityFrom;
                            //we remove item from first inventory
                            _inventoryFrom.DestroyItem(_indexFrom);
                        }
                        //if quantity of our item we try to move is higher than free quantity
                        else if (_quantityFrom > _restStack)
                        {
                            //we update quantity of our item to full stack
                            _inventoryItemTo.Quantity = _inventoryItemTo.MaximumStack;
                            //we check what's left after moving some items
                            int _restToAddQuantity = _quantityFrom - _restStack;
                            //if there is free slot in second inventory
                            if (_inventoryTo.NumberOfFreeSlots >= 1)
                            {
                                //we need to find first free slot
                                int _fistFreeSlot = FindFirstFreeSlot(_inventoryTo);
                                //we add rest of item quantity to inventory as new item
                                _inventoryTo.Content[_fistFreeSlot] = _inventoryItemFrom;
                                _inventoryTo.Content[_fistFreeSlot].Quantity = _restToAddQuantity;
                                //we remove item from first inventory
                                _inventoryFrom.DestroyItem(_indexFrom);
                            }
                            //there is no free slots in second inventory
                            else
                            {
                                //we reduce item quantity in first inventory
                                _inventoryFrom.Content[_indexFrom].Quantity = _inventoryFrom.Content[_indexFrom].Quantity - _restStack;
                            }
                        }
                    }
                    else
                    {
                        //we need to create copy of item from first inventory
                        InventoryItem _tempItemFrom = _inventoryItemFrom.Copy();
                        //we need to create copy of item from second inventory
                        InventoryItem _tempItemTo = _inventoryItemTo.Copy();
                        //we remove item from first inventory
                        _inventoryFrom.DestroyItem(_indexFrom);
                        //we remove item from second inventory
                        _inventoryTo.DestroyItem(_indexTo);
                        //we add item from first inventory to second inventory
                        _inventoryTo.Content[_indexTo] = _tempItemFrom.Copy();
                        //we add item from second inventory to first inventory
                        _inventoryFrom.Content[_indexFrom] = _tempItemTo.Copy();
                    }
                }
                _isMoving = false;
                if (keyboard)
                {
                    _useKeyboardJoypad = false;
                }
                MMInventoryEvent.Trigger(MMInventoryEventType.ContentChanged, null, _inventoryFrom.name, null, 0, 0);
                MMInventoryEvent.Trigger(MMInventoryEventType.ContentChanged, null, _inventoryTo.name, null, 0, 0);
            }
        }

        /// <summary>
        /// Move method for Mouse input
        /// </summary>
        public virtual void MoveWithMouse()
        {
            if (!_inVendor)
            {
                _useKeyboardJoypad = false;
                MultiInventoryMove(false);
            }
        }

        /// <summary>
        /// Move method used by Keyboard or Joypad input
        /// </summary>
        public virtual void MoveWithKeyboardJoypad()
        {
            _useKeyboardJoypad = true;
            if (!_isMoving && _useKeyboardJoypad)
            {
                MultiInventoryMove(true);
            }
            else if (_isMoving && _useKeyboardJoypad)
            {
                MultiInventoryMove(true);
            }

        }

        #endregion

        #region ----------Vendor Methods----------

        /// <summary>
        /// Sell item or items to vendor 
        /// </summary>
        public virtual void SellItem()
        {
            Vendor openedVendor = null;
            Vendor[] vendors = FindObjectsOfType<Vendor>();
            foreach (Vendor vendor in vendors)
            {
                if (vendor.enabled && vendor.isOpen)
                {
                    openedVendor = vendor;
                }
            }
            if (openedVendor != null)
            {
                InventorySlot slot = CurrentlySelectedInventorySlot;
                Inventory inventory = slot.ParentInventoryDisplay.TargetInventory;
                int index = slot.Index;
                InventoryItem item = inventory.Content[index];
                InventoryDetailsExtended ide = FindObjectOfType<InventoryDetailsExtended>();
                if (ide != null && item != null && inventory.name == openedVendor.mainInventory.name)
                {
                    foreach (InventoryItem disabled in ide.disabledItems)
                    {
                        if (item.name == disabled.name)
                        {
                            return;
                        }
                    }
                    int quantity = Int32.Parse(ide.vendorQuantity.text);
                    int price = Int32.Parse(ide.vendorPrice.text);
                    //we can buy back items from vendor
                    if (openedVendor.canRollBackItems)
                    {
                        if (openedVendor.vendorRollBackInventory.AddItem(item, quantity))
                        {
                            if (openedVendor.mainInventory.AddItem(openedVendor.currencyItem, price))
                            {
                                //openedVendor.vendorRollBackInventory.AddItem(item, quantity);
                                MMInventoryEvent.Trigger(MMInventoryEventType.ContentChanged, null, openedVendor.vendorRollBackInventory.name, null, 0, 0);
                                if (quantity < item.Quantity)
                                {
                                    item.Quantity -= quantity;
                                }
                                else
                                {
                                    openedVendor.mainInventory.DestroyItem(index);
                                }
                                //openedVendor.GetCashForItem(price);
                                MMInventoryEvent.Trigger(MMInventoryEventType.ContentChanged, null, openedVendor.mainInventory.name, null, 0, 0);
                            }
                        }
                    }
                    //we can't buy back items from vendor
                    else
                    {
                        if (openedVendor.mainInventory.AddItem(openedVendor.currencyItem, price))
                        {
                            if (quantity < item.Quantity)
                            {
                                item.Quantity -= quantity;
                            }
                            else
                            {
                                openedVendor.mainInventory.DestroyItem(index);
                            }
                            //openedVendor.GetCashForItem(price);
                            MMInventoryEvent.Trigger(MMInventoryEventType.ContentChanged, null, openedVendor.mainInventory.name, null, 0, 0);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Buy item or items from vendor
        /// </summary>
        public virtual void BuyItem()
        {
            Vendor openedVendor = null;
            Vendor[] vendors = FindObjectsOfType<Vendor>();
            foreach (Vendor vendor in vendors)
            {
                if (vendor.enabled && vendor.isOpen)
                {
                    openedVendor = vendor;
                }
            }
            if (openedVendor != null)
            {
                InventorySlot slot = CurrentlySelectedInventorySlot;
                Inventory inventory = slot.ParentInventoryDisplay.TargetInventory;
                int index = slot.Index;
                InventoryItem item = inventory.Content[index];
                InventoryDetailsExtended ide = FindObjectOfType<InventoryDetailsExtended>();
                if (ide != null && item != null && ide.disabledItems.Contains(item) == false && (inventory.name == openedVendor.vendorInventory.name || inventory.name == openedVendor.vendorRollBackInventory.name))
                {
                    int quantity = Int32.Parse(ide.vendorQuantity.text);
                    int price = Int32.Parse(ide.vendorPrice.text);
                    int cash = openedVendor.GetPlayerCash();
                    if (cash >= price)
                    {
                        int freeSlot = FindFirstFreeSlot(openedVendor.mainInventory);
                        if (freeSlot != -1)
                        {
                            openedVendor.PayForItem(price);
                            openedVendor.mainInventory.Content[freeSlot] = item.Copy();
                            openedVendor.mainInventory.Content[freeSlot].Quantity = quantity;
                            MMInventoryEvent.Trigger(MMInventoryEventType.ContentChanged, null, openedVendor.mainInventory.name, null, 0, 0);

                        }
                        if (inventory.name == openedVendor.vendorInventory.name)
                        {
                            //vendor has limited stock
                            if (openedVendor.limitedStock)
                            {
                                if (quantity < item.Quantity)
                                {
                                    item.Quantity -= quantity;
                                }
                                else
                                {
                                    openedVendor.vendorInventory.DestroyItem(index);
                                }
                                MMInventoryEvent.Trigger(MMInventoryEventType.ContentChanged, null, openedVendor.vendorInventory.name, null, 0, 0);
                            }
                        }
                        else if (inventory.name == openedVendor.vendorRollBackInventory.name)
                        {
                            if (quantity < item.Quantity)
                            {
                                item.Quantity -= quantity;
                            }
                            else
                            {
                                openedVendor.vendorRollBackInventory.DestroyItem(index);
                            }
                            MMInventoryEvent.Trigger(MMInventoryEventType.ContentChanged, null, openedVendor.vendorRollBackInventory.name, null, 0, 0);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Increase item quantity and price to buy / sell
        /// </summary>
        public virtual void IncreaseQuantity()
        {
            Vendor openedVendor = null;
            Vendor[] vendors = FindObjectsOfType<Vendor>();
            foreach (Vendor vendor in vendors)
            {
                if (vendor.enabled && vendor.isOpen)
                {
                    openedVendor = vendor;
                }
            }
            if (openedVendor != null)
            {
                InventorySlot slot = CurrentlySelectedInventorySlot;
                Inventory inventory = slot.ParentInventoryDisplay.TargetInventory;
                int index = slot.Index;
                InventoryItem item = inventory.Content[index];
                if (item != null)
                {
                    if (_itemDetails != null)
                    {
                        if (inventory == openedVendor.mainInventory || inventory == openedVendor.vendorRollBackInventory)
                        {
                            int maxQuantity = item.Quantity;
                            int currentQuantity = Int32.Parse(_itemDetails.vendorQuantity.text);
                            int currentPrice = Int32.Parse(_itemDetails.vendorPrice.text);
                            if (currentQuantity < maxQuantity)
                            {
                                currentPrice += item.Price;
                                currentQuantity++;
                                _itemDetails.vendorQuantity.text = currentQuantity.ToString();
                                _itemDetails.vendorPrice.text = currentPrice.ToString();
                            }
                        }
                        else if (inventory == openedVendor.vendorInventory)
                        {
                            //vendor has limited stock
                            if (openedVendor.limitedStock)
                            {
                                int maxQuantity = item.Quantity;
                                int currentQuantity = Int32.Parse(_itemDetails.vendorQuantity.text);
                                int currentPrice = Int32.Parse(_itemDetails.vendorPrice.text);
                                if (currentQuantity < maxQuantity)
                                {
                                    currentPrice += openedVendor.vendorContent[index].Price;
                                    currentQuantity++;
                                    _itemDetails.vendorQuantity.text = currentQuantity.ToString();
                                    _itemDetails.vendorPrice.text = currentPrice.ToString();
                                }
                            }
                            //vendor has unlimited stock
                            else
                            {
                                int maxQuantity = item.MaximumStack;
                                int currentQuantity = Int32.Parse(_itemDetails.vendorQuantity.text);
                                int currentPrice = Int32.Parse(_itemDetails.vendorPrice.text);
                                if (currentQuantity < maxQuantity)
                                {
                                    currentPrice += openedVendor.vendorContent[index].Price;
                                    currentQuantity++;
                                    _itemDetails.vendorQuantity.text = currentQuantity.ToString();
                                    _itemDetails.vendorPrice.text = currentPrice.ToString();
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Decrease item quantity and price to buy / sell
        /// </summary>
        public virtual void DecreaseQuantity()
        {
            Vendor openedVendor = null;
            Vendor[] vendors = FindObjectsOfType<Vendor>();
            foreach (Vendor vendor in vendors)
            {
                if (vendor.enabled && vendor.isOpen)
                {
                    openedVendor = vendor;
                }
            }
            if (openedVendor != null)
            {
                InventorySlot slot = CurrentlySelectedInventorySlot;
                Inventory inventory = slot.ParentInventoryDisplay.TargetInventory;
                int index = slot.Index;
                InventoryItem item = inventory.Content[index];
                if (item != null)
                {
                    if (_itemDetails != null)
                    {
                        if (inventory == openedVendor.mainInventory || inventory == openedVendor.vendorRollBackInventory)
                        {
                            int currentQuantity = Int32.Parse(_itemDetails.vendorQuantity.text);
                            int currentPrice = Int32.Parse(_itemDetails.vendorPrice.text);
                            if (currentQuantity > 1)
                            {
                                currentPrice -= item.Price;
                                currentQuantity--;
                                _itemDetails.vendorQuantity.text = currentQuantity.ToString();
                                _itemDetails.vendorPrice.text = currentPrice.ToString();
                            }
                        }
                        else if (inventory == openedVendor.vendorInventory)
                        {
                            //vendor has limited stock
                            if (openedVendor.limitedStock)
                            {
                                int currentQuantity = Int32.Parse(_itemDetails.vendorQuantity.text);
                                int currentPrice = Int32.Parse(_itemDetails.vendorPrice.text);
                                if (currentQuantity > 1)
                                {
                                    currentPrice -= openedVendor.vendorContent[index].Price;
                                    currentQuantity--;
                                    _itemDetails.vendorQuantity.text = currentQuantity.ToString();
                                    _itemDetails.vendorPrice.text = currentPrice.ToString();
                                }
                            }
                            //vendor has unlimited stock
                            else
                            {
                                int currentQuantity = Int32.Parse(_itemDetails.vendorQuantity.text);
                                int currentPrice = Int32.Parse(_itemDetails.vendorPrice.text);
                                if (currentQuantity > 1)
                                {
                                    currentPrice -= openedVendor.vendorContent[index].Price;
                                    currentQuantity--;
                                    _itemDetails.vendorQuantity.text = currentQuantity.ToString();
                                    _itemDetails.vendorPrice.text = currentPrice.ToString();
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Opens vendor roll back inventory
        /// </summary>
        public virtual void OpenRollBackVendorInventory()
        {
            Vendor openedVendor = null;
            Vendor[] vendors = FindObjectsOfType<Vendor>();
            foreach (Vendor vendor in vendors)
            {
                if (vendor.enabled && vendor.isOpen)
                {
                    openedVendor = vendor;
                }
            }
            if (openedVendor != null)
            {
                openedVendor.vendorInventoryDisplay.gameObject.SetActive(false);
                openedVendor.vendorInventoryRollBackDisplay.gameObject.SetActive(true);
                openedVendor.SetupVendorRollBackDisplay();
                openedVendor.AddVendorRollBackDisplayToMainDisplay();
                openedVendor.isVendorRollBackOpen = true;
            }
        }

        /// <summary>
        /// Opens vendor inventory
        /// </summary>
        public virtual void OpenVendorInventory()
        {
            Vendor openedVendor = null;
            Vendor[] vendors = FindObjectsOfType<Vendor>();
            foreach (Vendor vendor in vendors)
            {
                if (vendor.enabled && vendor.isOpen)
                {
                    openedVendor = vendor;
                }
            }
            if (openedVendor != null)
            {
                openedVendor.vendorInventoryRollBackDisplay.gameObject.SetActive(false);
                openedVendor.vendorInventoryDisplay.gameObject.SetActive(true);
                openedVendor.SetupVendorDisplay();
                openedVendor.AddVendorDisplayToMainDisplay();
                openedVendor.isVendorRollBackOpen = false;
            }
        }

        #endregion

    }
}