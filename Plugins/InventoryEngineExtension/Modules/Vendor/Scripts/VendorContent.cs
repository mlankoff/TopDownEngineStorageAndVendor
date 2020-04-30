using MoreMountains.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.InventoryEngine
{
    [System.Serializable]
    public class VendorContent
    {
        public string name;
        public InventoryItem Item;
        public int Price;
        [MMInformation("If 'Use Limited Stock' is checked vendor will have only that quantity of item", MMInformationAttribute.InformationType.Info, false)]
        public int Quantity;
    }
}

