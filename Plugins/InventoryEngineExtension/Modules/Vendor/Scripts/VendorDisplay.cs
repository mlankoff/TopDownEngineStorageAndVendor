using System.Collections.Generic;
using MoreMountains.Tools;
using UnityEngine;
using UnityEngine.UI;


namespace MoreMountains.InventoryEngine
{
    public class VendorDisplay : InventoryDisplay
    {
        /// <summary>
        /// Change vendor display title if existed
        /// </summary>
        /// <param name="title"></param>
        public virtual void ChangeDisplayTitle(string title)
        {
            GameObject titleObject = GetComponentInChildren<InventoryDisplayTitle>().gameObject;
            if (title != null && titleObject.name == "InventoryTitle")
            {
                GetComponentInChildren<InventoryDisplayTitle>().text = title;
            }
            RedrawInventoryDisplay();
        }

        /// <summary>
        /// Resize vendor inventory title size to fit display size
        /// </summary>
        protected virtual void ResizeInventoryTitle()
        {
            float newWidth = PaddingLeft + SlotSize.x * NumberOfColumns + SlotMargin.x * (NumberOfColumns - 1) + PaddingRight;
            float newHeight = PaddingTop + SlotSize.y * NumberOfRows + SlotMargin.y * (NumberOfRows - 1) + PaddingBottom;
            Vector2 newSize = new Vector2(newWidth, newHeight);
            this.GetComponentInChildren<InventoryDisplayTitle>().gameObject.GetComponent<RectTransform>().sizeDelta = newSize;
        }

        /// <summary>
        /// Setup inventory display to fit vendor inventory with resizing title
        /// </summary>
        public override void SetupInventoryDisplay()
        {
            base.SetupInventoryDisplay();
            GameObject title = GetComponentInChildren<InventoryDisplayTitle>().gameObject;
            if (title != null && title.name == "InventoryTitle")
            {
                ResizeInventoryTitle();
            }
            RedrawInventoryDisplay();
        }

        /// <summary>
        /// Set _targetInventory to new Inventory
        /// </summary>
        /// <param name="targetInventory"></param>
        public virtual void SetTargetInventory(Inventory targetInventory)
        {
            _targetInventory = targetInventory;
        }

        protected override void RedrawInventoryDisplay()
        {
            base.RedrawInventoryDisplay();
            foreach (var slot in SlotContainer)
            {
                slot.MMGetComponentNoAlloc<Image>().enabled = true;
                slot.MMGetComponentNoAlloc<InventorySlot>().enabled = true;
                slot.MMGetComponentNoAlloc<CanvasGroup>().enabled = true;
            }
        }

        protected override void Awake()
        {
            _contentLastUpdate = new List<InventoryItem>();
            SlotContainer = new List<GameObject>();
            _comparison = new List<int>();
        }
    }
}


