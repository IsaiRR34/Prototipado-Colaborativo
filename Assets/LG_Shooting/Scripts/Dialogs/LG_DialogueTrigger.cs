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
        if (playerInRange && !LG_DialogueManager.IsDialogueActive && Input.GetKeyDown(KeyCode.E))
        {
            // Le pasamos todo el objeto raíz del jugador al Manager para que lo paralice correctamente
            LG_DialogueManager.Instance.StartDialogue(dialogueLines, playerRootRef);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Revisamos si es el jugador el que entró al Trigger
        if (other.CompareTag("Player") || other.GetComponentInParent<LG_PlayerHealth>() != null)
        {
            playerInRange = true;
            // Guardamos el padre principal del jugador para pasárselo al manager
            playerRootRef = other.transform.root.gameObject;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") || other.GetComponentInParent<LG_PlayerHealth>() != null)
        {
            playerInRange = false;
            playerRootRef = null;
        }
    }
}