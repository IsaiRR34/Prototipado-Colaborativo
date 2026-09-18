using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using DG.Tweening;

public class LG_Shoot : MonoBehaviour
{
    [Header("Referencias de Disparo")]
    [SerializeField] private LG_ObjectPool bulletPool;
    [SerializeField] private Transform firePoint;
    [SerializeField] private Camera playerCamera;
    [SerializeField] private Transform weaponTransform;

    [Header("Configuración de Arma (Estilo DOOM - Munición Directa)")]
    [SerializeField] private float fireRate = 0.22f;

    [Header("Apuntado Dinámico (ADS)")]
    [SerializeField] private bool enableADS = true;
    [SerializeField] private float adsFov = 45f;
    [SerializeField] private float defaultFov = 65f;
    [SerializeField] private float adsSpeed = 10f;
    [SerializeField] private Vector3 hipPos = new Vector3(0.28f, -0.25f, 0.45f);
    [SerializeField] private Vector3 adsPos = new Vector3(0f, -0.18f, 0.38f);
    private bool isAiming = false;

    [Header("Retroceso y Game Feel (Recoil & Shake)")]
    [SerializeField] private float recoilVerticalKick = 1.8f;
    [SerializeField] private float recoilHorizontalKick = 0.4f;
    [SerializeField] private float recoilRecoverySpeed = 8f;
    [SerializeField] private float screenShakeStrength = 0.12f;
    private float currentRecoilX = 0f;
    private float currentRecoilY = 0f;

    [Header("Sistema de Munición")]
    [SerializeField] private LG_Inventory playerInventory;
    [SerializeField] private string ammoItemName = "Munición";

    [Header("UI HUD")]
    [SerializeField] private TextMeshProUGUI ammoText;
    [SerializeField] private GameObject crosshairUI;

    [Header("Efectos de Audio (SFX)")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip shootSound;
    [SerializeField] private AudioClip reloadSound;
    [SerializeField] private AudioClip emptySound;

    [Header("Efectos")]
    [SerializeField] private Light muzzleFlashLight;

    [Header("Inputs")]
    [SerializeField] private InputActionReference shootAction;
    private InputAction shootActionInstance;

    private float fireRateTimer = 0f;
    private bool canShoot = true;

    private void Awake()
    {
        if (playerCamera == null) playerCamera = Camera.main;
        if (playerCamera == null) playerCamera = GetComponentInChildren<Camera>();

        if (playerCamera != null)
        {
            defaultFov = playerCamera.fieldOfView;
        }

        var playerInput = GetComponentInParent<PlayerInput>();
        if (playerInput != null && playerInput.actions != null)
        {
            shootActionInstance = playerInput.actions.FindAction("Shoot");
        }
        if (shootActionInstance == null && shootAction != null)
        {
            shootActionInstance = shootAction.action;
        }
    }

    private void Start()
    {
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0f; // Audio 2D estéreo local
        }

#if UNITY_EDITOR
        if (shootSound == null) shootSound = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/LG_Shooting/LGAssets/Audio/SFX_Shoot.wav");
        if (reloadSound == null) reloadSound = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/LG_Shooting/LGAssets/Audio/SFX_Reload.wav");
        if (emptySound == null) emptySound = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/LG_Shooting/LGAssets/Audio/SFX_Empty.wav");
#endif

        if (playerInventory == null)
        {
            playerInventory = GetComponentInParent<LG_Inventory>();
            if (playerInventory == null) playerInventory = Object.FindFirstObjectByType<LG_Inventory>();
        }

        if (crosshairUI == null)
        {
            var crossGO = GameObject.Find("Crosshair");
            if (crossGO != null) crosshairUI = crossGO;
        }

        if (ammoText == null)
        {
            GameObject container = GameObject.Find("MunicionContainer");
            if (container != null) ammoText = container.GetComponentInChildren<TextMeshProUGUI>();
            if (ammoText == null)
            {
                GameObject panel = GameObject.Find("MunicionPannel");
                if (panel != null) ammoText = panel.GetComponentInChildren<TextMeshProUGUI>();
            }
        }

        if (playerInventory != null)
        {
            playerInventory.OnInventoryChanged += UpdateAmmoUI;
        }

        UpdateAmmoUI();
    }

    private void OnDestroy()
    {
        if (playerInventory != null)
        {
            playerInventory.OnInventoryChanged -= UpdateAmmoUI;
        }
    }

    private void OnEnable() => shootActionInstance?.Enable();
    private void OnDisable() => shootActionInstance?.Disable();

    public void EnableShooting(bool enable)
    {
        canShoot = enable;
        if (ammoText != null)
        {
            Transform parent = ammoText.transform.parent;
            if (parent != null && (parent.name == "MunicionPannel" || parent.name == "MunicionContainer"))
            {
                if (parent.parent != null && parent.parent.name == "MunicionContainer")
                    parent.parent.gameObject.SetActive(enable);
                else
                    parent.gameObject.SetActive(enable);
            }
            else
            {
                ammoText.gameObject.SetActive(enable);
            }
        }
    }

    private void Update()
    {
        fireRateTimer -= Time.deltaTime;

        HandleADS();
        HandleRecoilRecovery();

        if (!canShoot) return;

        bool wantsToShoot = (shootActionInstance != null && shootActionInstance.IsPressed()) || Input.GetMouseButton(0);

        if (wantsToShoot && fireRateTimer <= 0f)
        {
            TryShootDirect();
        }
    }

    private void HandleADS()
    {
        if (!enableADS) return;

        isAiming = Input.GetMouseButton(1);

        if (playerCamera != null)
        {
            float targetFov = isAiming ? adsFov : defaultFov;
            playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, targetFov, Time.deltaTime * adsSpeed);
        }

        if (weaponTransform != null)
        {
            Vector3 targetPos = isAiming ? adsPos : hipPos;
            weaponTransform.localPosition = Vector3.Lerp(weaponTransform.localPosition, targetPos, Time.deltaTime * adsSpeed);
        }

        if (crosshairUI != null)
        {
            crosshairUI.SetActive(!isAiming);
        }
    }

    private void HandleRecoilRecovery()
    {
        if (Mathf.Abs(currentRecoilX) > 0.001f || Mathf.Abs(currentRecoilY) > 0.001f)
        {
            currentRecoilX = Mathf.Lerp(currentRecoilX, 0f, Time.deltaTime * recoilRecoverySpeed);
            currentRecoilY = Mathf.Lerp(currentRecoilY, 0f, Time.deltaTime * recoilRecoverySpeed);
        }
    }

    private void TryShootDirect()
    {
        int availableAmmo = playerInventory != null ? playerInventory.GetTotalAmmoCount() : 0;

        if (availableAmmo > 0)
        {
            if (playerInventory != null)
            {
                playerInventory.ConsumeAmmo(1);
            }

            ShootBullet();
            fireRateTimer = fireRate;
        }
        else
        {
            // Gatillazo seco (Dry Fire) con audio directo original
            PlayAudio(emptySound, 0.8f);
            if (SoundList.Instance != null) SoundList.Instance.PlaySound("SFX_Empty");

            fireRateTimer = fireRate * 1.5f;
            UpdateAmmoUI();
        }
    }

    private void ShootBullet()
    {
        UpdateAmmoUI();

        // Audio directo original
        PlayAudio(shootSound, 1.0f);
        if (SoundList.Instance != null)
        {
            SoundList.Instance.PlaySoundRandomPitch("SFX_Shoot", 0.94f, 1.06f);
        }

        ApplyRecoilAndShake();

        if (muzzleFlashLight != null)
        {
            muzzleFlashLight.enabled = true;
            DOVirtual.DelayedCall(0.04f, () => { if (muzzleFlashLight != null) muzzleFlashLight.enabled = false; });
        }

        if (bulletPool == null)
        {
            bulletPool = Object.FindFirstObjectByType<LG_ObjectPool>();
        }

        GameObject bulletObj = bulletPool != null ? bulletPool.Get() : null;
        if (bulletObj != null)
        {
            Transform spawnSource = firePoint != null ? firePoint : transform;
            bulletObj.transform.position = spawnSource.position;
            bulletObj.transform.rotation = spawnSource.rotation;

            LG_Bullet bullet = bulletObj.GetComponent<LG_Bullet>();
            if (bullet != null) bullet.Initialize(bulletPool);
        }
    }

    private void ApplyRecoilAndShake()
    {
        if (playerCamera != null)
        {
            playerCamera.transform.DOComplete();
            playerCamera.transform.DOShakePosition(0.08f, screenShakeStrength, 14, 90, false, true);

            float kickX = -recoilVerticalKick;
            float kickY = Random.Range(-recoilHorizontalKick, recoilHorizontalKick);
            playerCamera.transform.DOLocalRotate(new Vector3(kickX, kickY, 0f), 0.04f)
                .OnComplete(() => playerCamera.transform.DOLocalRotate(Vector3.zero, 0.12f));
        }
    }

    public void PlayAudio(AudioClip clip, float volume = 1.0f)
    {
        if (clip == null) return;
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.spatialBlend = 0f;
        }
        audioSource.PlayOneShot(clip, volume);
    }

    public void AddAmmo(int amount)
    {
        if (playerInventory != null)
        {
            playerInventory.AddItem(ammoItemName, amount);
        }
    }

    private void UpdateAmmoUI()
    {
        if (ammoText == null) return;

        int totalAmmo = playerInventory != null ? playerInventory.GetTotalAmmoCount() : 0;

        string ammoColor = totalAmmo > 10 ? "#38BDF8" : (totalAmmo > 0 ? "#FBBF24" : "#EF4444");
        string statusText = totalAmmo > 0 ? "MUNICIÓN LISTA (DIRECTA)" : "SIN MUNICIÓN";

        ammoText.text = $"<b><size=36><color={ammoColor}>{totalAmmo}</color></size></b> <size=16><color=#94A3B8>BALAS</color></size>\n" +
                        $"<size=11><color={ammoColor}><b>{statusText}</b></color></size>";
    }
}
