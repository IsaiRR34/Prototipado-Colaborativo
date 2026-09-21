using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using DG.Tweening;

public class BossBabyController : MonoBehaviour
{
    public static bool IsBossDefeated { get; private set; } = false;

    public enum BossPhase
    {
        Phase1_Stalking,
        Phase2_Enraged,
        Stunned,
        Dead
    }

    [Header("Atributos del Jefe")]
    [SerializeField] private float maxHealth = 35f;
    private float currentHealth;
    [SerializeField] private string bossName = "El Bebé Maldito";

    [Header("Referencias de Combate")]
    [SerializeField] private Transform targetPlayer;
    [SerializeField] private Transform projectileSpawnPoint;
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform headTransform;

    [Header("Puntos de Acecho en Muros (Fase 1)")]
    [SerializeField] private Transform[] wallPerchPoints;
    private int currentPerchIndex = 0;

    [Header("Embestida en Línea Recta (Fase 2)")]
    [SerializeField] private float chargeSpeed = 18f;
    [SerializeField] private float chargeDamage = 30f;
    [SerializeField] private float stunDuration = 4.0f; // Conforme al GDD
    private Vector3 chargeDirection;
    private bool isCharging = false;

    [Header("UI Barra de Salud del Jefe")]
    [SerializeField] private GameObject bossUIRoot;
    [SerializeField] private Slider bossHealthSlider;
    [SerializeField] private TextMeshProUGUI bossNameText;

    [Header("Feedback Visual")]
    [SerializeField] private Renderer[] bossRenderers;
    [SerializeField] private Color enrageColor = new Color(0.9f, 0.1f, 0.1f);
    private Color[] originalColors;

    private BossPhase currentPhase = BossPhase.Phase1_Stalking;
    private CharacterController characterController;
    private bool isCasting = false;

    public BossPhase CurrentPhase => currentPhase;
    public float HealthNormalized => currentHealth / maxHealth;

    private void Awake()
    {
        IsBossDefeated = false;
        currentHealth = maxHealth;
        characterController = GetComponent<CharacterController>();

        bossRenderers = GetComponentsInChildren<Renderer>();
        originalColors = new Color[bossRenderers.Length];
        for (int i = 0; i < bossRenderers.Length; i++)
        {
            if (bossRenderers[i] != null) originalColors[i] = bossRenderers[i].material.color;
        }
    }

    private void Start()
    {
        if (targetPlayer == null)
        {
            var p = GameObject.Find("Player (RI + LG)");
            if (p != null) targetPlayer = p.transform;
        }

        SetupBossUI();
        StartCoroutine(BossBehaviorLoop());
    }

    private void SetupBossUI()
    {
        if (bossUIRoot == null)
        {
            var ui = GameObject.Find("BossHealthBarContainer");
            if (ui != null) bossUIRoot = ui;
        }

        if (bossHealthSlider == null && bossUIRoot != null)
        {
            bossHealthSlider = bossUIRoot.GetComponentInChildren<Slider>();
        }

        if (bossNameText == null && bossUIRoot != null)
        {
            bossNameText = bossUIRoot.GetComponentInChildren<TextMeshProUGUI>();
        }

        if (bossNameText != null) bossNameText.text = bossName;
        if (bossHealthSlider != null) bossHealthSlider.value = 1f;
        if (bossUIRoot != null) bossUIRoot.SetActive(true);
    }

    private IEnumerator BossBehaviorLoop()
    {
        while (currentPhase != BossPhase.Dead)
        {
            if (currentPhase == BossPhase.Phase1_Stalking)
            {
                yield return StartCoroutine(Phase1StalkRoutine());
            }
            else if (currentPhase == BossPhase.Phase2_Enraged)
            {
                yield return StartCoroutine(Phase2ChargeRoutine());
            }
            else if (currentPhase == BossPhase.Stunned)
            {
                yield return new WaitForSeconds(0.2f);
            }

            yield return null;
        }
    }

    // --- FASE 1: ACECHO Y PROYECTILES ---
    private IEnumerator Phase1StalkRoutine()
    {
        // Si hay puntos en paredes, reposicionar
        if (wallPerchPoints != null && wallPerchPoints.Length > 0)
        {
            currentPerchIndex = (currentPerchIndex + 1) % wallPerchPoints.Length;
            Transform targetPerch = wallPerchPoints[currentPerchIndex];

            transform.DOMove(targetPerch.position, 1.2f).SetEase(Ease.InOutSine);
            yield return new WaitForSeconds(1.3f);
        }

        // Mirar hacia el jugador y preparar proyectil
        if (targetPlayer != null)
        {
            Vector3 lookDir = (targetPlayer.position - transform.position).normalized;
            lookDir.y = 0f;
            if (lookDir != Vector3.zero) transform.rotation = Quaternion.LookRotation(lookDir);
        }

        isCasting = true;
        LG_TooltipManager.Instance?.ShowTooltipTemporary("<color=#EF4444>¡EL JEFE ESTÁ CANALIZANDO! ¡DISPARA A LA CABEZA!</color>", 1.5f);

        float timer = 0f;
        while (timer < 1.8f && isCasting && currentPhase == BossPhase.Phase1_Stalking)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        if (isCasting && currentPhase == BossPhase.Phase1_Stalking)
        {
            FireProjectile();
            yield return new WaitForSeconds(1.0f);
        }

        isCasting = false;
    }

    private void FireProjectile()
    {
        Vector3 spawnPos = projectileSpawnPoint != null ? projectileSpawnPoint.position : transform.position + Vector3.up * 1.5f;
        Vector3 targetPos = targetPlayer != null ? targetPlayer.position + Vector3.up * 1f : transform.position + transform.forward * 5f;
        Vector3 dir = (targetPos - spawnPos).normalized;

        if (projectilePrefab != null)
        {
            GameObject projGO = Instantiate(projectilePrefab, spawnPos, Quaternion.LookRotation(dir));
            BossProjectile bp = projGO.GetComponent<BossProjectile>();
            if (bp != null) bp.Initialize(dir);
        }
        else
        {
            // Fallback dinámico si no hay prefab asignado
            GameObject tempProj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            tempProj.name = "BossRitualSphere";
            tempProj.transform.position = spawnPos;
            tempProj.transform.localScale = Vector3.one * 0.7f;
            tempProj.GetComponent<Renderer>().material.color = new Color(0.8f, 0.1f, 0.9f);
            var col = tempProj.GetComponent<Collider>();
            if (col != null) col.isTrigger = true;

            BossProjectile bp = tempProj.AddComponent<BossProjectile>();
            bp.Initialize(dir);
        }

        if (SoundList.Instance != null) SoundList.Instance.PlaySound("SFX_Shoot");
    }

    // --- FASE 2: EMBESTIDA / FURIA Y ATURDIMIENTO ---
    private IEnumerator Phase2ChargeRoutine()
    {
        if (targetPlayer == null) yield break;

        // Bajar al suelo si estaba elevado
        Vector3 groundTarget = new Vector3(transform.position.x, 1f, transform.position.z);
        transform.position = groundTarget;

        // Fijar dirección recta hacia el jugador en el suelo
        Vector3 dir = (targetPlayer.position - transform.position);
        dir.y = 0f;
        chargeDirection = dir.normalized;
        if (chargeDirection != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(chargeDirection);
        }

        LG_TooltipManager.Instance?.ShowTooltipTemporary("<color=#EF4444><b>¡EMBESTIDA DEL JEFE! ¡ESQUIVA PARA QUE SE ESTRELLE!</b></color>", 1.5f);
        if (SoundList.Instance != null) SoundList.Instance.PlaySound("SFX_Zombie_Hit");

        yield return new WaitForSeconds(0.8f);

        isCharging = true;
        float maxChargeDuration = 2.5f;
        float elapsed = 0f;

        while (isCharging && elapsed < maxChargeDuration && currentPhase == BossPhase.Phase2_Enraged)
        {
            elapsed += Time.deltaTime;
            Vector3 move = chargeDirection * chargeSpeed * Time.deltaTime;

            if (characterController != null && characterController.enabled)
            {
                characterController.Move(move);
            }
            else
            {
                transform.position += move;
            }

            // Chequeo de colisión frontal contra muros / pilares
            if (Physics.Raycast(transform.position + Vector3.up * 1f, chargeDirection, out RaycastHit hit, 1.8f))
            {
                if (!hit.collider.isTrigger && hit.collider.GetComponentInParent<LG_PlayerHealth>() == null)
                {
                    StartCoroutine(CrashAndStun(hit.point));
                    yield break;
                }
            }

            yield return null;
        }

        isCharging = false;
        yield return new WaitForSeconds(1.2f);
    }

    private IEnumerator CrashAndStun(Vector3 hitPoint)
    {
        isCharging = false;
        currentPhase = BossPhase.Stunned;

        // Screen Shake por el impacto
        Camera.main?.transform.DOShakePosition(0.4f, 0.6f, 18);
        if (SoundList.Instance != null) SoundList.Instance.PlaySound("SFX_Melee_Hit");

        LG_TooltipManager.Instance?.ShowTooltipTemporary("<color=#10B981><b>¡JEFE ATURDIDO! ¡4 SEGUNDOS DE MÁXIMO DAÑO!</b></color>", 3.8f);

        // Animación de mareo / inclinación con DOTween
        transform.DORotate(new Vector3(15f, transform.eulerAngles.y, 25f), 0.2f);

        yield return new WaitForSeconds(stunDuration);

        // Recuperación de aturdimiento
        transform.DORotate(new Vector3(0f, transform.eulerAngles.y, 0f), 0.3f);
        currentPhase = BossPhase.Phase2_Enraged;
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (isCharging)
        {
            // Daño al jugador si es alcanzado durante la carga
            LG_PlayerHealth pHealth = hit.collider.GetComponentInParent<LG_PlayerHealth>();
            if (pHealth != null)
            {
                pHealth.TakeDamage(chargeDamage);
                isCharging = false;
                LG_TooltipManager.Instance?.ShowTooltipTemporary("¡Impacto frontal del Bebé Maldito!", 1.5f);
                return;
            }

            // Si choca con un pilar o pared
            if (!hit.collider.isTrigger)
            {
                StartCoroutine(CrashAndStun(hit.point));
            }
        }
    }

    public void TakeDamage(float damage, bool isHeadshot)
    {
        if (currentPhase == BossPhase.Dead) return;

        float finalDmg = damage;

        // En Fase 1, un disparo a la cabeza interrumpe el lanzamiento de proyectil
        if (currentPhase == BossPhase.Phase1_Stalking && isHeadshot)
        {
            finalDmg *= 2.5f;
            InterruptCast();
        }

        // Si está aturdido en Fase 2, recibe daño crítico aumentado
        if (currentPhase == BossPhase.Stunned)
        {
            finalDmg *= 2.0f;
        }

        currentHealth -= finalDmg;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);

        UpdateHealthUI();
        FlashBoss();

        // Chequeo de transición a Fase 2 al llegar al 50% de vida
        if (currentHealth <= (maxHealth * 0.5f) && currentPhase == BossPhase.Phase1_Stalking)
        {
            EnterPhase2Enrage();
        }

        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    private void InterruptCast()
    {
        if (isCasting)
        {
            isCasting = false;
            LG_TooltipManager.Instance?.ShowTooltipTemporary("<color=#38BDF8><b>¡LANZAMIENTO INTERRUMPIDO!</b></color>", 1.5f);
            Camera.main?.transform.DOShakePosition(0.15f, 0.2f, 10);
            if (SoundList.Instance != null) SoundList.Instance.PlaySound("SFX_Zombie_Hit");
        }
    }

    private void EnterPhase2Enrage()
    {
        currentPhase = BossPhase.Phase2_Enraged;
        isCasting = false;

        LG_TooltipManager.Instance?.ShowTooltipTemporary("<color=#EF4444><b>¡EL BEBÉ MALDITO ENTRA EN FURIA! (FASE 2)</b></color>", 3f);

        for (int i = 0; i < bossRenderers.Length; i++)
        {
            if (bossRenderers[i] != null) bossRenderers[i].material.color = enrageColor;
        }

        Camera.main?.transform.DOShakePosition(0.5f, 0.5f, 15);
        if (SoundList.Instance != null) SoundList.Instance.PlaySound("SFX_Zombie_Hit");
    }

    private void FlashBoss()
    {
        for (int i = 0; i < bossRenderers.Length; i++)
        {
            if (bossRenderers[i] != null)
            {
                bossRenderers[i].material.DOColor(Color.white, 0.08f)
                    .OnComplete(() =>
                    {
                        if (bossRenderers[i] != null)
                            bossRenderers[i].material.color = (currentPhase == BossPhase.Phase2_Enraged) ? enrageColor : originalColors[i];
                    });
            }
        }
    }

    private void UpdateHealthUI()
    {
        if (bossHealthSlider != null)
        {
            bossHealthSlider.value = HealthNormalized;
        }
    }

    private void Die()
    {
        currentPhase = BossPhase.Dead;
        IsBossDefeated = true;
        isCharging = false;
        StopAllCoroutines();

        if (bossUIRoot != null) bossUIRoot.SetActive(false);

        LG_TooltipManager.Instance?.ShowTooltipTemporary("<color=#F59E0B><b>¡EL BEBÉ MALDITO HA SIDO DESTRUIDO!</b></color>", 4f);
        if (SoundList.Instance != null) SoundList.Instance.PlaySound("SFX_Door_Unlock");

        DOVirtual.DelayedCall(2.5f, () =>
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("Victory");
        });

        Destroy(gameObject, 0.5f);
    }
}
