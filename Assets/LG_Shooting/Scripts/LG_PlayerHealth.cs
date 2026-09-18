using UnityEngine;
using System;

public class LG_PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private float maxHealth = 100f;
    private float currentHealth;

    [Header("Efectos de Audio (SFX)")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip hurtSound;

    public event Action OnHealthChanged;
    public event Action OnPlayerDeath;

    private void Start()
    {
        currentHealth = maxHealth;

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0f;
        }

#if UNITY_EDITOR
        if (hurtSound == null)
        {
            hurtSound = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/LG_Shooting/LGAssets/Audio/SFX_Player_Hurt.wav");
        }
#endif
    }

    public void TakeDamage(float damage)
    {
        if (currentHealth <= 0f) return;

        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);
        Debug.Log($"[LG_PlayerHealth] Player took {damage} damage! Current health: {currentHealth}/{maxHealth}", this);

        // Sonido directo original
        if (audioSource != null && hurtSound != null)
        {
            audioSource.PlayOneShot(hurtSound, 1.0f);
        }

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

    public void Heal(float amount)
    {
        if (currentHealth <= 0f) return;

        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);
        OnHealthChanged?.Invoke();
    }

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

        // Secuencia de reaparición desde el punto de control de Sala Segura
        currentHealth = maxHealth;
        OnHealthChanged?.Invoke();
        
        Vector3 respawnPos = SafeRoomCheckpoint.LastSafePosition;

        CharacterController cc = GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;
        
        transform.position = respawnPos;
        transform.rotation = Quaternion.identity;

        if (cc != null) cc.enabled = true;

        if (LG_TooltipManager.Instance != null)
        {
            LG_TooltipManager.Instance.ShowTooltipTemporary("Reapareciendo en Sala Segura...", 2f);
        }
    }
}
