using System;
using System.Collections.Generic;
using UnityEngine;

public class LG_Inventory : MonoBehaviour
{
    [System.Serializable]
    public struct InventoryItem
    {
        public string itemName;
        public int amount;

        public InventoryItem(string name, int qty)
        {
            itemName = name;
            amount = qty;
        }
    }

    [Header("Debug View")]
    [Tooltip("Visual list of items in the inspector for debugging purposes.")]
    [SerializeField] private List<InventoryItem> inspectorItems = new List<InventoryItem>();

    private Dictionary<string, int> itemDictionary = new Dictionary<string, int>();

    // Event triggered whenever an item is added or changed
    public event Action OnInventoryChanged;

    /// <summary>
    /// Adds a quantity of a specific item to the inventory.
    /// </summary>
    /// <param name="itemName">Name of the item.</param>
    /// <param name="amount">Amount to add (must be positive).</param>
    public void AddItem(string itemName, int amount)
    {
        if (amount <= 0) return;

        if (itemDictionary.ContainsKey(itemName))
        {
            itemDictionary[itemName] += amount;
        }
        else
        {
            itemDictionary.Add(itemName, amount);
        }

        SyncInspectorList();
        OnInventoryChanged?.Invoke();
    }

    /// <summary>
    /// Returns the quantity of a specific item in the inventory.
    /// </summary>
    public int GetItemCount(string itemName)
    {
        if (itemDictionary.TryGetValue(itemName, out int count))
        {
            return count;
        }
        return 0;
    }

    /// <summary>
    /// Returns a copy of the item dictionary.
    /// </summary>
    public Dictionary<string, int> GetItems()
    {
        return new Dictionary<string, int>(itemDictionary);
    }

    /// <summary>
    /// Helper to keep the Inspector List in sync with the Dictionary for debugging.
    /// </summary>
    private void SyncInspectorList()
    {
        inspectorItems.Clear();
        foreach (var kvp in itemDictionary)
        {
            inspectorItems.Add(new InventoryItem(kvp.Key, kvp.Value));
        }
    }
}
