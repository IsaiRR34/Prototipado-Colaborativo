using UnityEngine;

public class Door : MonoBehaviour
{
    [Header("Door Settings")]
    [SerializeField] private string requiredKeyName = "Key"; // O "Llave Roja"
    [SerializeField] private Transform doorHinge;

    private bool playerInRange = false;

    private void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            TryOpenDoor();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        LG_Inventory inventory = other.GetComponentInParent<LG_Inventory>();
        if (inventory != null)
        {
            playerInRange = true;

            if (LG_TooltipManager.Instance != null)
            {
                LG_TooltipManager.Instance.ShowTooltip("E - Puerta");
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        LG_Inventory inventory = other.GetComponentInParent<LG_Inventory>();
        if (inventory != null)
        {
            playerInRange = false;

            if (LG_TooltipManager.Instance != null)
            {
                LG_TooltipManager.Instance.HideTooltip();
            }
        }
    }

    private void TryOpenDoor()
    {
        // Ocultar Tooltip tras el intento
        if (LG_TooltipManager.Instance != null) LG_TooltipManager.Instance.HideTooltip();

        // Aquí iría tu lógica original para abrir la puerta o hacer ruido de bloqueada
        Debug.Log("Intentando abrir la puerta...");
    }
}