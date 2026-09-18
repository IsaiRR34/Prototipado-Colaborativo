using UnityEngine;
using System.Collections;
using DG.Tweening;

public class Hand : MonoBehaviour
{
    [Header("Referencias de Armas")]
    [SerializeField] private LG_Shoot shootScript;
    [SerializeField] private LG_Inventory playerInventory;
    public GameObject sword; // Bate de béisbol
    public GameObject flashLight;
    public GameObject gun;

    [Header("Configuración de Ataque Melee")]
    [SerializeField] private float attackDamage = 2.5f;
    [SerializeField] private float attackRange = 2.6f;
    [SerializeField] private float attackRate = 0.4f;
    [SerializeField] private float hitForce = 12f;

    [Header("Durabilidad del Bate")]
    [SerializeField] private float batMaxDurability = 100f;
    private float batDurability;
    [SerializeField] private float batLossPerHit = 10f;

    [Header("Combustible / Durabilidad de Mechero / Antorcha")]
    [SerializeField] private Light torchLight;
    [SerializeField] private float torchMaxFuel = 100f;
    private float torchFuel;
    [SerializeField] private float torchBurnRate = 1.5f;

    [Header("Efectos de Audio (SFX)")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip swingSound;
    [SerializeField] private AudioClip hitSound;

    private bool canHit;
    private bool isTorchActive;
    private float nextAttackTime = 0f;
    private bool isSwinging = false;
    private Quaternion swordOriginalRot;
    private Vector3 swordOriginalPos;
    private Camera playerCamera;

    public float BatDurabilityNormalized => batDurability / batMaxDurability;
    public float TorchFuelNormalized => torchFuel / torchMaxFuel;
    public bool IsTorchOn => isTorchActive;

    private void Awake()
    {
        batDurability = batMaxDurability;
        torchFuel = torchMaxFuel;
    }

    private void Start()
    {
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }

#if UNITY_EDITOR
        if (swingSound == null) swingSound = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/LG_Shooting/LGAssets/Audio/SFX_Melee_Swing.wav");
        if (hitSound == null) hitSound = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/LG_Shooting/LGAssets/Audio/SFX_Melee_Hit.wav");
#endif

        if (shootScript == null)
        {
            shootScript = GetComponentInChildren<LG_Shoot>();
            if (shootScript == null) shootScript = GetComponentInParent<LG_Shoot>();
        }

        if (playerInventory == null)
        {
            playerInventory = GetComponent<LG_Inventory>();
            if (playerInventory == null) playerInventory = GetComponentInParent<LG_Inventory>();
        }

        playerCamera = Camera.main;
        if (playerCamera == null) playerCamera = GetComponentInParent<Camera>();

        if (sword != null)
        {
            swordOriginalRot = sword.transform.localRotation;
            swordOriginalPos = sword.transform.localPosition;
        }

        if (flashLight != null && torchLight == null)
        {
            torchLight = flashLight.GetComponentInChildren<Light>();
        }

        DefaultGun();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) DefaultGun();
        if (Input.GetKeyDown(KeyCode.Alpha3)) EquipBat();

        if (Input.GetKeyDown(KeyCode.F))
        {
            ToggleTorch();
        }

        if (isTorchActive)
        {
            torchFuel -= torchBurnRate * Time.deltaTime;
            if (torchFuel <= 0f)
            {
                torchFuel = 0f;
                SetTorchLightState(false);
                LG_TooltipManager.Instance?.ShowTooltipTemporary("Linterna agotada. Repara con Recursos.", 2f);
            }
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            TryRepairEquipment();
        }

        bool quickMeleePressed = Input.GetKeyDown(KeyCode.V) || Input.GetMouseButtonDown(2);
        bool batAttackPressed = canHit && Input.GetMouseButtonDown(0);

        if ((quickMeleePressed || batAttackPressed) && !isSwinging && Time.time >= nextAttackTime)
        {
            nextAttackTime = Time.time + attackRate;
            StartCoroutine(PerformMeleeSwing(quickMeleePressed && !canHit));
        }
    }

    public void DefaultGun()
    {
        CancelSwing();
        canHit = false;

        if (sword != null) sword.SetActive(false);
        if (gun != null) gun.SetActive(true);
        if (shootScript != null) shootScript.EnableShooting(true);
    }

    public void EquipBat()
    {
        CancelSwing();
        canHit = true;

        if (sword != null) sword.SetActive(true);
        if (gun != null) gun.SetActive(false);
        if (shootScript != null) shootScript.EnableShooting(false);
    }

    public void ToggleTorch()
    {
        if (torchFuel <= 0f)
        {
            LG_TooltipManager.Instance?.ShowTooltipTemporary("Linterna sin combustible. Usa [R] con Recursos.", 1.5f);
            return;
        }

        isTorchActive = !isTorchActive;
        SetTorchLightState(isTorchActive);

        if (SoundList.Instance != null) SoundList.Instance.PlaySound("SFX_Pickup");
    }

    private void SetTorchLightState(bool active)
    {
        if (flashLight != null) flashLight.SetActive(active);
        if (torchLight != null) torchLight.enabled = active;
    }

    public void TryRepairEquipment()
    {
        if (playerInventory == null) return;

        bool repairedAnything = false;

        if (batDurability < batMaxDurability)
        {
            float rep = playerInventory.ConsumeRepairResource(batMaxDurability - batDurability);
            if (rep > 0f)
            {
                batDurability = Mathf.Min(batMaxDurability, batDurability + rep);
                repairedAnything = true;
                LG_TooltipManager.Instance?.ShowTooltipTemporary($"Bate reparado ({Mathf.RoundToInt(batDurability)}%)", 1.5f);
            }
        }

        if (torchFuel < torchMaxFuel)
        {
            float rep = playerInventory.ConsumeRepairResource(torchMaxFuel - torchFuel);
            if (rep > 0f)
            {
                torchFuel = Mathf.Min(torchMaxFuel, torchFuel + rep);
                repairedAnything = true;
                LG_TooltipManager.Instance?.ShowTooltipTemporary($"Linterna recargada ({Mathf.RoundToInt(torchFuel)}%)", 1.5f);
            }
        }

        if (!repairedAnything)
        {
            LG_TooltipManager.Instance?.ShowTooltipTemporary("No tienes 'Recursos' para reparar.", 1.5f);
        }
        else if (SoundList.Instance != null)
        {
            SoundList.Instance.PlaySound("SFX_Pickup");
        }
    }

    private void CancelSwing()
    {
        if (isSwinging)
        {
            StopAllCoroutines();
            if (sword != null)
            {
                sword.transform.localRotation = swordOriginalRot;
                sword.transform.localPosition = swordOriginalPos;
            }
            isSwinging = false;
        }
    }

    private IEnumerator PerformMeleeSwing(bool isQuickMelee)
    {
        isSwinging = true;

        if (isQuickMelee && sword != null)
        {
            sword.SetActive(true);
            if (gun != null) gun.SetActive(false);
        }

        // Reproducir sonido directo original
        if (audioSource != null && swingSound != null)
        {
            audioSource.PlayOneShot(swingSound, 0.85f);
        }
        if (SoundList.Instance != null)
        {
            SoundList.Instance.PlaySound("SFX_Melee_Swing");
        }

        if (playerCamera == null) playerCamera = Camera.main;

        float duration = 0.22f;
        float elapsed = 0f;
        bool hitRegistered = false;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            float swingAngle = Mathf.Sin(t * Mathf.PI) * 75f;
            float forwardThrust = Mathf.Sin(t * Mathf.PI) * 0.3f;

            if (sword != null)
            {
                sword.transform.localRotation = swordOriginalRot * Quaternion.Euler(swingAngle, -swingAngle * 0.6f, 0f);
                sword.transform.localPosition = swordOriginalPos + Vector3.forward * forwardThrust;
            }

            if (t >= 0.45f && !hitRegistered)
            {
                hitRegistered = true;
                CheckMeleeHit();
            }

            yield return null;
        }

        if (sword != null)
        {
            sword.transform.localRotation = swordOriginalRot;
            sword.transform.localPosition = swordOriginalPos;
        }

        if (isQuickMelee)
        {
            if (sword != null) sword.SetActive(false);
            if (gun != null) gun.SetActive(true);
        }

        isSwinging = false;
    }

    private void CheckMeleeHit()
    {
        Vector3 origin = (playerCamera != null) ? playerCamera.transform.position : transform.position;
        Vector3 direction = (playerCamera != null) ? playerCamera.transform.forward : transform.forward;

        if (Physics.SphereCast(origin, 0.45f, direction, out RaycastHit hit, attackRange))
        {
            // Reproducir sonido directo original
            if (audioSource != null && hitSound != null)
            {
                audioSource.PlayOneShot(hitSound, 0.95f);
            }
            if (SoundList.Instance != null)
            {
                SoundList.Instance.PlaySound("SFX_Melee_Hit");
            }

            batDurability = Mathf.Max(0f, batDurability - batLossPerHit);
            float effectiveDamage = batDurability > 0f ? attackDamage : 0.6f;

            if (batDurability <= 0f)
            {
                LG_TooltipManager.Instance?.ShowTooltipTemporary("¡Bate desgastado! Daño reducido. [R] para reparar.", 1.8f);
            }

            LG_Enemy enemy = hit.collider.GetComponentInParent<LG_Enemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(effectiveDamage);
            }

            BossBabyController boss = hit.collider.GetComponentInParent<BossBabyController>();
            if (boss != null)
            {
                boss.TakeDamage(effectiveDamage, false);
            }

            Rigidbody rb = hit.rigidbody != null ? hit.rigidbody : hit.collider.attachedRigidbody;
            if (rb != null && !rb.isKinematic)
            {
                rb.AddForce(direction * hitForce, ForceMode.Impulse);
            }
        }
    }
}
