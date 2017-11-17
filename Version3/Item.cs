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
        public static List<OwnedItem> GetItemsByOwner(ObjectOwner owner)
        {
            List<OwnedItem> items = new List<OwnedItem>();
            foreach (OwnedItem I in Items.Values)
            {
                if (I.OwnerID.Equals(owner.Id)) items.Add(I);
            }
            return items;
        }

        public static string Formatter<T>(List<T> items)
        {
            if (items.Count == 0) return $"Nothing at all.\n";
            StringBuilder s = new StringBuilder();
            foreach (T I in items)
            {
                Item item = I as Item;
                s.AppendLine($"{item.Name}");
            }
            return s.ToString();   
        }
    }

    class OwnedItem: Item
    {
        public Guid OwnerID { get; set; }
        public OwnedItem(ObjectOwner owner, string name, int weight, string description = "a mysterious item") : base(name, weight, description)
        {
            OwnerID = owner.Id;
        }
        public bool TransferTo(ObjectOwner newowner)
        {
            OwnerID = newowner.Id;
            return true;
        }
    }
}
