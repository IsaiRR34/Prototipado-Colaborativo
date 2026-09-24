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
        //if (Instance != null && Instance != this)
        //{
        //    Destroy(gameObject);
        //    return;
        //}

        Instance = this;
        IsDialogueActive = false; // Forzamos el reinicio de la variable al cargar la escena
        sentences = new Queue<string>();
    }

    private void Start()
    {
        if (dialoguePanel == null)
        {
            GameObject p = GameObject.Find("DialoguePanel");
            if (p != null)
            {
                dialoguePanel = p;
                if (dialogueText == null) dialogueText = p.GetComponentInChildren<TextMeshProUGUI>(true);
                if (optionsContainer == null)
                {
                    Transform opt = p.transform.Find("OptionsContainer");
                    if (opt != null) optionsContainer = opt.gameObject;
                }
                if (truthButton == null && optionsContainer != null)
                {
                    Transform tb = optionsContainer.transform.Find("TruthButton");
                    if (tb != null) truthButton = tb.GetComponent<Button>();
                }
                if (lieButton == null && optionsContainer != null)
                {
                    Transform lb = optionsContainer.transform.Find("LieButton");
                    if (lb != null) lieButton = lb.GetComponent<Button>();
                }
            }
        }

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
        if (!IsDialogueActive || optionsContainer == null || dialogueText == null) return;

        if (!optionsContainer.activeSelf)
        {
            bool advancePressed = false;
            if (Keyboard.current != null && (Keyboard.current.spaceKey.wasPressedThisFrame || Keyboard.current.fKey.wasPressedThisFrame)) advancePressed = true;
            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame) advancePressed = true;

            if (advancePressed)
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
        else
        {
            if (Keyboard.current != null && (Keyboard.current.spaceKey.wasPressedThisFrame || Keyboard.current.fKey.wasPressedThisFrame))
            {
                if (EventSystem.current != null)
                {
                    GameObject selectedBtn = EventSystem.current.currentSelectedGameObject;
                    if (selectedBtn == truthButton.gameObject) OnTruthSelected();
                    else if (selectedBtn == lieButton.gameObject) OnLieSelected();
                }
            }
        }
    }

    public void StartDialogue(string[] dialogueLines, string truthTxt, string lieTxt, GameObject player)
    {
        if (dialoguePanel == null || optionsContainer == null) return;

        IsDialogueActive = true;
        dialoguePanel.SetActive(true);
        optionsContainer.SetActive(false);

        playerObject = player;
        LockPlayer(true);

        // Actualizamos dinámicamente el texto de los botones
        if (truthButton != null)
        {
            TextMeshProUGUI tText = truthButton.GetComponentInChildren<TextMeshProUGUI>();
            if (tText != null) tText.text = truthTxt;
        }

        if (lieButton != null)
        {
            TextMeshProUGUI lText = lieButton.GetComponentInChildren<TextMeshProUGUI>();
            if (lText != null) lText.text = lieTxt;
        }

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
                // Castigo: Recibe 30 de daño
                health.TakeDamage(30f);

                // Solo lo teletransportamos manualmente si sobrevivió al castigo.
                if (health.GetCurrentHealth() > 0f)
                {
                    Vector3 respawnPos = SafeRoomCheckpoint.HasCheckpoint ? SafeRoomCheckpoint.LastSafePosition : new Vector3(0f, 1f, 0f);
                    CharacterController cc = health.GetComponent<CharacterController>();
                    if (cc != null)
                    {
                        cc.enabled = false;
                        health.transform.position = respawnPos;
                        cc.enabled = true;
                    }
                    else
                    {
                        health.transform.position = respawnPos;
                    }

                    LG_TooltipManager.Instance?.ShowTooltipTemporary("<color=#EF4444><b>Las mentiras tienen un precio. (-30 Salud)</b></color>", 3.5f);
                }
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
