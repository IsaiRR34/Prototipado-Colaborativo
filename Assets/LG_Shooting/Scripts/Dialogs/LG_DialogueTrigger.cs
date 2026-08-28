using UnityEngine;

public class LG_DialogueTrigger : MonoBehaviour
{
    [Header("Escribe aquí los párrafos cortos")]
    [TextArea(3, 10)]
    public string[] dialogueLines;

    private bool playerInRange = false;
    private LG_PlayerMovement pMove;
    private LG_Shoot pShoot;

    private void Update()
    {
        // Iniciar interacción con 'E' si el jugador está cerca y no hay un diálogo activo
        if (playerInRange && !LG_DialogueManager.IsDialogueActive && Input.GetKeyDown(KeyCode.E))
        {
            LG_DialogueManager.Instance.StartDialogue(dialogueLines, pMove, pShoot);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Si el jugador entra a la zona (Box Collider IsTrigger)
        if (other.CompareTag("Player") || other.GetComponent<LG_PlayerMovement>() != null)
        {
            playerInRange = true;
            pMove = other.GetComponent<LG_PlayerMovement>();
            pShoot = other.GetComponent<LG_Shoot>();

            // Pendiente: mostrar tooltip "Presiona E para hablar", por ahora para testing nosotros sabemos que está en rango y puede presionar E para iniciar el diálogo
            // Probablemente cree un script nuevo para los tooltips y lo llame desde aquí, o lo haga directamente desde el LG_DialogueManager. Por ahora no lo implemento.
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") || other.GetComponent<LG_PlayerMovement>() != null)
        {
            playerInRange = false;
        }
    }
}