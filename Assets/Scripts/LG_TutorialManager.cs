using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class LG_TutorialManager : MonoBehaviour
{
    public static LG_TutorialManager Instance { get; private set; }
    public static bool IsTutorialActive { get; private set; }

    [Header("UI References")]
    [SerializeField] private GameObject tutorialPanel;
    [SerializeField] private TextMeshProUGUI tutorialText;
    [SerializeField] private Button continueButton;
    [SerializeField] private Button skipButton; // Botón para saltar todo el tutorial

    [Header("Pasos del Tutorial")]
    [TextArea(2, 4)]
    [SerializeField]
    private string[] tutorialSteps = new string[]
    {
        "¡Bienvenido al Testbed de Sombra Rural!\nUsa las teclas **WASD** para moverte por las instalaciones.",
        "Mantén presionado **Left Shift** para correr y **Ctrl** para agacharte y cubrirte.",
        "Usa las teclas **1, 2 y 3** para alternar entre tu Rifle de Asalto, la Lámpara y el Bate cuerpo a cuerpo.",
        "Con el Rifle equipado (Tecla 1), presiona **R** para recargar tu cargador utilizando la munición de reserva.",
        "Con la Lámpara equipada (Tecla 2), presiona **F** para cambiar o recargar la batería cuando la luz comience a fallar.",
        "Presiona la tecla **H** (o 4) en cualquier momento para aplicar vendas y curar tus heridas.",
        "¡Estás listo! Explora las zonas de prueba, interactúa con las terminales y sobrevive."
    };

    private int currentStep = 0;

    private void Awake()
    {
        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
        IsTutorialActive = false;
    }

    private void Start()
    {
        if (continueButton != null)
        {
            continueButton.onClick.RemoveAllListeners();
            continueButton.onClick.AddListener(NextStep);
        }

        if (skipButton != null)
        {
            skipButton.onClick.RemoveAllListeners();
            skipButton.onClick.AddListener(EndTutorial);
        }

        StartTutorial();
    }

    private void Update()
    {
        if (!IsTutorialActive) return;

        // NUEVO MÉTODO: Avanzamos manualmente con Espacio o F
        if (Keyboard.current != null)
        {
            if (Keyboard.current.spaceKey.wasPressedThisFrame || Keyboard.current.fKey.wasPressedThisFrame)
            {
                NextStep();
            }
        }
    }

    // Método diseñado para ser llamado desde el botón del Menú de Pausa
    public void StartTutorialFromPause()
    {
        // Le avisamos al sistema que el tutorial ya empezó ANTES de quitar la pausa
        IsTutorialActive = true;

        if (TimeManager.Instance != null) TimeManager.Instance.ResumeGame();
        StartTutorial();
    }

    public void StartTutorial()
    {
        currentStep = 0;
        IsTutorialActive = true;

        if (tutorialPanel != null) tutorialPanel.SetActive(true);

        LockPlayer(true);
        ShowStep();
    }

    private void ShowStep()
    {
        if (tutorialSteps == null || tutorialSteps.Length == 0) return;

        if (currentStep < tutorialSteps.Length)
        {
            if (tutorialText != null)
            {
                tutorialText.text = tutorialSteps[currentStep];
            }

            // CORRECCIÓN DEL SALTO DOBLE:
            // Quitamos la selección automática del EventSystem. 
            // Así la barra espaciadora no activará el botón nativamente, solo a través de nuestro código en el Update.
            if (EventSystem.current != null)
            {
                EventSystem.current.SetSelectedGameObject(null);
            }
        }
        else
        {
            EndTutorial();
        }
    }

    public void NextStep()
    {
        currentStep++;
        ShowStep();
    }

    private void EndTutorial()
    {
        IsTutorialActive = false;

        if (tutorialPanel != null) tutorialPanel.SetActive(false);

        LockPlayer(false);
    }

    private void LockPlayer(bool lockInput)
    {
        GameObject playerRef = GameObject.Find("Player (RI + LG)");
        if (playerRef == null) playerRef = GameObject.FindGameObjectWithTag("Player");

        if (playerRef != null)
        {
            PlayerInput pInput = playerRef.GetComponentInChildren<PlayerInput>();
            if (pInput != null) pInput.enabled = !lockInput;

            LG_Shoot pShoot = playerRef.GetComponentInChildren<LG_Shoot>();
            if (pShoot != null) pShoot.enabled = !lockInput;

            Hand pHand = playerRef.GetComponentInChildren<Hand>();
            if (pHand != null) pHand.enabled = !lockInput;

            RIMovement pMovement = playerRef.GetComponentInChildren<RIMovement>();
            if (pMovement != null) pMovement.enabled = !lockInput;
        }

        if (lockInput)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            bool isUIActive = false;
            if (LG_DialogueManager.Instance != null && LG_DialogueManager.IsDialogueActive) isUIActive = true;
            if (LG_TutorialManager.Instance != null && LG_TutorialManager.IsTutorialActive) isUIActive = true;

            if (!isUIActive)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }
    }
}