using UnityEngine;
using DG.Tweening;

public class Door : MonoBehaviour
{
    [Header("Door Settings")]
    [SerializeField] private string requiredKeyName = "Llave Roja";
    [SerializeField] private Transform doorHinge;
    [SerializeField] private float openAngle = -95f;
    [SerializeField] private float openDuration = 1.2f;

    private bool playerInRange = false;
    private bool isOpen = false;
    private bool isOpening = false;
    private LG_Inventory currentInventory;

    private void Start()
    {
        if (doorHinge == null)
        {
            Transform found = transform.Find("Door_Hinge");
            if (found != null)
            {
                doorHinge = found;
            }
            else
            {
                found = transform.Find("Door_Mesh");
                doorHinge = found != null ? found : transform;
            }
        }
    }

    private void Update()
    {
        if (playerInRange && !isOpen && !isOpening && Input.GetKeyDown(KeyCode.E))
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
            currentInventory = inventory;

            if (!isOpen && !isOpening)
            {
                if (LG_TooltipManager.Instance != null)
                {
                    LG_TooltipManager.Instance.ShowTooltip("E - Abrir Puerta");
                }
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        LG_Inventory inventory = other.GetComponentInParent<LG_Inventory>();
        if (inventory != null)
        {
            playerInRange = false;
            currentInventory = null;

            if (LG_TooltipManager.Instance != null)
            {
                LG_TooltipManager.Instance.HideTooltip();
            }
        }
    }

    public void TryOpenDoor()
    {
        if (isOpen || isOpening) return;

        if (currentInventory == null)
        {
            currentInventory = Object.FindFirstObjectByType<LG_Inventory>();
        }

        bool hasKey = false;
        if (string.IsNullOrEmpty(requiredKeyName))
        {
            hasKey = true;
        }
        else if (currentInventory != null)
        {
            hasKey = currentInventory.HasKey(requiredKeyName) ||
                     currentInventory.GetItemCount(requiredKeyName) > 0 ||
                     currentInventory.HasKey("Llave") ||
                     currentInventory.HasKey("Key");
        }

        if (hasKey)
        {
            isOpen = true;
            isOpening = true;

            if (LG_TooltipManager.Instance != null)
            {
                LG_TooltipManager.Instance.ShowTooltipTemporary("ACCESO CONCEDIDO", 2f);
            }

            if (SoundList.Instance != null)
            {
                SoundList.Instance.PlaySound("SFX_Door_Unlock");
            }

            if (doorHinge != null)
            {
                doorHinge.DOLocalRotate(new Vector3(0, openAngle, 0), openDuration)
                    .SetEase(Ease.OutQuad)
                    .OnComplete(() => { isOpening = false; });
            }
            else
            {
                isOpening = false;
            }

            Debug.Log("[Door] Puerta abierta con éxito.");
        }
        else
        {
            if (SoundList.Instance != null)
            {
                SoundList.Instance.PlaySound("SFX_Empty");
            }

            if (LG_TooltipManager.Instance != null)
            {
                LG_TooltipManager.Instance.ShowTooltipTemporary($"Se requiere {requiredKeyName}", 2f);
            }

            Debug.LogWarning($"[Door] No se puede abrir la puerta. Falta {requiredKeyName}.");
        }
    }
}