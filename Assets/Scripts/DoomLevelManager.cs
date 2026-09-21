using UnityEngine;
using UnityEngine.SceneManagement;

public class DoomLevelManager : MonoBehaviour
{
    public static DoomLevelManager Instance { get; private set; }

    [Header("Estado de Progresión")]
    [SerializeField] private int currentZoneIndex = 1;
    [SerializeField] private int killsInZone = 0;
    [SerializeField] private int totalEnemiesInZone = 0;
    [SerializeField] private int itemsCollectedInZone = 0;
    [SerializeField] private float zoneTimer = 0f;
    [SerializeField] private bool timerActive = true;

    [Header("Referencias")]
    [SerializeField] private DoomIntermissionUI intermissionUI;

    public int CurrentZoneIndex => currentZoneIndex;
    public int KillsInZone => killsInZone;
    public int TotalEnemiesInZone => totalEnemiesInZone;
    public int ItemsCollectedInZone => itemsCollectedInZone;
    public float ZoneTimer => zoneTimer;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        ResetZoneStats();
    }

    private void Update()
    {
        if (timerActive)
        {
            zoneTimer += Time.deltaTime;
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (intermissionUI == null)
        {
            intermissionUI = Object.FindFirstObjectByType<DoomIntermissionUI>();
        }
        ResetZoneStats();
    }

    public void RegisterEnemySpawned()
    {
        totalEnemiesInZone++;
    }

    public void RegisterEnemyKilled()
    {
        killsInZone++;
    }

    public void RegisterItemCollected()
    {
        itemsCollectedInZone++;
    }

    public void CompleteCurrentLevel(int nextLevelIndex)
    {
        timerActive = false;

        if (intermissionUI == null)
        {
            intermissionUI = Object.FindFirstObjectByType<DoomIntermissionUI>();
        }

        if (intermissionUI != null)
        {
            intermissionUI.ShowIntermission(currentZoneIndex, killsInZone, totalEnemiesInZone, itemsCollectedInZone, zoneTimer, nextLevelIndex);
        }
        else
        {
            LoadNextLevel(nextLevelIndex);
        }
    }

    public void LoadNextLevel(int nextLevelIndex)
    {
        currentZoneIndex++;
        ResetZoneStats();

        if (nextLevelIndex < SceneManager.sceneCountInBuildSettings && nextLevelIndex >= 0)
        {
            SceneManager.LoadScene(nextLevelIndex);
        }
        else
        {
            SceneManager.LoadScene("Victory");
        }
    }

    public void ResetZoneStats()
    {
        killsInZone = 0;
        totalEnemiesInZone = 0;
        itemsCollectedInZone = 0;
        zoneTimer = 0f;
        timerActive = true;
    }
}
