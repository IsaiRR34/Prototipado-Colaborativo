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
    [SerializeField] private List<InventoryItem> inspectorItems = new List<InventoryItem>();

    private Dictionary<string, int> itemDictionary = new Dictionary<string, int>();

    public event Action OnInventoryChanged;

    public void AddItem(string itemName, int amount)
    {
        if (amount <= 0) return;

        if (itemDictionary.ContainsKey(itemName))
            itemDictionary[itemName] += amount;
        else
            itemDictionary.Add(itemName, amount);

        SyncInspectorList();
        OnInventoryChanged?.Invoke();
    }

    // NUEVA FUNCIÓN: Necesaria para consumir munición o curas
    public void RemoveItem(string itemName, int amount)
    {
        if (amount <= 0) return;

        if (itemDictionary.ContainsKey(itemName))
        {
            itemDictionary[itemName] -= amount;
            if (itemDictionary[itemName] <= 0)
            {
                itemDictionary.Remove(itemName); // Si se acaban, lo quitamos del diccionario
            }
            SyncInspectorList();
            OnInventoryChanged?.Invoke();
        }
    }

    public int GetItemCount(string itemName)
    {
        if (itemDictionary.TryGetValue(itemName, out int count)) return count;
        return 0;
    }

    public Dictionary<string, int> GetItems()
    {
        return new Dictionary<string, int>(itemDictionary);
    }

    private void SyncInspectorList()
    {
        inspectorItems.Clear();
        foreach (var kvp in itemDictionary)
        {
            inspectorItems.Add(new InventoryItem(kvp.Key, kvp.Value));
        }
    }
}