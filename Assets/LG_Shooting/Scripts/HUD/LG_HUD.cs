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
            playerMovement = Object.FindFirstObjectByType<RIMovement>();
        }

        if (playerInventory == null)
        {
            playerInventory = Object.FindFirstObjectByType<LG_Inventory>();
        }

        if (playerHealth == null)
        {
            playerHealth = Object.FindFirstObjectByType<LG_PlayerHealth>();
        }

        if (healthSlider == null)
        {
            GameObject hGo = GameObject.Find("HealthSlider");
            if (hGo == null) hGo = GameObject.Find("HPSlider");
            if (hGo != null) healthSlider = hGo.GetComponent<Slider>();
            if (healthSlider == null)
            {
                Slider[] sliders = GetComponentsInChildren<Slider>(true);
                foreach (var s in sliders)
                {
                    string sName = s.gameObject.name.ToLower();
                    if (sName.Contains("health") || sName.Contains("hp"))
                    {
                        healthSlider = s;
                        break;
                    }
                }
            }
        }

        if (staminaSlider == null)
        {
            GameObject sGo = GameObject.Find("StaminaSlider");
            if (sGo != null) staminaSlider = sGo.GetComponent<Slider>();
            if (staminaSlider == null)
            {
                Slider[] sliders = GetComponentsInChildren<Slider>(true);
                foreach (var s in sliders)
                {
                    if (s.gameObject.name.ToLower().Contains("stamina"))
                    {
                        staminaSlider = s;
                        break;
                    }
                }
            }
        }

        if (inventoryText == null)
        {
            GameObject invGo = GameObject.Find("InventoryText");
            if (invGo != null) inventoryText = invGo.GetComponent<Text>();
            if (inventoryText == null)
            {
                Text[] texts = GetComponentsInChildren<Text>(true);
                foreach (var t in texts)
                {
                    if (t.gameObject.name.ToLower().Contains("invent"))
                    {
                        inventoryText = t;
                        break;
                    }
                }
            }
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
