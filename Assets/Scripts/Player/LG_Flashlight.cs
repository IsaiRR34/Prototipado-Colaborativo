using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class LG_Flashlight : MonoBehaviour
{
    [Header("Referencias de Linterna")]
    [Tooltip("El componente Light de la linterna (el foco real)")]
    [SerializeField] private Light spotLight;

    [Header("Batería")]
    [SerializeField] private float maxBattery = 100f;
    [SerializeField] private float batteryDrainPerSecond = 1.5f;
    private float currentBattery;

    [Header("Recarga")]
    [SerializeField] private LG_Inventory inventory;
    [SerializeField] private float rechargeAmount = 50f;
    [SerializeField] private string batteryItemName = "Bateria";

    [Header("UI Batería")]
    [SerializeField] private Slider batterySlider;

    private Hand playerHand;

    private void Start()
    {
        currentBattery = maxBattery;
        if (inventory == null) inventory = GetComponent<LG_Inventory>();

        // Obtenemos el script Hand automáticamente
        playerHand = GetComponent<Hand>();

        UpdateUI();
    }

    private void Update()
    {
        // 1. Verificamos si la linterna está equipada leyendo la variable flashLight directamente del script Hand
        bool isEquipped = false;
        if (playerHand != null && playerHand.flashLight != null)
        {
            isEquipped = playerHand.flashLight.activeInHierarchy;
        }

        // 2. Lógica de drenado y apagado automático
        if (isEquipped)
        {
            if (currentBattery > 0)
            {
                currentBattery -= batteryDrainPerSecond * Time.deltaTime;
                if (spotLight != null) spotLight.enabled = true;
            }
            else
            {
                currentBattery = 0;
                if (spotLight != null) spotLight.enabled = false; // Se apaga sola por falta de pila
            }
            UpdateUI();
        }
        else
        {
            // Si cambiamos de arma (pistola o bate), apagamos el foco por completo y NO gastamos pila
            if (spotLight != null) spotLight.enabled = false;
        }

        // 3. Recarga con F
        if (Keyboard.current != null && Keyboard.current.fKey.wasPressedThisFrame)
        {
            TryRecharge();
        }
    }

    private void TryRecharge()
    {
        // Evitamos que recargue si está en un diálogo
        if (LG_DialogueManager.IsDialogueActive) return;

        if (currentBattery >= maxBattery)
        {
            LG_TooltipManager.Instance?.ShowTooltipTemporary("Batería al máximo", 1.5f);
            return;
        }

        // Si tenemos baterías en el inventario...
        if (inventory != null && inventory.GetItemCount(batteryItemName) > 0)
        {
            inventory.RemoveItem(batteryItemName, 1);
            currentBattery += rechargeAmount;
            currentBattery = Mathf.Clamp(currentBattery, 0, maxBattery);

            if (SoundList.Instance != null) SoundList.Instance.PlaySound("SFX_Pickup_Battery");
            LG_TooltipManager.Instance?.ShowTooltipTemporary("<color=#3B82F6><b>Batería reemplazada</b></color>", 2f);

            UpdateUI();
        }
        else
        {
            LG_TooltipManager.Instance?.ShowTooltipTemporary("<color=#EF4444>No tienes baterías</color>", 1.5f);
        }
    }

    private void UpdateUI()
    {
        if (batterySlider != null)
        {
            batterySlider.value = currentBattery / maxBattery;
        }
    }
}