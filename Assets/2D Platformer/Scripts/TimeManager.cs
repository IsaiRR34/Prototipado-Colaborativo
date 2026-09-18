using UnityEngine;
using UnityEngine.InputSystem;
using DG.Tweening;
using System.Collections;

public class TimeManager : MonoBehaviour
{
    public static TimeManager Instance { get; private set; }

    [Header("Configuración de Tecla")]
    [SerializeField] private KeyCode pauseKey = KeyCode.Escape;

    [Header("UI Referencias")]
    [SerializeField] private CanvasGroup pauseScreen;
    [SerializeField] private float pauseTweenTime = 0.25f;

    private bool isPaused = false;
    private Tween pauseTween;
    private Coroutine freezeFrameRoutine;

    // Referencia al Player para congelar controles de cámara/movimiento
    private GameObject playerRef;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        if (pauseScreen != null)
        {
            pauseScreen.alpha = 0f;
            pauseScreen.blocksRaycasts = false;
            pauseScreen.interactable = false;
        }

        playerRef = GameObject.Find("Player (RI + LG)");
        if (playerRef == null)
        {
            playerRef = GameObject.FindGameObjectWithTag("Player");
        }
    }

    private void Update()
    {
        // No abrir pausa si un diálogo está activo en pantalla
        if (LG_DialogueManager.IsDialogueActive) return;

        if (Input.GetKeyDown(pauseKey))
        {
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

        if (isPaused)
        {
            Time.timeScale = 0f;
            LockPlayerForTransition(true);

            if (pauseScreen != null)
            {
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
                    .OnComplete(() => Time.timeScale = 1f);
            }
            else
            {
                Time.timeScale = 1f;
            }
        }
    }

    public void LockPlayerForTransition(bool lockInput)
    {
        if (playerRef != null)
        {
            PlayerInput pInput = playerRef.GetComponentInChildren<PlayerInput>();
            if (pInput != null) pInput.enabled = !lockInput;

            LG_Shoot pShoot = playerRef.GetComponentInChildren<LG_Shoot>();
            if (pShoot != null) pShoot.enabled = !lockInput;
        }

        // Manejo del cursor para navegar por el menú de pausa
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