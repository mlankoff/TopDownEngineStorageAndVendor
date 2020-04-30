using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using MoreMountains.Tools;

namespace MoreMountains.InventoryEngine
{
    /// <summary>
    /// A class used to display an item's details in GUI
    /// </summary>
    public class InventoryDetailsExtended : InventoryDetails
    {
        [Header("Multi Inventory Details Setup")]
        [MMInformation("If checked then this display will handle every active inventory you have on scene. If you want to use vendor or storage it must be checked.", MMInformationAttribute.InformationType.Info, false)]
        public bool multiInventory = true;

        [MMInformation("If Multi Inventory is checked those inventories won't be handled. Put here exact Inventory Name you want to disable. Case sensitive", MMInformationAttribute.InformationType.Info, false)]
        public string[] disabledInventories;

        #region -------------- Vendor variables (If you don't want vendor you can delete this) ----------------

        [Header("Vendor UI Setup")]
        public GameObject vendorUI;
        public GameObject sellButton;
        public GameObject buyButton;
        public GameObject decreaseButton;
        public GameObject increaseButton;
        public Text vendorPrice;
        public Text vendorQuantity;
        public Image currencyImage;

        [Header("Vendor Disable Buy / Sell Items")]
        public InventoryItem[] disabledItems;

        #endregion

        /// <summary>
        /// Catches MMInventoryEvents and displays details if needed
        /// </summary>
        /// <param name="inventoryEvent">Inventory event.</param>
        public override void OnMMEvent(MMInventoryEvent inventoryEvent)
        {
            if (multiInventory == false)
            {
                // if this event doesn't concern our inventory display, we do nothing and exit
                if (inventoryEvent.TargetInventoryName != this.TargetInventoryName)
                {
                    return;
                }
            }
            else
            {
                foreach (string disabledInv in disabledInventories)
                {
                    if (inventoryEvent.TargetInventoryName == disabledInv)
                    {
                        return;
                    }
                }
            }
            switch (inventoryEvent.InventoryEventType)
            {
                case MMInventoryEventType.Select:
                    DisplayDetails(inventoryEvent.EventItem);
                    break;
                case MMInventoryEventType.InventoryOpens:
                    DisplayDetails(inventoryEvent.EventItem);
                    break;
            }
        }

        /// <summary>
		/// Fills the various detail fields with the item's metadata
		/// </summary>
		/// <returns>The detail fields.</returns>
		/// <param name="item">Item.</param>
		/// <param name="initialDelay">Initial delay.</param>
		protected override IEnumerator FillDetailFields(InventoryItem item, float initialDelay)
        {
            yield return new WaitForSeconds(initialDelay);
            if (Title != null) { Title.text = item.ItemName; }
            if (ShortDescription != null) { ShortDescription.text = item.ShortDescription; }
            if (Description != null) { Description.text = item.Description; }
            if (Quantity != null) { Quantity.text = item.Quantity.ToString(); }
            if (Icon != null) { Icon.sprite = item.Icon; }

            #region ------------ Vendor UI (If you don't want vendor you can delete this) ---------------

            Vendor openedVendor = null;
            //we get all objects with Vendor script
            Vendor[] vendors = FindObjectsOfType<Vendor>();
            //we loop all Vendors to see if any of them is enabled
            foreach (Vendor vendor in vendors)
            {
                //if vendor is enabled
                if (vendor.enabled == true && vendor.isOpen)
                {
                    openedVendor = vendor;
                }
            }
            //we check if our vendor is not null
            if (openedVendor != null && openedVendor.isOpen)
            {
                //we are searching for InventoryInputManagerExtended
                InventoryInputManagerExtended iime = FindObjectOfType<InventoryInputManagerExtended>();
                //and if it's not null
                if (iime != null)
                {
                    //we get currently selected slot and data like inventory, index and item
                    InventorySlot _slot = iime.CurrentlySelectedInventorySlot;
                    Inventory _inventory = _slot.ParentInventoryDisplay.TargetInventory;
                    int _index = _slot.Index;
                    InventoryItem _item = _inventory.Content[_index];
                    //we loop over all disabled items
                    foreach (InventoryItem disabledItem in disabledItems)
                    {
                        //if item is not null
                        if (_item != null)
                        {
                            //and item selected item is disabled item we update item display and hide vendor UI to prevent sell / buy
                            if (disabledItem.name == _item.name)
                            {
                                openedVendor.UpdateVendorItemDisplay();
                                vendorUI.SetActive(false);
                            }
                            //if item is not in disabled we can update item display and show vendor UI
                            else
                            {
                                openedVendor.UpdateVendorItemDisplay();
                                vendorUI.SetActive(true);
                            }
                        }
                    }
                }
            }
            #endregion
        }
    }
}