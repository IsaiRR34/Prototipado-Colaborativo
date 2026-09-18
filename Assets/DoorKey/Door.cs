using UnityEngine;

public class Door : MonoBehaviour
{
    [Header("Door Settings")]
    [SerializeField] private string requiredKeyName = "Llave Roja";
    [SerializeField] private Transform doorHinge;
    [SerializeField] private float openAngle = -95f;
    [SerializeField] private bool isOpen = false;

    [Header("Efectos de Audio (SFX)")]
    [SerializeField] private AudioClip unlockSound;
    [SerializeField] private AudioClip deniedSound;

    private bool playerInRange = false;
    private LG_Inventory cachedInventory = null;

    private void Start()
    {
#if UNITY_EDITOR
        if (unlockSound == null) unlockSound = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/LG_Shooting/LGAssets/Audio/SFX_Door_Unlock.wav");
        if (deniedSound == null) deniedSound = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/LG_Shooting/LGAssets/Audio/SFX_Empty.wav");
#endif
    }

    private void Update()
    {
        if (playerInRange && !isOpen && Input.GetKeyDown(KeyCode.E))
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
            cachedInventory = inventory;

            if (LG_TooltipManager.Instance != null)
            {
                string msg = isOpen ? "Puerta Abierta" : (inventory.HasKey(requiredKeyName) ? $"[ E ] Abrir con {requiredKeyName}" : $"Bloqueada (Requiere {requiredKeyName})");
                LG_TooltipManager.Instance.ShowTooltip(msg);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        LG_Inventory inventory = other.GetComponentInParent<LG_Inventory>();
        if (inventory != null && inventory == cachedInventory)
        {
            playerInRange = false;
            cachedInventory = null;

            if (LG_TooltipManager.Instance != null)
            {
                LG_TooltipManager.Instance.HideTooltip();
            }
        }
    }

    private void TryOpenDoor()
    {
        if (cachedInventory != null && cachedInventory.HasKey(requiredKeyName))
        {
            isOpen = true;
            if (unlockSound != null) AudioSource.PlayClipAtPoint(unlockSound, transform.position);
            if (SoundList.Instance != null) SoundList.Instance.PlaySound("SFX_Door_Unlock");

            if (doorHinge != null)
            {
                DG.Tweening.ShortcutExtensions.DOLocalRotate(doorHinge, new Vector3(0f, openAngle, 0f), 1.2f);
            }

            if (LG_TooltipManager.Instance != null) LG_TooltipManager.Instance.ShowTooltipTemporary("ACCESO CONCEDIDO", 1.5f);
        }
        else
        {
            if (deniedSound != null) AudioSource.PlayClipAtPoint(deniedSound, transform.position);
            if (SoundList.Instance != null) SoundList.Instance.PlaySound("SFX_Empty");
            if (LG_TooltipManager.Instance != null) LG_TooltipManager.Instance.ShowTooltipTemporary($"Requiere {requiredKeyName}", 1.5f);
        }
    }
}