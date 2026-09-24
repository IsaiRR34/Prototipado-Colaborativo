using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using DG.Tweening;
using System.Collections;
using UnityEngine.SceneManagement;

public class TimeManager : MonoBehaviour
{
    public static TimeManager Instance { get; private set; }

    [Header("UI Referencias")]
    [SerializeField] private CanvasGroup pauseScreen;
    [SerializeField] private float pauseTweenTime = 0.25f;

    private bool isPaused = false;
    private Tween pauseTween;
    private Coroutine freezeFrameRoutine;
    private float lastToggleTime = -1f;

    private GameObject playerRef;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void AutoInit()
    {
        if (Instance == null && Object.FindFirstObjectByType<TimeManager>() == null)
        {
            GameObject tmGO = new GameObject("TimeManager");
            tmGO.AddComponent<TimeManager>();
            DontDestroyOnLoad(tmGO);
        }
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        InitializeForScene();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        InitializeForScene();
    }

    private void InitializeForScene()
    {
        pauseTween?.Kill();
        if (freezeFrameRoutine != null) StopCoroutine(freezeFrameRoutine);

        isPaused = false;
        Time.timeScale = 1f;

        EnsurePauseScreen();

        if (pauseScreen != null)
        {
            pauseScreen.alpha = 0f;
            pauseScreen.blocksRaycasts = false;
            pauseScreen.interactable = false;
            pauseScreen.gameObject.SetActive(false);
        }

        EnsureEventSystem();
        playerRef = null;
        FindPlayer();

        string currentScene = SceneManager.GetActiveScene().name;
        if (currentScene != "MainMenu" && currentScene != "Victory" && currentScene != "GameOver")
        {
            LockPlayerForTransition(false);
        }
    }

    private void FindPlayer()
    {
        if (playerRef == null)
        {
            playerRef = GameObject.Find("Player (RI + LG)");
            if (playerRef == null) playerRef = GameObject.FindGameObjectWithTag("Player");
        }
    }

    public CanvasGroup EnsurePauseScreen()
    {
        if (pauseScreen != null && pauseScreen.gameObject != null) return pauseScreen;

        GameObject canvasGO = GameObject.Find("Canvas");
        if (canvasGO != null)
        {
            Transform psT = canvasGO.transform.Find("PauseScreen");
            if (psT != null) pauseScreen = psT.GetComponent<CanvasGroup>();
        }

        if (pauseScreen == null)
        {
            CanvasGroup[] groups = Object.FindObjectsByType<CanvasGroup>(FindObjectsSortMode.None);
            foreach (var cg in groups)
            {
                if (cg.gameObject.name.ToLower().Contains("pause"))
                {
                    pauseScreen = cg;
                    break;
                }
            }
        }

        if (pauseScreen != null)
        {
            UnityEngine.UI.Button[] botones = pauseScreen.GetComponentsInChildren<UnityEngine.UI.Button>(true);
            foreach (var btn in botones)
            {
                string btnName = btn.gameObject.name.ToLower();
                if (btnName.Contains("resume") || btnName.Contains("reanudar") || btnName.Contains("continue"))
                {
                    btn.onClick.RemoveListener(ResumeGame);
                    btn.onClick.AddListener(ResumeGame);
                    break;
                }
            }
        }

        return pauseScreen;
    }

    public static void EnsureEventSystem()
    {
        if (Object.FindFirstObjectByType<EventSystem>() == null)
        {
            GameObject esGO = new GameObject("EventSystem");
            esGO.AddComponent<EventSystem>();
            esGO.AddComponent<InputSystemUIInputModule>().AssignDefaultActions();
        }
    }

    private void Update()
    {
        string currentScene = SceneManager.GetActiveScene().name;
        if (currentScene == "Victory" || currentScene == "GameOver" || currentScene == "MainMenu")
        {
            if (isPaused) TogglePause(false);
            return;
        }

        bool pausePressed = false;

        // NUEVO MÉTODO: Lectura directa del teclado
        if (Keyboard.current != null)
        {
            if (Keyboard.current.escapeKey.wasPressedThisFrame || Keyboard.current.pKey.wasPressedThisFrame)
            {
                pausePressed = true;
            }
        }

        if (pausePressed && (Time.unscaledTime - lastToggleTime > 0.2f))
        {
            lastToggleTime = Time.unscaledTime;
            TogglePause(!isPaused);
        }
    }

    public void TogglePause(bool pause)
    {
        if (freezeFrameRoutine != null) StopCoroutine(freezeFrameRoutine);

        isPaused = pause;
        pauseTween?.Kill();

        EnsurePauseScreen();
        EnsureEventSystem();

        if (isPaused)
        {
            Time.timeScale = 0f;
            LockPlayerForTransition(true);

            if (pauseScreen != null)
            {
                pauseScreen.gameObject.SetActive(true);
                pauseScreen.blocksRaycasts = true;
                pauseScreen.interactable = true;
                pauseTween = pauseScreen.DOFade(1f, pauseTweenTime).SetUpdate(true);
            }
        }
        else
        {
            LockPlayerForTransition(false);

            if (pauseScreen != null)
            {
                pauseScreen.blocksRaycasts = false;
                pauseScreen.interactable = false;
                pauseTween = pauseScreen.DOFade(0f, pauseTweenTime)
                    .SetUpdate(true)
                    .OnComplete(() =>
                    {
                        Time.timeScale = 1f;
                        pauseScreen.gameObject.SetActive(false);
                    });
            }
            else
            {
                Time.timeScale = 1f;
            }
        }
    }

    public void LockPlayerForTransition(bool lockInput)
    {
        FindPlayer();

        if (!lockInput)
        {
            bool isUIActive = false;
            if (LG_DialogueManager.Instance != null && LG_DialogueManager.IsDialogueActive) isUIActive = true;
            if (LG_TutorialManager.Instance != null && LG_TutorialManager.IsTutorialActive) isUIActive = true;
            if (isUIActive) return;
        }

        if (playerRef != null)
        {
            PlayerInput pInput = playerRef.GetComponentInChildren<PlayerInput>();
            if (pInput != null) pInput.enabled = !lockInput;

            LG_Shoot pShoot = playerRef.GetComponentInChildren<LG_Shoot>();
            if (pShoot != null) pShoot.enabled = !lockInput;

            RIMovement pMovement = playerRef.GetComponentInChildren<RIMovement>();
            if (pMovement != null) pMovement.enabled = !lockInput;

            Hand pHand = playerRef.GetComponentInChildren<Hand>();
            if (pHand != null) pHand.enabled = !lockInput;
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

    public void ResumeGame() => TogglePause(false);

    public void FreezeFrame(float timeScale, float duration)
    {
        if (freezeFrameRoutine != null) StopCoroutine(freezeFrameRoutine);
        freezeFrameRoutine = StartCoroutine(FreezeFrameRoutine(timeScale, duration));
    }

    private IEnumerator FreezeFrameRoutine(float targetScale, float duration)
    {
        Time.timeScale = targetScale;
        yield return new WaitForSecondsRealtime(duration);
        Time.timeScale = 1f;
    }
}