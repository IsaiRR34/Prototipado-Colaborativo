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

    private Dictionary<string, int> itemDictionary = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

    public event Action OnInventoryChanged;

    private void Awake()
    {
        InitializeFromInspector();
    }

    public void InitializeFromInspector()
    {
        if (itemDictionary == null)
        {
            itemDictionary = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        }

        if (inspectorItems != null && inspectorItems.Count > 0)
        {
            foreach (var item in inspectorItems)
            {
                if (!string.IsNullOrEmpty(item.itemName) && item.amount > 0)
                {
                    string normName = NormalizeItemName(item.itemName);
                    if (itemDictionary.ContainsKey(normName))
                        itemDictionary[normName] += item.amount;
                    else
                        itemDictionary[normName] = item.amount;
                }
            }
            SyncInspectorList();
            OnInventoryChanged?.Invoke();
        }
    }

    public string NormalizeItemName(string name)
    {
        if (string.IsNullOrEmpty(name)) return name;
        string trimmed = name.Trim();

        // Normalización de Munición (Ammo, Municion, Munición 9mm, etc.)
        if (trimmed.IndexOf("Ammo", StringComparison.OrdinalIgnoreCase) >= 0 ||
            trimmed.IndexOf("Municion", StringComparison.OrdinalIgnoreCase) >= 0 ||
            trimmed.IndexOf("Munición", StringComparison.OrdinalIgnoreCase) >= 0 ||
            trimmed.Equals("Bala", StringComparison.OrdinalIgnoreCase) ||
            trimmed.Equals("Balas", StringComparison.OrdinalIgnoreCase))
        {
            return "Munición";
        }

        // Normalización de Curación (Vendas, Vendas Curativas, Botiquin, etc.)
        if (trimmed.IndexOf("Venda", StringComparison.OrdinalIgnoreCase) >= 0 ||
            trimmed.IndexOf("Curacion", StringComparison.OrdinalIgnoreCase) >= 0 ||
            trimmed.IndexOf("Curación", StringComparison.OrdinalIgnoreCase) >= 0 ||
            trimmed.IndexOf("Bandage", StringComparison.OrdinalIgnoreCase) >= 0 ||
            trimmed.IndexOf("Botiquin", StringComparison.OrdinalIgnoreCase) >= 0 ||
            trimmed.IndexOf("Botiquín", StringComparison.OrdinalIgnoreCase) >= 0)
        {
            return "Vendas";
        }

        // Normalización de Batería
        if (trimmed.IndexOf("Bateria", StringComparison.OrdinalIgnoreCase) >= 0 ||
            trimmed.IndexOf("Batería", StringComparison.OrdinalIgnoreCase) >= 0 ||
            trimmed.IndexOf("Battery", StringComparison.OrdinalIgnoreCase) >= 0)
        {
            return "Bateria";
        }

        return trimmed;
    }

    public void AddItem(string itemName, int amount)
    {
        if (amount <= 0) return;
        if (itemDictionary == null) InitializeFromInspector();

        itemName = NormalizeItemName(itemName);

        if (itemDictionary.ContainsKey(itemName))
            itemDictionary[itemName] += amount;
        else
            itemDictionary.Add(itemName, amount);

        SyncInspectorList();
        OnInventoryChanged?.Invoke();
    }

    public void RemoveItem(string itemName, int amount)
    {
        if (amount <= 0) return;
        if (itemDictionary == null) InitializeFromInspector();

        itemName = NormalizeItemName(itemName);

        if (itemDictionary.ContainsKey(itemName))
        {
            itemDictionary[itemName] -= amount;
            if (itemDictionary[itemName] <= 0)
            {
                itemDictionary.Remove(itemName);
            }
            SyncInspectorList();
            OnInventoryChanged?.Invoke();
        }
    }

    public int GetItemCount(string itemName)
    {
        if (itemDictionary == null || itemDictionary.Count == 0)
        {
            InitializeFromInspector();
        }

        itemName = NormalizeItemName(itemName);
        if (itemDictionary != null && itemDictionary.TryGetValue(itemName, out int count)) return count;
        return 0;
    }

    public Dictionary<string, int> GetItems()
    {
        if (itemDictionary == null || itemDictionary.Count == 0)
        {
            InitializeFromInspector();
        }

        return new Dictionary<string, int>(itemDictionary);
    }

    public bool HasKey(string keySubString = "llave")
    {
        if (string.IsNullOrEmpty(keySubString)) return true;
        if (GetItemCount(keySubString) > 0) return true;
        foreach (var kvp in itemDictionary)
        {
            if (kvp.Value > 0 && kvp.Key.IndexOf(keySubString, StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return true;
            }
        }
        return false;
    }

    public bool AddItem(ItemDataSO itemData, int amount)
    {
        if (itemData == null || amount <= 0) return false;
        AddItem(itemData.itemName, amount);
        return true;
    }

    private void Update()
    {
        // Tecla H o 4 para consumir curación estratégica (Vendas) según GDD
        if (Input.GetKeyDown(KeyCode.H) || Input.GetKeyDown(KeyCode.Alpha4))
        {
            UseHealingItem();
        }
    }

    public bool UseHealingItem()
    {
        string healItemKey = null;
        if (GetItemCount("Vendas") > 0) healItemKey = "Vendas";
        else if (GetItemCount("Vendas Curativas") > 0) healItemKey = "Vendas Curativas";
        else if (GetItemCount("Curacion") > 0) healItemKey = "Curacion";

        if (!string.IsNullOrEmpty(healItemKey))
        {
            LG_PlayerHealth pHealth = GetComponent<LG_PlayerHealth>();
            if (pHealth != null)
            {
                if (pHealth.GetCurrentHealth() >= pHealth.GetMaxHealth())
                {
                    LG_TooltipManager.Instance?.ShowTooltipTemporary("Salud al máximo", 1.2f);
                    return false;
                }

                RemoveItem(healItemKey, 1);
                pHealth.Heal(35f);

                if (SoundList.Instance != null) SoundList.Instance.PlaySound("SFX_Pickup_Battery");
                LG_TooltipManager.Instance?.ShowTooltipTemporary("<color=#10B981><b>¡Vendas aplicadas! (+35 Salud)</b></color>", 2f);
                return true;
            }
        }
        else
        {
            LG_TooltipManager.Instance?.ShowTooltipTemporary("No tienes vendas en el inventario", 1.2f);
        }

        return false;
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
