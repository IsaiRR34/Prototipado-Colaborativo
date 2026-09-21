using UnityEngine;
using UnityEngine.SceneManagement;

public class DoorTR : MonoBehaviour
{
    [Header("Door Settings")]
    [Tooltip("El nombre exacto de la llave requerida (ej. 'Llave Roja')")]
    [SerializeField] private string requiredKeyName = "Llave Roja";
    [SerializeField] private Transform doorHinge;
    [SerializeField] private float openAngle = -95f;
    [SerializeField] private float openSpeed = 2f;

    [Header("Game Flow")]
    [Tooltip("¿Abrir esta puerta termina el nivel?")]
    [SerializeField] private bool loadVictoryScene = true;
    [Tooltip("Índice de la escena de Victoria en el Build Settings (ej: 2)")]
    [SerializeField] private int victorySceneIndex = 2;

    private bool playerInRange = false;
    private bool isOpen = false;
    private Quaternion targetRotation;
    private GameObject playerRootRef;

    private void Start()
    {
        if (doorHinge != null)
        {
            targetRotation = doorHinge.localRotation;
        }
    }

    private void Update()
    {
        // Interacción para abrir la puerta
        if (playerInRange && !isOpen && Input.GetKeyDown(KeyCode.E))
        {
            TryOpenDoor();
        }

        // Animación suave de la bisagra
        if (isOpen && doorHinge != null)
        {
            doorHinge.localRotation = Quaternion.Slerp(doorHinge.localRotation, targetRotation, Time.deltaTime * openSpeed);
        }
    }

    private void TryOpenDoor()
    {
        if (playerRootRef == null) return;

        LG_Inventory inventory = playerRootRef.GetComponentInChildren<LG_Inventory>();

        if (inventory != null && inventory.GetItemCount(requiredKeyName) > 0)
        {
            // ¡Éxito! Tiene la llave
            isOpen = true;
            targetRotation = Quaternion.Euler(0f, openAngle, 0f);

            // Audio centralizado
            if (SoundList.Instance != null)
            {
                SoundList.Instance.PlaySound("SFX_Door_Unlock");
            }

            // Ocultar Tooltip
            if (LG_TooltipManager.Instance != null)
            {
                LG_TooltipManager.Instance.HideTooltip();
            }

            // Flujo de Victoria (Fader y cambio de escena)
            if (loadVictoryScene)
            {
                TriggerVictorySequence();
            }
        }
        else
        {
            // Denegado
            if (SoundList.Instance != null)
            {
                SoundList.Instance.PlaySound("SFX_Empty");
            }

            if (LG_TooltipManager.Instance != null)
            {
                LG_TooltipManager.Instance.ShowTooltipTemporary("¡Necesitas la Llave Amarilla!", 2f);
            }
        }
    }

    private void TriggerVictorySequence()
    {
        // 1. Apagamos controles del jugador usando el TimeManager
        if (TimeManager.Instance != null)
        {
            TimeManager.Instance.LockPlayerForTransition(true);
        }

        // 2. Transición de Audio (opcional si usaste el SceneAudioController)
        if (SoundList.Instance != null)
        {
            SoundList.Instance.StopSound("BGM_Gameplay");
            SoundList.Instance.PlaySound("BGM_Victory");
        }

        // 3. Buscar el Fader en escena, si existe, hacer fundido a negro y luego cargar
        Fader fader = Object.FindFirstObjectByType<Fader>();
        if (fader != null)
        {
            // Asignar el evento dinámicamente para cargar la escena tras oscurecer
            fader.onEndFadeEvent.RemoveAllListeners();
            fader.onEndFadeEvent.AddListener(LoadVictorySceneNow);
            fader.Fade(true); // Fade a negro
        }
        else
        {
            // Si no hay Fader, cargar de golpe tras 1 segundo
            Invoke(nameof(LoadVictorySceneNow), 1f);
        }
    }

    private void LoadVictorySceneNow()
    {
        // Asegurarse de que el índice exista en Build Settings
        if (victorySceneIndex >= 0 && victorySceneIndex < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(victorySceneIndex);
        }
        else
        {
            Debug.LogError($"[Door] El índice de escena de Victoria ({victorySceneIndex}) no es válido en el Build Settings.");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") || other.GetComponentInParent<LG_PlayerHealth>() != null)
        {
            playerInRange = true;
            playerRootRef = other.transform.root.gameObject;

            if (!isOpen && LG_TooltipManager.Instance != null)
            {
                LG_TooltipManager.Instance.ShowTooltip("E - Puerta");
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") || other.GetComponentInParent<LG_PlayerHealth>() != null)
        {
            playerInRange = false;
            playerRootRef = null;

            if (LG_TooltipManager.Instance != null)
            {
                LG_TooltipManager.Instance.HideTooltip();
            }
        }
    }
}