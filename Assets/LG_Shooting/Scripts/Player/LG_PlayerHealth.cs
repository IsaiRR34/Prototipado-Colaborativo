using UnityEngine;
using System;
using UnityEngine.SceneManagement;

public class LG_PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private float maxHealth = 100f;
    private float currentHealth;

    [Header("Game Over Settings")]
    [Tooltip("Nombre exacto de la escena de Game Over en el Build Settings")]
    [SerializeField] private string gameOverSceneName = "GameOver";

    public event Action OnHealthChanged;
    public event Action OnPlayerDeath;

    private void Start()
    {
        currentHealth = maxHealth;
    }

    /// <summary>
    /// Deducts health and checks for death.
    /// </summary>
    /// <param name="damage">Amount of damage to receive.</param>
    public void TakeDamage(float damage)
    {
        if (currentHealth <= 0f) return;

        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);
        Debug.Log($"[LG_PlayerHealth] Player took {damage} damage! Current health: {currentHealth}/{maxHealth}", this);

        if (SoundList.Instance != null)
        {
            SoundList.Instance.PlaySound("SFX_Player_Hurt");
        }

        OnHealthChanged?.Invoke();

        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    /// <summary>
    /// Restores health up to max health.
    /// </summary>
    /// <param name="amount">Amount of health to restore.</param>
    public void Heal(float amount)
    {
        if (currentHealth <= 0f) return;

        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);
        OnHealthChanged?.Invoke();
    }

    /// <summary>
    /// Returns normalized health (0 to 1) for UI sliders.
    /// </summary>
    public float GetHealthNormalized()
    {
        return currentHealth / maxHealth;
    }

    public float GetCurrentHealth() => currentHealth;
    public float GetMaxHealth() => maxHealth;

    private void Die()
    {
        Debug.Log("[LG_PlayerHealth] Player has died! Loading Game Over...", this);
        OnPlayerDeath?.Invoke();

        // 1. Liberar el cursor para poder usar los botones en la pantalla de Game Over
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // 2. Detener música de fondo si es necesario
        if (SoundList.Instance != null)
        {
            SoundList.Instance.StopSound("BGM_Gameplay");
            SoundList.Instance.StopSound("BGM_Zona1"); // Agrega las zonas que necesites detener
            SoundList.Instance.StopSound("BGM_Zona2");
        }

        // 3. Cargar la escena de derrota
        SceneManager.LoadScene(gameOverSceneName);
    }
}