using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class LG_HUD : MonoBehaviour
{
    [Header("Components to Monitor")]
    [SerializeField] private RIMovement playerMovement;
    [SerializeField] private LG_Inventory playerInventory;
    [SerializeField] private LG_PlayerHealth playerHealth;
    [SerializeField] private Hand playerHand;

    [Header("UI Elements")]
    [SerializeField] private Slider staminaSlider;
    [SerializeField] private Slider healthSlider;
    [SerializeField] private Text inventoryText;
    [SerializeField] private Text equipmentStatusText;

    private void Start()
    {
        if (playerMovement == null) playerMovement = Object.FindFirstObjectByType<RIMovement>();
        if (playerInventory == null) playerInventory = Object.FindFirstObjectByType<LG_Inventory>();
        if (playerHealth == null) playerHealth = Object.FindFirstObjectByType<LG_PlayerHealth>();
        if (playerHand == null) playerHand = Object.FindFirstObjectByType<Hand>();

        // Auto-detectar o buscar InventoryText en el Canvas si no está enlazado
        if (inventoryText == null)
        {
            GameObject textGO = GameObject.Find("InventoryText");
            if (textGO != null) inventoryText = textGO.GetComponent<Text>();
        }

        if (playerInventory != null)
        {
            playerInventory.OnInventoryChanged += UpdateInventoryUI;
            UpdateInventoryUI();
        }
    }

    private void OnDestroy()
    {
        if (playerInventory != null)
        {
            playerInventory.OnInventoryChanged -= UpdateInventoryUI;
        }
    }

    private void Update()
    {
        if (staminaSlider != null && playerMovement != null)
        {
            staminaSlider.value = playerMovement.EstaminaNormalizada();
        }

        if (healthSlider != null && playerHealth != null)
        {
            healthSlider.value = playerHealth.GetHealthNormalized();
        }

        UpdateEquipmentUI();
    }

    private void UpdateEquipmentUI()
    {
        if (equipmentStatusText == null) return;
        if (playerHand == null) return;

        int batPercent = Mathf.RoundToInt(playerHand.BatDurabilityNormalized * 100f);
        int torchPercent = Mathf.RoundToInt(playerHand.TorchFuelNormalized * 100f);
        string torchState = playerHand.IsTorchOn ? "<color=#FBBF24>[ON]</color>" : "<color=#64748B>[OFF]</color>";

        equipmentStatusText.text = $"<b>EQUIPO:</b> Bate: <color=#38BDF8>{batPercent}%</color> | Linterna [F]: {torchState} <color=#FBBF24>{torchPercent}%</color>";
    }

    /// <summary>
    /// Muestra el listado de ítems en el inventario con el formato original y claro en pantalla.
    /// </summary>
    public void UpdateInventoryUI()
    {
        if (inventoryText == null) return;

        if (playerInventory == null)
        {
            inventoryText.text = "<b>INVENTARIO</b>\n<i>No disponible</i>";
            return;
        }

        Dictionary<string, int> items = playerInventory.GetItems();

        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        sb.AppendLine("<b>INVENTARIO</b>");
        sb.AppendLine("=================");

        if (items.Count == 0)
        {
            sb.AppendLine("<i>Vacío</i>");
        }
        else
        {
            foreach (var kvp in items)
            {
                sb.AppendLine($"• {kvp.Key}: {kvp.Value}");
            }
        }

        sb.AppendLine("=================");
        sb.AppendLine("<size=13><color=#94A3B8>[H] Curar | [R] Reparar | [F] Linterna | [V] Melee</color></size>");

        inventoryText.text = sb.ToString();
    }
}
