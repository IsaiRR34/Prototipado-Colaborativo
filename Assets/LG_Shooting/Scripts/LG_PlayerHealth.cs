using UnityEngine;
using System;

public class LG_PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private float maxHealth = 100f;
    private float currentHealth;

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
        Debug.Log("[LG_PlayerHealth] Player has died!", this);
        OnPlayerDeath?.Invoke();

        // Respawn sequence (returns to starting position and refills health)
        currentHealth = maxHealth;
        OnHealthChanged?.Invoke();
        
        transform.position = new Vector3(0f, 1f, 0f);
        
        // Reset rotation
        transform.rotation = Quaternion.identity;

        // If there's a CharacterController, we should temporarily disable it to avoid teleport physics conflicts
        CharacterController cc = GetComponent<CharacterController>();
        if (cc != null)
        {
            cc.enabled = false;
            transform.position = new Vector3(0f, 1f, 0f);
            cc.enabled = true;
        }
    }
}
