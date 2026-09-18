using UnityEngine;

public class LG_DialogueTrigger : MonoBehaviour
{
    [Header("Escribe aquí los párrafos cortos")]
    [TextArea(3, 10)]
    public string[] dialogueLines;

    private bool playerInRange = false;
    private GameObject playerRootRef;

    private void Update()
    {
        // 1. Validación de seguridad (Failsafe) por si el jugador respawnea/teletransporta
        if (playerInRange && playerRootRef != null)
        {
            if (Vector3.Distance(transform.position, playerRootRef.transform.position) > 5f)
            {
                playerInRange = false;
                playerRootRef = null;
                if (LG_TooltipManager.Instance != null) LG_TooltipManager.Instance.HideTooltip();
                return;
            }
        }

        // 2. Comportamiento normal de interacción
        if (playerInRange && !LG_DialogueManager.IsDialogueActive && Input.GetKeyDown(KeyCode.E))
        {
            // Ocultar Tooltip porque ya inició el diálogo
            if (LG_TooltipManager.Instance != null) LG_TooltipManager.Instance.HideTooltip();

            LG_DialogueManager.Instance.StartDialogue(dialogueLines, playerRootRef);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") || other.GetComponentInParent<LG_PlayerHealth>() != null)
        {
            playerInRange = true;
            playerRootRef = other.transform.root.gameObject;

            if (LG_TooltipManager.Instance != null)
            {
                LG_TooltipManager.Instance.ShowTooltip("E - Terminal");
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