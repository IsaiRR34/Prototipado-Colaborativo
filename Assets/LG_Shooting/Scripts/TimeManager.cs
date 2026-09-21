using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using DG.Tweening;
using System.Collections;
using UnityEngine.SceneManagement; // Agregado para detectar escenas

public class TimeManager : MonoBehaviour
{
    public static TimeManager Instance { get; private set; }

    [Header("Configuracion de Tecla")]
    [SerializeField] private KeyCode pauseKey = KeyCode.Escape;
    [SerializeField] private KeyCode alternatePauseKey = KeyCode.P;

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

    private void Start()
    {
        EliminarFadersResiduales();
        EnsurePauseScreen();
        EnsureEventSystem();
        FindPlayer();
    }

    private void EliminarFadersResiduales()
    {
        Fader[] faders = Object.FindObjectsByType<Fader>(FindObjectsSortMode.None);
        foreach (var f in faders)
        {
            if (f != null && f.gameObject != null)
            {
                f.gameObject.SetActive(false);
                Destroy(f.gameObject);
            }
        }

        GameObject[] allGOs = Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None);
        foreach (var go in allGOs)
        {
            if (go != null && go.name.Equals("Fader", System.StringComparison.OrdinalIgnoreCase))
            {
                go.SetActive(false);
                Destroy(go);
            }
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
        if (pauseScreen != null)
        {
            pauseScreen.alpha = 0f;
            pauseScreen.blocksRaycasts = false;
            pauseScreen.interactable = false;
            return pauseScreen;
        }

        GameObject canvasGO = GameObject.Find("Canvas");
        if (canvasGO != null)
        {
            LimpiarElementosResidualesCanvas(canvasGO);
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

        if (pauseScreen == null)
        {
            GameObject prefab = null;
#if UNITY_EDITOR
            prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/AExport/Canvas.prefab");
#endif
            if (prefab == null)
            {
                prefab = Resources.Load<GameObject>("Canvas");
            }

            if (prefab != null)
            {
                GameObject inst = Instantiate(prefab);
                inst.name = "Canvas";
                LimpiarElementosResidualesCanvas(inst);

                Transform psT = inst.transform.Find("PauseScreen");
                if (psT != null) pauseScreen = psT.GetComponent<CanvasGroup>();
                else pauseScreen = inst.GetComponentInChildren<CanvasGroup>(true);
            }
        }

        if (pauseScreen != null)
        {
            pauseScreen.alpha = 0f;
            pauseScreen.blocksRaycasts = false;
            pauseScreen.interactable = false;

            UnityEngine.UI.Button resumeBtn = pauseScreen.GetComponentInChildren<UnityEngine.UI.Button>(true);
            if (resumeBtn != null)
            {
                resumeBtn.onClick.RemoveListener(ResumeGame);
                resumeBtn.onClick.AddListener(ResumeGame);
            }
        }

        return pauseScreen;
    }

    private void LimpiarElementosResidualesCanvas(GameObject canvasObj)
    {
        if (canvasObj == null) return;

        string[] nombresResiduales = new string[] { "Fader", "CanvasGroup_LifePoints", "CoinText", "IconImage" };
        foreach (string n in nombresResiduales)
        {
            Transform t = canvasObj.transform.Find(n);
            if (t != null)
            {
                t.gameObject.SetActive(false);
                Destroy(t.gameObject);
            }
        }
    }

    public static void EnsureEventSystem()
    {
        if (Object.FindFirstObjectByType<EventSystem>() == null)
        {
            GameObject esGO = new GameObject("EventSystem");
            esGO.AddComponent<EventSystem>();
            var inputModule = esGO.AddComponent<InputSystemUIInputModule>();
            inputModule.AssignDefaultActions();
        }
    }

    private void Update()
    {
        // 1. Evitar que la pausa funcione en las pantallas de fin de juego.
        string currentScene = SceneManager.GetActiveScene().name;
        if (currentScene == "Victory" || currentScene == "GameOver")
        {
            // Si está pausado de alguna manera rara, lo reanudamos a la fuerza
            if (isPaused) TogglePause(false);
            return;
        }

        bool pausePressed = false;

        if (Keyboard.current != null)
        {
            if (Keyboard.current.escapeKey.wasPressedThisFrame ||
                Keyboard.current.escapeKey.wasReleasedThisFrame ||
                Keyboard.current.pKey.wasPressedThisFrame)
            {
                pausePressed = true;
            }
        }

        if (!pausePressed)
        {
            if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyUp(KeyCode.Escape) ||
                Input.GetKeyDown(KeyCode.P) ||
                Input.GetKeyDown(pauseKey) || Input.GetKeyUp(pauseKey) ||
                Input.GetKeyDown(alternatePauseKey))
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
        if (freezeFrameRoutine != null)
        {
            StopCoroutine(freezeFrameRoutine);
        }

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

        if (playerRef != null)
        {
            PlayerInput pInput = playerRef.GetComponentInChildren<PlayerInput>();
            if (pInput != null) pInput.enabled = !lockInput;

            LG_Shoot pShoot = playerRef.GetComponentInChildren<LG_Shoot>();
            if (pShoot != null) pShoot.enabled = !lockInput;

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