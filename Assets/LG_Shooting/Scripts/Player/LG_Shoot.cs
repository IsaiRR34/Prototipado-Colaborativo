using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using System.Collections;

public class LG_Shoot : MonoBehaviour
{
    [Header("Referencias de Disparo")]
    [SerializeField] private LG_ObjectPool bulletPool;
    [SerializeField] private Transform firePoint;

    [Header("Configuración de Arma")]
    [SerializeField] private float fireRate = 0.2f;
    private float fireRateTimer = 0f;
    private bool canShoot = true;
    private bool isReloading = false;

    [Header("Sistema de Munición")]
    [SerializeField] private LG_Inventory playerInventory;
    [SerializeField] private string ammoItemName = "Munición";

    public int maxClipSize = 12;
    public int currentClip;

    [Header("UI HUD")]
    [SerializeField] private TextMeshProUGUI ammoText;

    //[Header("Efectos de Audio (SFX)")]
    //[SerializeField] private AudioSource audioSource;
    //[SerializeField] private AudioClip shootSound;
    //[SerializeField] private AudioClip reloadSound;
    //[SerializeField] private AudioClip emptySound;

    [Header("Inputs")]
    [SerializeField] private InputActionReference shootAction;
    private InputAction shootActionInstance;

    private void Awake()
    {
        currentClip = maxClipSize;

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
        //if (audioSource == null)
        //{
        //    audioSource = GetComponent<AudioSource>();
        //    if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
        //    audioSource.playOnAwake = false;
        //    audioSource.spatialBlend = 0f; // Audio 2D estéreo para el jugador
        //}

//#if UNITY_EDITOR
//        if (shootSound == null) shootSound = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/LG_Shooting/LGAssets/Audio/SFX_Shoot.wav");
//        if (reloadSound == null) reloadSound = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/LG_Shooting/LGAssets/Audio/SFX_Reload.wav");
//        if (emptySound == null) emptySound = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/LG_Shooting/LGAssets/Audio/SFX_Empty.wav");
//#endif

        if (playerInventory == null)
        {
            playerInventory = GetComponentInParent<LG_Inventory>();
            if (playerInventory == null) playerInventory = Object.FindFirstObjectByType<LG_Inventory>();
        }

        if (playerInventory != null)
        {
            playerInventory.InitializeFromInspector();
        }

        if (ammoText == null)
        {
            // Priorizar el cuadro de municion en la esquina inferior derecha
            GameObject container = GameObject.Find("MunicionContainer");
            if (container != null)
            {
                ammoText = container.GetComponentInChildren<TextMeshProUGUI>();
            }

            if (ammoText == null)
            {
                GameObject panel = GameObject.Find("MunicionPannel");
                if (panel != null)
                {
                    ammoText = panel.GetComponentInChildren<TextMeshProUGUI>();
                }
            }

            if (ammoText == null)
            {
                GameObject ammoGo = GameObject.Find("MunicionText");
                if (ammoGo == null) ammoGo = GameObject.Find("AmmoText");
                if (ammoGo != null)
                {
                    ammoText = ammoGo.GetComponent<TextMeshProUGUI>();
                }
            }
        }

        // Limpiar cualquier texto de municion suelto en la esquina superior del Canvas
        if (ammoText != null)
        {
            var canvas = Object.FindFirstObjectByType<Canvas>();
            if (canvas != null)
            {
                for (int i = canvas.transform.childCount - 1; i >= 0; i--)
                {
                    Transform child = canvas.transform.GetChild(i);
                    if ((child.name == "MunicionText" || child.name == "AmmoText") && child != ammoText.transform && child != ammoText.transform.parent)
                    {
                        Destroy(child.gameObject);
                    }
                }
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
                {
                    parent.parent.gameObject.SetActive(enable);
                }
                else
                {
                    parent.gameObject.SetActive(enable);
                }
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

        if (!canShoot || isReloading) return;

        int reserveAmmo = GetTotalReserveAmmo();

        // Recarga con tecla R (soporta tanto Input Manager clásico como New Input System)
        bool reloadPressed = (Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame);

        if (reloadPressed)
        {
            if (currentClip < maxClipSize && reserveAmmo > 0)
            {
                StartCoroutine(ReloadRoutine());
                return;
            }
            else if (currentClip >= maxClipSize)
            {
                LG_TooltipManager.Instance?.ShowTooltipTemporary("Cargador lleno", 1.0f);
            }
            else if (reserveAmmo == 0)
            {
                if (SoundList.Instance != null) SoundList.Instance.PlaySound("SFX_Empty");
                LG_TooltipManager.Instance?.ShowTooltipTemporary("<color=#EF4444>Sin munición de reserva</color>", 1.2f);
            }
        }

        bool wantsToShoot = (shootActionInstance != null && shootActionInstance.IsPressed());

        if (wantsToShoot && fireRateTimer <= 0f)
        {
            if (currentClip > 0)
            {
                ShootBullet();
                fireRateTimer = fireRate;
            }
            else
            {
                // Si no hay balas en cargador pero sí en reserva, recargar automáticamente
                if (reserveAmmo > 0 && !isReloading)
                {
                    StartCoroutine(ReloadRoutine());
                    return;
                }

                // Sonido de gatillo sin balas (Dry Fire)
                if (SoundList.Instance != null) SoundList.Instance.PlaySound("SFX_Empty");
                fireRateTimer = fireRate * 1.5f;
            }
        }
    }

    /// <summary>
    /// Intenta recargar el arma si hay munición de reserva y el cargador no está lleno.
    /// </summary>
    public void TryReload()
    {
        if (!canShoot || isReloading) return;
        if (currentClip < maxClipSize && GetTotalReserveAmmo() > 0)
        {
            StartCoroutine(ReloadRoutine());
        }
    }

    private void ShootBullet()
    {
        currentClip--;
        UpdateAmmoUI();
        if (SoundList.Instance != null) SoundList.Instance.PlaySoundRandomPitch("SFX_ARShot", 0.95f, 1.05f);

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

    private IEnumerator ReloadRoutine()
    {
        isReloading = true;
        UpdateAmmoUI();
        if (SoundList.Instance != null) SoundList.Instance.PlaySound("SFX_ARReload");

        yield return new WaitForSeconds(1.5f);

        int ammoNeeded = maxClipSize - currentClip;
        int reserveAmmo = GetTotalReserveAmmo();

        int ammoToReload = Mathf.Min(ammoNeeded, reserveAmmo);

        if (ammoToReload > 0 && playerInventory != null)
        {
            playerInventory.RemoveItem(ammoItemName, ammoToReload);
            currentClip += ammoToReload;
        }

        isReloading = false;
        UpdateAmmoUI();
    }

    public void AddMunicion(int amount)
    {
        if (playerInventory != null)
        {
            playerInventory.AddItem(ammoItemName, amount);
        }
    }

    public void AddAmmo(int amount)
    {
        AddMunicion(amount);
    }

    private int GetTotalReserveAmmo()
    {
        if (playerInventory == null) return 0;
        return playerInventory.GetItemCount(ammoItemName);
    }

    private void UpdateAmmoUI()
    {
        if (ammoText == null) return;

        if (isReloading)
        {
            ammoText.text = "<b><size=22><color=#FBBF24>RECARGANDO...</color></size></b>\n<size=11><color=#94A3B8>PISTOLA 9MM</color></size>";
            return;
        }

        int reserveAmmo = GetTotalReserveAmmo();

        string clipColor = "#FFFFFF";
        string statusLine = "<size=11><color=#38BDF8><b>PISTOLA 9MM</b></color></size>";

        if (currentClip == 0)
        {
            clipColor = "#EF4444";
            statusLine = reserveAmmo > 0 
                ? "<size=12><color=#FBBF24><b>[ R ] RECARGAR</b></color></size>" 
                : "<size=11><color=#EF4444><b>SIN MUNICIÓN</b></color></size>";
        }
        else if (currentClip <= 3)
        {
            clipColor = "#F87171";
            statusLine = "<size=11><color=#F87171>MUNICIÓN BAJA</color></size>";
        }

        ammoText.text = $"<b><size=38><color={clipColor}>{currentClip}</color></size></b>" +
                        $"<size=20><color=#64748B> / </color><color=#CBD5E1>{reserveAmmo}</color></size>\n" +
                        $"{statusLine}";
    }

    //private void PlayAudio(AudioClip clip, float volume = 1.0f)
    //{
    //    if (clip == null) return;
    //    if (audioSource == null)
    //    {
    //        audioSource = GetComponent<AudioSource>();
    //        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
    //    }
    //    audioSource.PlayOneShot(clip, volume);
    //}
}
