using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class LG_HUD : MonoBehaviour
{
    [Header("Components to Monitor")]
    [SerializeField] private RIMovement playerMovement;
    [SerializeField] private LG_Inventory playerInventory;
    [SerializeField] private LG_PlayerHealth playerHealth;

    [Header("UI Elements")]
    [SerializeField] private Slider staminaSlider;
    [SerializeField] private Slider healthSlider;
    [SerializeField] private Text inventoryText;

    private void Start()
    {
        // Try finding components automatically if not assigned in Inspector
        if (playerMovement == null)
        {
            playerMovement = FindObjectOfType<RIMovement>();
        }

        if (playerInventory == null)
        {
            playerInventory = FindObjectOfType<LG_Inventory>();
        }

        if (playerHealth == null)
        {
            playerHealth = FindObjectOfType<LG_PlayerHealth>();
        }

        // Subscribe to inventory update events
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
        // Update Stamina Slider in real time
        if (staminaSlider != null && playerMovement != null)
        {
            staminaSlider.value = playerMovement.EstaminaNormalizada();
        }

        // Update Health Slider in real time
        if (healthSlider != null && playerHealth != null)
        {
            healthSlider.value = playerHealth.GetHealthNormalized();
        }
    }

    /// <summary>
    /// Compiles a list of items currently in inventory and displays them on screen.
    /// </summary>
    private void UpdateInventoryUI()
    {
        if (inventoryText == null) return;

        if (playerInventory == null)
        {
            inventoryText.text = "Inventario: No disponible";
            return;
        }

        Dictionary<string, int> items = playerInventory.GetItems();

        if (items.Count == 0)
        {
            inventoryText.text = "<b>INVENTARIO</b>\n<i>Vacío</i>";
            return;
        }

        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        sb.AppendLine("<b>INVENTARIO</b>");
        sb.AppendLine("=================");

        foreach (var kvp in items)
        {
            sb.AppendLine($"• {kvp.Key}: {kvp.Value}");
        }

        inventoryText.text = sb.ToString();
    }
}
