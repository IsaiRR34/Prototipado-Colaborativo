using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems; // Necesario para navegar con W/S y Flechas
using System.Collections;
using System.Collections.Generic;

public class LG_DialogueManager : MonoBehaviour
{
    public static LG_DialogueManager Instance { get; private set; }
    public static bool IsDialogueActive { get; private set; }

    [Header("UI References")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private Text dialogueText;
    [SerializeField] private GameObject optionsContainer;
    [SerializeField] private Button truthButton;
    [SerializeField] private Button lieButton;

    [Header("Typing Effect Settings")]
    [SerializeField] private float typingSpeed = 0.02f;

    private Queue<string> sentences;
    private bool isTyping = false;
    private string currentSentence;

    // Fix - Referencias para bloquear al jugador cuando está en diálogo, sigo testeando funcionalidad y ajustando detalles
    private LG_PlayerMovement playerMovement;
    private LG_Shoot playerShoot;

    private void Awake()
    {
        // Singleton pattern para acceder fácilmente desde cualquier script
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        sentences = new Queue<string>();
    }

    private void Start()
    {
        dialoguePanel.SetActive(false);
        optionsContainer.SetActive(false);

        // Asignar los eventos a los botones truthButton y lieButton
        truthButton.onClick.AddListener(OnTruthSelected);
        lieButton.onClick.AddListener(OnLieSelected);
    }

    private void Update()
    {
        if (!IsDialogueActive) return;

        // Avanzar diálogo con Espacio, F o Click (Solo si NO estamos mostrando las opciones) - Ajuste para diálogos largos, falta testear
        if (!optionsContainer.activeSelf && (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.F) || Input.GetMouseButtonDown(0)))
        {
            if (isTyping)
            {
                // Si está escribiendo, saltar el efecto y mostrar texto completo
                StopAllCoroutines();
                dialogueText.text = currentSentence;
                isTyping = false;
            }
            else
            {
                DisplayNextSentence();
            }
        }
    }

    public void StartDialogue(string[] dialogueLines, LG_PlayerMovement pMove, LG_Shoot pShoot)
    {
        IsDialogueActive = true;
        dialoguePanel.SetActive(true);
        optionsContainer.SetActive(false);

        // Guardamos las referencias y bloqueamos al jugador
        playerMovement = pMove;
        playerShoot = pShoot;

        if (playerMovement != null) playerMovement.enabled = false;
        if (playerShoot != null) playerShoot.enabled = false;

        sentences.Clear();

        // Encolamos los párrafos cortos
        foreach (string line in dialogueLines)
        {
            sentences.Enqueue(line);
        }

        DisplayNextSentence();
    }

    public void DisplayNextSentence()
    {
        // Si ya no hay más texto, mostramos las opciones
        if (sentences.Count == 0)
        {
            ShowOptions();
            return;
        }

        currentSentence = sentences.Dequeue();
        StopAllCoroutines();
        StartCoroutine(TypeSentence(currentSentence));
    }

    private IEnumerator TypeSentence(string sentence)
    {
        isTyping = true;
        dialogueText.text = "";
        foreach (char letter in sentence.ToCharArray())
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }
        isTyping = false;
    }

    private void ShowOptions()
    {
        optionsContainer.SetActive(true);

        // Esto es CLAVE para usar W/S o Flechas. Seleccionamos el primer botón automáticamente.
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(truthButton.gameObject);
    }

    private void OnTruthSelected()
    {
        Debug.Log("Decidiste decir la VERDAD.");
        EndDialogue();
    }

    private void OnLieSelected()
    {
        Debug.Log("Decidiste MENTIR. ¡Consecuencia fatal!");

        // Ejecutamos la penalización llamando a la salud del jugador
        LG_PlayerHealth health = Object.FindFirstObjectByType<LG_PlayerHealth>();
        if (health != null)
        {
            health.TakeDamage(health.GetMaxHealth()); // Daño masivo / instakill
        }

        EndDialogue();
    }

    private void EndDialogue()
    {
        IsDialogueActive = false;
        dialoguePanel.SetActive(false);
        optionsContainer.SetActive(false);

        // Desbloqueamos al jugador
        if (playerMovement != null) playerMovement.enabled = true;
        if (playerShoot != null) playerShoot.enabled = true;
    }
}