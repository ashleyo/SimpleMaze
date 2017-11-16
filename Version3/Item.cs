using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Version4
{
    /// <summary>
    /// Represents items found in maze
    /// Rethunk.
    /// Let's keep all items in a dictionary keyed by Item.name - the Values should be <Item, GUID> objects where
    /// the GUID represents the owner
    /// </summary>
    class Item
    {
        public string Name { get; private set; }
        public string Description { get; set; }
        public int Weight { get; set; } // 0 -> weightless, nolimits 999 -> too heavy to lift
        public Item (string name, int weight, string description="a mysterious item")
        {
            Name = name; Weight = weight; Description = description;
        }
    }

    static class ItemBase
    {
        public static SortedDictionary<string, OwnedItem> Items = new SortedDictionary<string, OwnedItem>();
        public static void AddNewItem(OwnedItem item)
        {
            Items[item.Name] = item;
        }
        public static List<Item> GetItemsByOwner(Guid owner)
        {
            List<Item> items = new List<Item>();
            foreach (OwnedItem I in Items.Values)
            {
                if (I.Owner.Equals(owner)) items.Add(I);
            }
            return items;
        }
    }

    class OwnedItem: Item
    {
        public Guid Owner { get; set; }
        public OwnedItem(Guid owner, string name, int weight, string description = "a mysterious item") : base(name, weight, description)
        {
            Owner = owner;
        }
        public bool TransferTo(Guid newowner)
        {
            Owner = newowner;
            return true;
        }
    }
}
