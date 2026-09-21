using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem; // Necesario para bloquear el Input del jugador

public class LG_TutorialManager : MonoBehaviour
{
    public static LG_TutorialManager Instance { get; private set; }
    public static bool IsTutorialActive { get; private set; }

    [Header("UI References")]
    [SerializeField] private GameObject tutorialPanel;
    [SerializeField] private TextMeshProUGUI tutorialText;
    [SerializeField] private Button continueButton;

    [Header("Pasos del Tutorial")]
    [TextArea(2, 4)]
    [SerializeField]
    private string[] tutorialSteps = new string[]
    {
        "¡Bienvenido al juego Sombra Rural!\nUsa las teclas **WASD** para moverte por las instalaciones.",
        "Mantén presionado **Left Shift** para correr y **Ctrl** para agacharte y cubrirte.",
        "Usa las teclas **1, 2 y 3** para alternar entre tu Rifle de Asalto, la Lámpara y el Bat cuerpo a cuerpo.",
        "Con el Rifle equipado (Tecla 1), presiona **R** para recargar tu cargador utilizando la munición de reserva.",
        "Con la Lámpara equipada (Tecla 2), presiona **F** para cambiar o recargar la batería cuando la luz comience a fallar.",
        "Presiona la tecla **H** (o 4) en cualquier momento para aplicar vendas y curar tus heridas.",
        "¡Estás listo! Explora las zonas de prueba, interactúa con las terminales y sobrevive."
    };

    private int currentStep = 0;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        if (continueButton != null)
        {
            continueButton.onClick.RemoveAllListeners();
            continueButton.onClick.AddListener(NextStep);
        }

        StartTutorial();
    }

    public void StartTutorial()
    {
        currentStep = 0;
        IsTutorialActive = true;

        if (tutorialPanel != null) tutorialPanel.SetActive(true);

        // Liberamos el cursor y bloqueamos al jugador
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

            // Seleccionar el botón para permitir navegación por teclado/gamepad
            if (EventSystem.current != null && continueButton != null)
            {
                EventSystem.current.SetSelectedGameObject(null);
                EventSystem.current.SetSelectedGameObject(continueButton.gameObject);
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

        if (tutorialPanel != null)
        {
            tutorialPanel.SetActive(false);
        }

        // Ocultamos el cursor y devolvemos el control al jugador
        LockPlayer(false);

        LG_TooltipManager.Instance?.ShowTooltipTemporary("<color=#10B981><b>¡Tutorial completado! Buena suerte.</b></color>", 3f);
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
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
}