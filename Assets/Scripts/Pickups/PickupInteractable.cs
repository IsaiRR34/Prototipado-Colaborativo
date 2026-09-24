using UnityEngine;
using UnityEngine.InputSystem;

public class PickupInteractable : MonoBehaviour
{
    [Header("Item Data")]
    [SerializeField] private ItemDataSO itemData;
    [SerializeField] private int quantity = 1;
    [SerializeField] private bool requireInteractionKey = true;

    [Header("Animación")]
    [SerializeField] private float rotationSpeed = 60f;
    [SerializeField] private float bobFrequency = 2f;
    [SerializeField] private float bobAmplitude = 0.12f;

    private Vector3 startPos;
    private bool playerInRange = false;
    private LG_Inventory currentInventory = null;

    public ItemDataSO ItemData => itemData;
    public int Quantity => quantity;

    private void Start()
    {
        startPos = transform.position;

        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.isTrigger = true;
        }
    }

    private void Update()
    {
        transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime, Space.World);
        Vector3 tempPos = startPos;
        tempPos.y += Mathf.Sin(Time.time * bobFrequency) * bobAmplitude;
        transform.position = tempPos;

        if (playerInRange && requireInteractionKey)
        {
            if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
            {
                TryCollect(currentInventory);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        LG_Inventory inv = other.GetComponentInParent<LG_Inventory>();
        if (inv != null)
        {
            playerInRange = true;
            currentInventory = inv;

            if (requireInteractionKey)
            {
                string nameDisplay = itemData != null ? itemData.itemName : "Objeto";
                if (LG_TooltipManager.Instance != null)
                {
                    LG_TooltipManager.Instance.ShowTooltip($"[ E ] Recoger {nameDisplay} x{quantity}");
                }
            }
            else
            {
                TryCollect(inv);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        LG_Inventory inv = other.GetComponentInParent<LG_Inventory>();
        if (inv != null && inv == currentInventory)
        {
            playerInRange = false;
            currentInventory = null;
            if (LG_TooltipManager.Instance != null)
            {
                LG_TooltipManager.Instance.HideTooltip();
            }
        }
    }

    public bool TryCollect(LG_Inventory inventory)
    {
        if (inventory == null) return false;

        if (itemData == null)
        {
            Debug.LogWarning($"[PickupInteractable] {gameObject.name} no tiene ItemDataSO asignado.");
            return false;
        }

        bool added = inventory.AddItem(itemData, quantity);
        if (added)
        {
            if (LG_TooltipManager.Instance != null)
            {
                LG_TooltipManager.Instance.ShowTooltipTemporary($"+ {quantity}x {itemData.itemName}", 1.5f);
            }

            // Reproducir sonido correspondiente
            if (SoundList.Instance != null)
            {
                switch (itemData.itemType)
                {
                    case ItemType.Municion:
                        SoundList.Instance.PlaySound("SFX_Pickup_Ammo");
                        break;
                    case ItemType.LlaveMaestra:
                        SoundList.Instance.PlaySound("SFX_Pickup_Key");
                        break;
                    case ItemType.Curacion:
                        SoundList.Instance.PlaySound("SFX_Pickup_Battery");
                        break;
                    case ItemType.Recurso:
                    default:
                        SoundList.Instance.PlaySound("SFX_Pickup");
                        break;
                }
            }

            DoomLevelManager.Instance?.RegisterItemCollected();
            Destroy(gameObject);
            return true;
        }
        else
        {
            if (LG_TooltipManager.Instance != null)
            {
                LG_TooltipManager.Instance.ShowTooltipTemporary("Inventario lleno", 1.2f);
            }
            return false;
        }
    }

    public void Initialize(ItemDataSO data, int qty, bool reqKey = true)
    {
        itemData = data;
        quantity = qty;
        requireInteractionKey = reqKey;
    }
}
