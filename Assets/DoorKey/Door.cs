using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class Door : MonoBehaviour
{
    [Header("Configuracion de Llave")]
    [SerializeField] private string requiredKeyName = "Llave Roja";
    [SerializeField] private bool consumeKey = false;

    [Header("Movimiento de Apertura")]
    [SerializeField] private Transform doorHinge; // Transform que rotara (bisagra o puerta)
    [SerializeField] private float openAngle = -90f;
    [SerializeField] private float openDuration = 1.2f;

    [Header("Condicion de Victoria")]
    [SerializeField] private bool loadVictoryScene = true;
    [SerializeField] private float victoryDelay = 1.2f;
    [SerializeField] private int victorySceneIndex = 2; // Indice en Build Settings (Victory.unity)

    private bool isPlayerInRange = false;
    private bool isOpen = false;
    private bool isOpening = false;
    private LG_Inventory currentInventory;

    private void Start()
    {
        if (doorHinge == null)
        {
            Transform meshChild = transform.Find("Door_Mesh");
            doorHinge = meshChild != null ? meshChild : transform;
        }
    }

    private void Update()
    {
        if (isPlayerInRange && !isOpen && !isOpening)
        {
            // Abrir al presionar E o F
            if (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.F))
            {
                TryOpenDoor();
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        LG_Inventory inv = other.GetComponentInParent<LG_Inventory>();
        if (inv != null)
        {
            isPlayerInRange = true;
            currentInventory = inv;

            if (!isOpen && !isOpening)
            {
                if (inv.GetItemCount(requiredKeyName) > 0)
                {
                    Debug.Log($"[Door] Jugador cerca con '{requiredKeyName}'. Presiona 'E' para abrir la puerta.");
                    // Si ya tiene la llave y colisiona directamente, tambien puede abrir automaticamente
                    TryOpenDoor();
                }
                else
                {
                    Debug.Log($"[Door] Puerta cerrada. Se requiere '{requiredKeyName}' para abrir.");
                }
            }
            else if (isOpen && loadVictoryScene)
            {
                TriggerVictory();
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        LG_Inventory inv = other.GetComponentInParent<LG_Inventory>();
        if (inv != null && inv == currentInventory)
        {
            isPlayerInRange = false;
            currentInventory = null;
        }
    }

    public void TryOpenDoor()
    {
        if (currentInventory == null)
        {
            currentInventory = FindObjectOfType<LG_Inventory>();
        }

        if (currentInventory != null && currentInventory.GetItemCount(requiredKeyName) > 0)
        {
            if (consumeKey)
            {
                currentInventory.RemoveItem(requiredKeyName, 1);
            }

            Debug.Log($"[Door] ¡Llave '{requiredKeyName}' aceptada! Abriendo puerta...");
            StartCoroutine(OpenDoorRoutine());
        }
        else
        {
            Debug.LogWarning($"[Door] No puedes abrir la puerta. Falta la '{requiredKeyName}'.");
        }
    }

    private IEnumerator OpenDoorRoutine()
    {
        isOpening = true;

        Quaternion initialRotation = doorHinge.localRotation;
        Quaternion targetRotation = initialRotation * Quaternion.Euler(0f, openAngle, 0f);

        float elapsed = 0f;
        while (elapsed < openDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / openDuration);
            doorHinge.localRotation = Quaternion.Slerp(initialRotation, targetRotation, t);
            yield return null;
        }

        doorHinge.localRotation = targetRotation;
        isOpen = true;
        isOpening = false;

        Debug.Log("[Door] Puerta completamente abierta. ¡Nivel completado!");

        if (loadVictoryScene)
        {
            yield return new WaitForSeconds(victoryDelay);
            TriggerVictory();
        }
    }

    private void TriggerVictory()
    {
        Debug.Log("[Door] ¡Cargando pantalla de Victoria!");
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (victorySceneIndex >= 0 && victorySceneIndex < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(victorySceneIndex);
        }
        else
        {
            SceneManager.LoadScene("Victory");
        }
    }
}
