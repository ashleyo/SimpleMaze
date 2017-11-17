using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using IList = System.Collections.Generic.List<Version4.Item>; //Would this make the code cleaner or just more obscure?

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
        public static List<Item> GetItemsByOwner(ObjectOwner owner)
        {
            // IList items = new IList();
            List<Item> items = new List<Item>();
            foreach (OwnedItem I in Items.Values)
            {
                if (I.OwnerID.Equals(owner.Id)) items.Add(I);
            }
            return items;
        }

        public static string Formatter(List<Item> items, bool withDescription=false)
        {
            if (items.Count == 0) return $"Nothing at all.\n";
            StringBuilder s = new StringBuilder();
            foreach (Item I in items)
            {
                Item item = I as Item;
                if (withDescription)
                    s.AppendLine($"{item.Name}: {item.Description}");
                else
                    s.AppendLine($"{item.Name}");
            }
            return s.ToString();   
        }

        public static List<Item> FindAllMatchingItems(ObjectOwner owner, string search)
        {
            // Get list of room items and remove from it any that do not match
            List<Item> present = ItemBase.GetItemsByOwner(owner);
            foreach (OwnedItem I in present.ToList<Item>())
                if (!Regex.Match(I.Name, search).Success) { present.Remove(I); }
            return present;
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
