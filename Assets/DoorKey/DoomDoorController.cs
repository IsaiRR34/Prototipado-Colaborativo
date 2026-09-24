using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;

public class DoomDoorController : MonoBehaviour
{
    public enum DoorLockType
    {
        None,
        RedKey,
        BlueKey,
        YellowKey,
        BossDefeated
    }

    [Header("Configuración de Seguridad")]
    [SerializeField] private DoorLockType lockType = DoorLockType.None;
    [SerializeField] private string customRequiredKey = "Llave Roja";
    [SerializeField] private bool isLevelExitDoor = false;
    [SerializeField] private int nextLevelIndex = 2;

    [Header("Componentes Mecánicos de la Puerta")]
    [SerializeField] private Transform doorHinge;
    [SerializeField] private float openAngle = -95f;
    [SerializeField] private float openDuration = 1.2f;

    [Header("Indicador Visual de Acceso")]
    [SerializeField] private Renderer statusLightRenderer;
    [SerializeField] private Color lockedColor = Color.red;
    [SerializeField] private Color unlockedColor = Color.green;

    [Header("Efectos de Audio (SFX)")]
    [SerializeField] private AudioClip unlockSound;
    [SerializeField] private AudioClip deniedSound;

    private bool isUnlocked = false;
    private bool isOpen = false;
    private bool playerInRange = false;
    private LG_Inventory cachedInventory = null;

    private void Start()
    {
#if UNITY_EDITOR
        if (unlockSound == null) unlockSound = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/LG_Shooting/LGAssets/Audio/SFX_Door_Unlock.wav");
        if (deniedSound == null) deniedSound = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/LG_Shooting/LGAssets/Audio/SFX_Empty.wav");
#endif
        UpdateStatusVisual();
    }

    private void Update()
    {
        if (playerInRange && (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame))
        {
            TryInteractDoor();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        LG_Inventory inv = other.GetComponentInParent<LG_Inventory>();
        if (inv != null)
        {
            playerInRange = true;
            cachedInventory = inv;

            ShowDoorPrompt();

            // Si es la puerta de salida y ya está abierta, avanzar de nivel al cruzar
            if (isOpen && isLevelExitDoor)
            {
                TriggerLevelComplete();
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        LG_Inventory inv = other.GetComponentInParent<LG_Inventory>();
        if (inv != null && inv == cachedInventory)
        {
            playerInRange = false;
            cachedInventory = null;
            LG_TooltipManager.Instance?.HideTooltip();
        }
    }

    private void ShowDoorPrompt()
    {
        if (isOpen)
        {
            if (isLevelExitDoor)
                LG_TooltipManager.Instance?.ShowTooltip("[ E ] Cruzar al siguiente sector");
            return;
        }

        string keyRequiredName = GetKeyDisplayName();
        if (HasRequiredKey(cachedInventory))
        {
            LG_TooltipManager.Instance?.ShowTooltip($"[ E ] Abrir Compuerta ({keyRequiredName})");
        }
        else
        {
            LG_TooltipManager.Instance?.ShowTooltip($"[ E ] Compuerta Bloqueada (Requiere {keyRequiredName})");
        }
    }

    public void TryInteractDoor()
    {
        if (isOpen)
        {
            if (isLevelExitDoor)
            {
                TriggerLevelComplete();
            }
            return;
        }

        if (HasRequiredKey(cachedInventory))
        {
            UnlockAndOpen();
        }
        else
        {
            // Acceso denegado
            if (deniedSound != null)
            {
                AudioSource.PlayClipAtPoint(deniedSound, transform.position, 1.0f);
            }
            if (SoundList.Instance != null) SoundList.Instance.PlaySound("SFX_Empty");

            string keyName = GetKeyDisplayName();
            LG_TooltipManager.Instance?.ShowTooltipTemporary($"ACCESO DENEGADO: Requiere {keyName}", 2f);

            // Parpadeo rojo en la luz de status
            if (statusLightRenderer != null)
            {
                statusLightRenderer.material.DOColor(Color.red * 2f, 0.1f).SetLoops(4, LoopType.Yoyo);
            }
        }
    }

    private bool HasRequiredKey(LG_Inventory inv)
    {
        if (lockType == DoorLockType.None) return true;
        if (lockType == DoorLockType.BossDefeated)
        {
            return BossBabyController.IsBossDefeated;
        }

        if (inv == null) return false;

        string keyToSearch = GetKeySearchString();
        return inv.HasKey(keyToSearch);
    }

    private string GetKeySearchString()
    {
        switch (lockType)
        {
            case DoorLockType.RedKey: return "Roja";
            case DoorLockType.BlueKey: return "Azul";
            case DoorLockType.YellowKey: return "Amarilla";
            default: return customRequiredKey;
        }
    }

    private string GetKeyDisplayName()
    {
        switch (lockType)
        {
            case DoorLockType.RedKey: return "Llave Roja";
            case DoorLockType.BlueKey: return "Llave Azul";
            case DoorLockType.YellowKey: return "Llave Amarilla";
            case DoorLockType.BossDefeated: return "Derrotar al Bebé Maldito";
            default: return customRequiredKey;
        }
    }

    private void UnlockAndOpen()
    {
        isUnlocked = true;
        isOpen = true;

        UpdateStatusVisual();

        if (unlockSound != null)
        {
            AudioSource.PlayClipAtPoint(unlockSound, transform.position, 1.0f);
        }
        if (SoundList.Instance != null)
        {
            SoundList.Instance.PlaySound("SFX_Door_Unlock");
        }

        LG_TooltipManager.Instance?.ShowTooltipTemporary("ACCESO AUTORIZADO", 1.5f);

        // Rotación suave de la compuerta usando DOTween
        if (doorHinge != null)
        {
            doorHinge.DOLocalRotate(new Vector3(0f, openAngle, 0f), openDuration).SetEase(Ease.OutCubic)
                .OnComplete(() =>
                {
                    if (isLevelExitDoor && playerInRange)
                    {
                        TriggerLevelComplete();
                    }
                });
        }
    }

    private void TriggerLevelComplete()
    {
        if (DoomLevelManager.Instance != null)
        {
            DoomLevelManager.Instance.CompleteCurrentLevel(nextLevelIndex);
        }
        else
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(nextLevelIndex);
        }
    }

    private void UpdateStatusVisual()
    {
        if (statusLightRenderer != null)
        {
            Color targetColor = (isUnlocked || lockType == DoorLockType.None) ? unlockedColor : lockedColor;
            statusLightRenderer.material.color = targetColor;
        }
    }
}
