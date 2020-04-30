using MoreMountains.Tools;

namespace MoreMountains.InventoryEngine
{
    [System.Serializable]
    public class Loot
    {
        [MMInformation("Item to spawn", MMInformationAttribute.InformationType.Info, false)]
        public InventoryItem item;

        [MMInformation("Item quantity to spawn. If given quantity is higher than max stack it will spawn only max posible stack", MMInformationAttribute.InformationType.Info, false)]
        public int quantity;

        [MMInformation("If 'Random Loot' is checked, it will spawn item with random quantity less or equal 'Max Quantity'", MMInformationAttribute.InformationType.Info, false)]
        public int maxQuantity;

        [MMInformation("If 'Random Loot' is checked, it will look for items with higher drop chance. For example item with 100 drop chance will have higher chance to spawn than item with 1 drop chance. Minimal value is 1 and Maximal value is 100", MMInformationAttribute.InformationType.Info, false)]
        public int dropChance;
    }
}

