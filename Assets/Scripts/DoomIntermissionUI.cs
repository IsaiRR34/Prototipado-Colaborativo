using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using DG.Tweening;

public class DoomIntermissionUI : MonoBehaviour
{
    public static DoomIntermissionUI Instance { get; private set; }

    [Header("Contenedores UI")]
    [SerializeField] private GameObject intermissionPanel;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI killsText;
    [SerializeField] private TextMeshProUGUI itemsText;
    [SerializeField] private TextMeshProUGUI timeText;
    [SerializeField] private Button continueButton;
    [SerializeField] private TextMeshProUGUI continueButtonText;

    private int targetSceneIndex = 2;
    private bool isShowing = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        if (intermissionPanel != null) intermissionPanel.SetActive(false);
    }

    private void Start()
    {
        if (continueButton != null)
        {
            continueButton.onClick.AddListener(OnContinueClicked);
        }
    }

    private void Update()
    {
        if (isShowing)
        {
            if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.E))
            {
                OnContinueClicked();
            }
        }
    }

    public void ShowIntermission(int zoneIndex, int kills, int totalEnemies, int items, float seconds, int nextIndex)
    {
        targetSceneIndex = nextIndex;
        isShowing = true;

        if (intermissionPanel != null) intermissionPanel.SetActive(true);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Time.timeScale = 0f; // Pausar simulación del mundo durante la intermisión

        if (titleText != null)
        {
            titleText.text = $"<b><color=#F59E0B>SECTOR {zoneIndex} DESPEJADO</color></b>\n<size=18><color=#94A3B8>INFORME DE COMBATE</color></size>";
        }

        StartCoroutine(AnimateStatsRoutine(kills, totalEnemies, items, seconds));
    }

    private IEnumerator AnimateStatsRoutine(int kills, int totalEnemies, int items, float seconds)
    {
        if (killsText != null) killsText.text = "ENEMIGOS: CALCULANDO...";
        if (itemsText != null) itemsText.text = "RECURSOS: CALCULANDO...";
        if (timeText != null) timeText.text = "TIEMPO: CALCULANDO...";

        yield return new WaitForSecondsRealtime(0.4f);

        int killPercent = totalEnemies > 0 ? Mathf.RoundToInt(((float)kills / totalEnemies) * 100f) : 100;
        if (killsText != null)
        {
            killsText.text = $"ENEMIGOS ELIMINADOS: <color=#EF4444><b>{kills}</b> / {totalEnemies} ({killPercent}%)</color>";
            if (SoundList.Instance != null) SoundList.Instance.PlaySound("SFX_Melee_Hit");
        }

        yield return new WaitForSecondsRealtime(0.4f);

        if (itemsText != null)
        {
            itemsText.text = $"RECURSOS RECOGIDOS: <color=#38BDF8><b>{items}</b></color>";
            if (SoundList.Instance != null) SoundList.Instance.PlaySound("SFX_Pickup");
        }

        yield return new WaitForSecondsRealtime(0.4f);

        int mins = Mathf.FloorToInt(seconds / 60f);
        int secs = Mathf.FloorToInt(seconds % 60f);
        if (timeText != null)
        {
            timeText.text = $"TIEMPO DE INCURSIÓN: <color=#10B981><b>{mins:00}:{secs:00}</b></color>";
            if (SoundList.Instance != null) SoundList.Instance.PlaySound("SFX_Pickup_Key");
        }

        if (continueButtonText != null)
        {
            continueButtonText.text = "CONTINUAR [ESPACIO]";
        }
    }

    public void OnContinueClicked()
    {
        Time.timeScale = 1f;
        isShowing = false;
        if (intermissionPanel != null) intermissionPanel.SetActive(false);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (DoomLevelManager.Instance != null)
        {
            DoomLevelManager.Instance.LoadNextLevel(targetSceneIndex);
        }
        else
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(targetSceneIndex);
        }
    }
}
