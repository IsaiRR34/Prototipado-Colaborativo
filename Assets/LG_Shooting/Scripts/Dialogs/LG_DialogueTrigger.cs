using UnityEngine;

public class LG_DialogueTrigger : MonoBehaviour
{
    [Header("Párrafos del Diálogo")]
    [TextArea(3, 10)]
    public string[] dialogueLines;

    [Header("Opciones de Respuesta")]
    [TextArea(2, 3)]
    public string truthAnswer = "Decir la verdad";
    [TextArea(2, 3)]
    public string lieAnswer = "Mentir";

    private bool playerInRange = false;
    private GameObject playerRootRef;

    private void Update()
    {
        if (playerInRange && playerRootRef != null)
        {
            if (Vector3.Distance(transform.position, playerRootRef.transform.position) > 5f)
            {
                playerInRange = false;
                playerRootRef = null;
                LG_TooltipManager.Instance?.HideTooltip();
                return;
            }
        }

        if (playerInRange && !LG_DialogueManager.IsDialogueActive && Input.GetKeyDown(KeyCode.E))
        {
            LG_TooltipManager.Instance?.HideTooltip();

            // Le pasamos las líneas y los textos de los botones al Manager
            LG_DialogueManager.Instance.StartDialogue(dialogueLines, truthAnswer, lieAnswer, playerRootRef);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") || other.GetComponentInParent<LG_PlayerHealth>() != null)
        {
            playerInRange = true;
            playerRootRef = other.transform.root.gameObject;
            LG_TooltipManager.Instance?.ShowTooltip("E - Hablar");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") || other.GetComponentInParent<LG_PlayerHealth>() != null)
        {
            playerInRange = false;
            playerRootRef = null;
            LG_TooltipManager.Instance?.HideTooltip();
        }
    }
}