using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class LG_DialogueManager : MonoBehaviour
{
    public static LG_DialogueManager Instance { get; private set; }
    public static bool IsDialogueActive { get; private set; }

    [Header("UI References")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private GameObject optionsContainer;
    [SerializeField] private Button truthButton;
    [SerializeField] private Button lieButton;

    [Header("Typing Effect Settings")]
    [SerializeField] private float typingSpeed = 0.02f;

    private Queue<string> sentences;
    private bool isTyping = false;
    private string currentSentence;

    private GameObject playerObject;

    private void Awake()
    {
        // Patrón Singleton a prueba de recarga de escenas
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        IsDialogueActive = false; // Forzamos el reinicio de la variable al cargar la escena
        sentences = new Queue<string>();
    }

    private void Start()
    {
        if (dialoguePanel != null) dialoguePanel.SetActive(false);
        if (optionsContainer != null) optionsContainer.SetActive(false);

        if (truthButton != null)
        {
            truthButton.onClick.RemoveAllListeners();
            truthButton.onClick.AddListener(OnTruthSelected);
        }

        if (lieButton != null)
        {
            lieButton.onClick.RemoveAllListeners();
            lieButton.onClick.AddListener(OnLieSelected);
        }
    }

    private void Update()
    {
        // Añadida protección contra nulos para evitar el MissingReferenceException
        if (!IsDialogueActive || optionsContainer == null || dialogueText == null) return;

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
        if (dialoguePanel == null || optionsContainer == null) return;

        IsDialogueActive = true;
        dialoguePanel.SetActive(true);
        optionsContainer.SetActive(false);

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
        if (optionsContainer != null) optionsContainer.SetActive(true);

        if (EventSystem.current != null && truthButton != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(truthButton.gameObject);
        }
        else if (EventSystem.current == null)
        {
            Debug.LogWarning("Falta un EventSystem en la escena para navegar los botones con el teclado.");
        }
    }

    public void OnTruthSelected()
    {
        EndDialogue();
    }

    public void OnLieSelected()
    {
        if (playerObject != null)
        {
            LG_PlayerHealth health = playerObject.GetComponentInChildren<LG_PlayerHealth>();
            if (health != null)
            {
                health.TakeDamage(1000f);
            }
        }
        EndDialogue();
    }

    private void EndDialogue()
    {
        IsDialogueActive = false;

        if (dialoguePanel != null) dialoguePanel.SetActive(false);
        if (optionsContainer != null) optionsContainer.SetActive(false);

        LockPlayer(false);
    }

    private void LockPlayer(bool lockInput)
    {
        if (playerObject == null) return;

        PlayerInput pInput = playerObject.GetComponentInChildren<PlayerInput>();
        if (pInput != null) pInput.enabled = !lockInput;

        LG_Shoot pShoot = playerObject.GetComponentInChildren<LG_Shoot>();
        if (pShoot != null) pShoot.enabled = !lockInput;

        Hand pHand = playerObject.GetComponentInChildren<Hand>();
        if (pHand != null) pHand.enabled = !lockInput;

        MonoBehaviour[] scripts = playerObject.GetComponentsInChildren<MonoBehaviour>();
        foreach (var script in scripts)
        {
            string scriptName = script.GetType().Name;
            if (scriptName.Contains("Move") || scriptName.Contains("Look") || scriptName.Contains("FPS"))
            {
                script.enabled = !lockInput;
            }
        }

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

    private void OnDestroy()
    {
        // Limpieza de memoria crucial al cambiar o reiniciar la escena
        if (Instance == this)
        {
            Instance = null;
            IsDialogueActive = false;
        }
    }
}