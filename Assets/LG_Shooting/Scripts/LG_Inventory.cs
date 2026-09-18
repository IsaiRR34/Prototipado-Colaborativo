using System;
using System.Collections.Generic;
using UnityEngine;

public class LG_Inventory : MonoBehaviour
{
    [System.Serializable]
    public class InventorySlot
    {
        public ItemDataSO itemData;
        public int amount;

        public InventorySlot(ItemDataSO data, int qty)
        {
            itemData = data;
            amount = qty;
        }
    }

    [Header("Configuración de Ranuras Fijas")]
    [SerializeField] private int maxSlots = 8;
    [SerializeField] private List<InventorySlot> slots = new List<InventorySlot>();

    [Header("Referencias del Jugador")]
    [SerializeField] private LG_PlayerHealth playerHealth;

    public event Action OnInventoryChanged;

    public int MaxSlots => maxSlots;
    public IReadOnlyList<InventorySlot> Slots => slots;

    private void Awake()
    {
        if (playerHealth == null) playerHealth = GetComponent<LG_PlayerHealth>();
        if (playerHealth == null) playerHealth = GetComponentInParent<LG_PlayerHealth>();
    }

    private void Update()
    {
        // Tecla rápida 'H' para consumir vendas/curación del inventario
        if (Input.GetKeyDown(KeyCode.H))
        {
            UseFirstHealingItem();
        }
    }

    /// <summary>
    /// Añade un ítem gestionado mediante ItemDataSO respetando las ranuras fijas y stacks.
    /// </summary>
    public bool AddItem(ItemDataSO item, int amount)
    {
        if (item == null || amount <= 0) return false;

        // 1. Intentar apilar en ranuras existentes que coincidan y no estén llenas
        int remaining = amount;
        for (int i = 0; i < slots.Count; i++)
        {
            if (slots[i].itemData == item)
            {
                int canAdd = item.maxStack - slots[i].amount;
                if (canAdd > 0)
                {
                    int toAdd = Mathf.Min(canAdd, remaining);
                    slots[i].amount += toAdd;
                    remaining -= toAdd;

                    if (remaining <= 0)
                    {
                        OnInventoryChanged?.Invoke();
                        return true;
                    }
                }
            }
        }

        // 2. Si sobra cantidad, intentar crear nuevas ranuras hasta el límite maxSlots
        while (remaining > 0)
        {
            if (slots.Count >= maxSlots)
            {
                // No hay más ranuras disponibles en el inventario
                Debug.LogWarning($"[LG_Inventory] Inventario lleno ({slots.Count}/{maxSlots}). No se pudo añadir {remaining}x {item.itemName}.");
                OnInventoryChanged?.Invoke();
                return false;
            }

            int toSlot = Mathf.Min(remaining, item.maxStack);
            slots.Add(new InventorySlot(item, toSlot));
            remaining -= toSlot;
        }

        OnInventoryChanged?.Invoke();
        return true;
    }

    /// <summary>
    /// Remueve una cantidad del ítem especificado. Respeta la regla de que las llaves maestras permanentes no pueden desecharse.
    /// </summary>
    public bool RemoveItem(ItemDataSO item, int amount)
    {
        if (item == null || amount <= 0) return false;

        // Si es un ítem permanente (Llave Maestra), el GDD y Edit prohíben descartarlo
        if (item.isPermanent)
        {
            Debug.Log($"[LG_Inventory] El ítem '{item.itemName}' es permanente (sello/llave) y no puede ser removido del inventario.");
            return false;
        }

        int remaining = amount;
        for (int i = slots.Count - 1; i >= 0; i--)
        {
            if (slots[i].itemData == item)
            {
                if (slots[i].amount <= remaining)
                {
                    remaining -= slots[i].amount;
                    slots.RemoveAt(i);
                }
                else
                {
                    slots[i].amount -= remaining;
                    remaining = 0;
                    break;
                }
            }
        }

        OnInventoryChanged?.Invoke();
        return remaining == 0;
    }

    /// <summary>
    /// Remueve ítem por ID o nombre (compatibilidad con scripts existentes)
    /// </summary>
    public bool RemoveItem(string itemName, int amount)
    {
        if (amount <= 0 || string.IsNullOrEmpty(itemName)) return false;

        for (int i = slots.Count - 1; i >= 0; i--)
        {
            var slot = slots[i];
            if (slot.itemData != null && (slot.itemData.itemName.Equals(itemName, StringComparison.OrdinalIgnoreCase) ||
                                          slot.itemData.itemID.Equals(itemName, StringComparison.OrdinalIgnoreCase)))
            {
                if (slot.itemData.isPermanent)
                {
                    Debug.Log($"[LG_Inventory] Llave/ítem permanente '{slot.itemData.itemName}' no puede eliminarse.");
                    return false;
                }

                if (slot.amount <= amount)
                {
                    amount -= slot.amount;
                    slots.RemoveAt(i);
                }
                else
                {
                    slot.amount -= amount;
                    amount = 0;
                }

                if (amount <= 0) break;
            }
        }

        OnInventoryChanged?.Invoke();
        return true;
    }

    /// <summary>
    /// Añade ítem por nombre (método de compatibilidad para código legacy)
    /// </summary>
    public void AddItem(string legacyName, int amount)
    {
        if (amount <= 0) return;

        // Buscar si existe un slot con ese nombre
        foreach (var slot in slots)
        {
            if (slot.itemData != null && slot.itemData.itemName.Equals(legacyName, StringComparison.OrdinalIgnoreCase))
            {
                AddItem(slot.itemData, amount);
                return;
            }
        }

        // Crear ItemDataSO dinámico en memoria si no existiera
        ItemDataSO dynamicSO = ScriptableObject.CreateInstance<ItemDataSO>();
        dynamicSO.itemName = legacyName;
        dynamicSO.itemID = legacyName.ToLower().Replace(" ", "_");
        if (legacyName.Contains("muni", StringComparison.OrdinalIgnoreCase) || legacyName.Contains("ammo", StringComparison.OrdinalIgnoreCase))
            dynamicSO.itemType = ItemType.Municion;
        else if (legacyName.Contains("llave", StringComparison.OrdinalIgnoreCase) || legacyName.Contains("key", StringComparison.OrdinalIgnoreCase))
        {
            dynamicSO.itemType = ItemType.LlaveMaestra;
            dynamicSO.isPermanent = true;
        }
        else if (legacyName.Contains("cura", StringComparison.OrdinalIgnoreCase) || legacyName.Contains("venda", StringComparison.OrdinalIgnoreCase))
            dynamicSO.itemType = ItemType.Curacion;
        else
            dynamicSO.itemType = ItemType.Recurso;

        AddItem(dynamicSO, amount);
    }

    public int GetItemCount(string itemName)
    {
        if (string.IsNullOrEmpty(itemName)) return 0;
        int count = 0;
        foreach (var slot in slots)
        {
            if (slot.itemData != null && (slot.itemData.itemName.Equals(itemName, StringComparison.OrdinalIgnoreCase) ||
                                          slot.itemData.itemID.Equals(itemName, StringComparison.OrdinalIgnoreCase)))
            {
                count += slot.amount;
            }
        }
        return count;
    }

    public int GetItemCount(ItemDataSO item)
    {
        if (item == null) return 0;
        int count = 0;
        foreach (var slot in slots)
        {
            if (slot.itemData == item) count += slot.amount;
        }
        return count;
    }

    /// <summary>
    /// Cuenta total de munición disponible en todas las ranuras del inventario.
    /// </summary>
    public int GetTotalAmmoCount()
    {
        int total = 0;
        foreach (var slot in slots)
        {
            if (slot.itemData != null && slot.itemData.itemType == ItemType.Municion)
            {
                total += slot.amount;
            }
        }
        return total;
    }

    /// <summary>
    /// Consume munición directa de las ranuras (estilo DOOM).
    /// </summary>
    public bool ConsumeAmmo(int amount)
    {
        if (amount <= 0) return false;
        if (GetTotalAmmoCount() < amount) return false;

        int toRemove = amount;
        for (int i = slots.Count - 1; i >= 0; i--)
        {
            if (slots[i].itemData != null && slots[i].itemData.itemType == ItemType.Municion)
            {
                if (slots[i].amount <= toRemove)
                {
                    toRemove -= slots[i].amount;
                    slots.RemoveAt(i);
                }
                else
                {
                    slots[i].amount -= toRemove;
                    toRemove = 0;
                    break;
                }
            }
        }

        OnInventoryChanged?.Invoke();
        return true;
    }

    /// <summary>
    /// Usa la primera venda / cura encontrada en el inventario.
    /// </summary>
    public bool UseFirstHealingItem()
    {
        if (playerHealth == null) playerHealth = GetComponent<LG_PlayerHealth>();
        if (playerHealth == null) playerHealth = GetComponentInParent<LG_PlayerHealth>();

        if (playerHealth != null && playerHealth.GetCurrentHealth() >= playerHealth.GetMaxHealth())
        {
            LG_TooltipManager.Instance?.ShowTooltipTemporary("Salud al máximo", 1.2f);
            return false;
        }

        for (int i = 0; i < slots.Count; i++)
        {
            if (slots[i].itemData != null && slots[i].itemData.itemType == ItemType.Curacion)
            {
                ItemDataSO healItem = slots[i].itemData;
                float healAmount = healItem.healAmount > 0 ? healItem.healAmount : 30f;

                if (playerHealth != null)
                {
                    playerHealth.Heal(healAmount);
                }

                slots[i].amount--;
                if (slots[i].amount <= 0)
                {
                    slots.RemoveAt(i);
                }

                LG_TooltipManager.Instance?.ShowTooltipTemporary($"Usada {healItem.itemName} (+{healAmount} HP)", 1.5f);
                if (SoundList.Instance != null) SoundList.Instance.PlaySound("SFX_Pickup");

                OnInventoryChanged?.Invoke();
                return true;
            }
        }

        LG_TooltipManager.Instance?.ShowTooltipTemporary("No tienes curaciones", 1.2f);
        return false;
    }

    /// <summary>
    /// Consume recursos del inventario para reparar herramientas (bate/antorcha).
    /// </summary>
    public float ConsumeRepairResource(float maxNeeded)
    {
        for (int i = 0; i < slots.Count; i++)
        {
            if (slots[i].itemData != null && slots[i].itemData.itemType == ItemType.Recurso)
            {
                float repairVal = slots[i].itemData.repairAmount > 0 ? slots[i].itemData.repairAmount : 50f;
                slots[i].amount--;
                if (slots[i].amount <= 0)
                {
                    slots.RemoveAt(i);
                }

                OnInventoryChanged?.Invoke();
                return repairVal;
            }
        }
        return 0f;
    }

    /// <summary>
    /// Comprueba si el jugador posee una llave por coincidencia de ID o nombre.
    /// </summary>
    public bool HasKey(string keyIdentifier)
    {
        if (string.IsNullOrEmpty(keyIdentifier)) return false;

        foreach (var slot in slots)
        {
            if (slot.itemData != null && slot.itemData.itemType == ItemType.LlaveMaestra)
            {
                if (slot.itemData.itemName.IndexOf(keyIdentifier, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    slot.itemData.itemID.IndexOf(keyIdentifier, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return true;
                }
            }
        }
        return false;
    }

    public Dictionary<string, int> GetItems()
    {
        Dictionary<string, int> dict = new Dictionary<string, int>();
        foreach (var slot in slots)
        {
            if (slot.itemData == null) continue;
            if (dict.ContainsKey(slot.itemData.itemName))
                dict[slot.itemData.itemName] += slot.amount;
            else
                dict.Add(slot.itemData.itemName, slot.amount);
        }
        return dict;
    }
}
