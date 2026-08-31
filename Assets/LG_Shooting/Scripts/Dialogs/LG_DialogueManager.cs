using UnityEngine;
using UnityEngine.UI;
using TMPro; // Necesario para TextMeshPro
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem; // Para bloquear el nuevo Input System

public class LG_DialogueManager : MonoBehaviour
{
    public static LG_DialogueManager Instance { get; private set; }
    public static bool IsDialogueActive { get; private set; }

    [Header("UI References")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TextMeshProUGUI dialogueText; // Cambiado a TMPro, ya que dejamos de usar Legacy, que es el modo compatible con el proyecto antiguo (versión 1 del equipo original)
    [SerializeField] private GameObject optionsContainer;
    [SerializeField] private Button truthButton;
    [SerializeField] private Button lieButton;

    [Header("Typing Effect Settings")]
    [SerializeField] private float typingSpeed = 0.02f;

    private Queue<string> sentences;
    private bool isTyping = false;
    private string currentSentence;

    // Referencia al jugador para bloquearlo
    private GameObject playerObject;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        sentences = new Queue<string>();
    }

    private void Start()
    {
        dialoguePanel.SetActive(false);
        optionsContainer.SetActive(false);

        // Limpiamos y asignamos los eventos a los botones por código
        truthButton.onClick.RemoveAllListeners();
        lieButton.onClick.RemoveAllListeners();
        truthButton.onClick.AddListener(OnTruthSelected);
        lieButton.onClick.AddListener(OnLieSelected);
    }

    private void Update()
    {
        if (!IsDialogueActive) return;

        // Avanzar diálogo (Solo si NO estamos mostrando las opciones)
        if (!optionsContainer.activeSelf && (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.F) || Input.GetMouseButtonDown(0)))
        {
            if (isTyping)
            {
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

    public void StartDialogue(string[] dialogueLines, GameObject player)
    {
        IsDialogueActive = true;
        dialoguePanel.SetActive(true);
        optionsContainer.SetActive(false);

        // Guardamos la referencia del jugador y lo bloqueamos
        playerObject = player;
        LockPlayer(true);

        sentences.Clear();
        foreach (string line in dialogueLines)
        {
            sentences.Enqueue(line);
        }

        DisplayNextSentence();
    }

    public void DisplayNextSentence()
    {
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

        // Protección para evitar el NullReferenceException
        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(truthButton.gameObject);
        }
        else
        {
            // Advertencia si no hay un EventSystem en la escena, porque por alguna razón no se agrega automáticamente
            Debug.LogWarning("Falta un EventSystem en la escena. Los botones no funcionarán con teclado.");
        }
    }

    public void OnTruthSelected()
    {
        EndDialogue();
    }

    public void OnLieSelected()
    {
        // Instakill buscando el script de vida en la jerarquía del jugador
        LG_PlayerHealth health = playerObject.GetComponentInChildren<LG_PlayerHealth>();
        if (health != null)
        {
            health.TakeDamage(1000f); // Daño masivo
        }
        EndDialogue();
    }

    private void EndDialogue()
    {
        IsDialogueActive = false;
        dialoguePanel.SetActive(false);
        optionsContainer.SetActive(false);

        LockPlayer(false); // Desbloqueamos al jugador
    }

    private void LockPlayer(bool lockInput)
    {
        if (playerObject == null) return;

        // 1. Bloqueamos el componente PlayerInput completo... en teoría...
        PlayerInput pInput = playerObject.GetComponentInChildren<PlayerInput>();
        if (pInput != null) pInput.enabled = !lockInput;

        // 2. Bloqueo de armas para que no sea posible disparar/cambiar de arma mientras el player está en diálogo // FIX NEW VERSION, el anterior no funcionaba
        LG_Shoot pShoot = playerObject.GetComponentInChildren<LG_Shoot>();
        if (pShoot != null) pShoot.enabled = !lockInput;

        Hand pHand = playerObject.GetComponentInChildren<Hand>();
        if (pHand != null) pHand.enabled = !lockInput;

        // 3. Deshabilitamos cualquier script que controle movimiento y rotación de cámara como paso adicional para asegurarnos de que el jugador no pueda moverse ni mirar alrededor durante el diálogo
        // Nota: Esto es algo genérico y puede mejorarse, por ahora lo usamos para testing.
        MonoBehaviour[] scripts = playerObject.GetComponentsInChildren<MonoBehaviour>();
        foreach (var script in scripts)
        {
            string scriptName = script.GetType().Name;
            // Buscamos palabras clave en los scripts del jugador
            if (scriptName.Contains("Move") || scriptName.Contains("Look") || scriptName.Contains("FPS"))
            {
                script.enabled = !lockInput;
            }
        }

        // 4. Habilitar el mouse para los botones
        if (lockInput)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
}