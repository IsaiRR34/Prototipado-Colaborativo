using UnityEngine;
using UnityEngine.InputSystem;
using TMPro; // Necesario para la UI
using System.Collections;

public class LG_Shoot : MonoBehaviour
{
    [Header("Referencias de Disparo")]
    [SerializeField] private LG_ObjectPool bulletPool;
    [SerializeField] private Transform firePoint;

    [Header("Configuración de Arma")]
    [SerializeField] private float fireRate = 1f;
    private float fireRateTimer = 0f;
    private bool canShoot = true;
    private bool isReloading = false;

    [Header("Sistema de Munición")]
    // Referencia al inventario para descontar balas al recargar
    [SerializeField] private LG_Inventory playerInventory;
    [SerializeField] private string ammoItemName = "Ammo"; // Nombre del ítem en el inventario

    public int maxClipSize = 12;
    public int currentClip;
    // totalAmmo se elimina porque ahora leemos las balas directamente del inventario (LG_Inventory)

    [Header("UI HUD")]
    [SerializeField] private TextMeshProUGUI ammoText;

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
        // Nos suscribimos a los cambios del inventario para que el HUD se actualice si recogemos balas
        if (playerInventory != null)
        {
            playerInventory.OnInventoryChanged += UpdateAmmoUI;
        }

        UpdateAmmoUI();
    }

    private void OnDestroy()
    {
        // Limpiamos la suscripción para evitar errores de memoria
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

        // Ocultar el HUD de munición si cambiamos al bat o a la linterna
        if (ammoText != null) ammoText.gameObject.SetActive(enable);
    }

    private void Update()
    {
        fireRateTimer -= Time.deltaTime;

        if (!canShoot || isReloading || shootActionInstance == null) return;

        // Obtenemos la cantidad real de balas guardadas en el inventario
        int reserveAmmo = playerInventory != null ? playerInventory.GetItemCount(ammoItemName) : 0;

        // Lógica de Recarga (Tecla R respetando GDD)
        if (Input.GetKeyDown(KeyCode.R) && currentClip < maxClipSize && reserveAmmo > 0)
        {
            StartCoroutine(ReloadRoutine());
            return;
        }

        // Lógica de Disparo
        if (shootActionInstance.IsPressed() && fireRateTimer <= 0f)
        {
            if (currentClip > 0)
            {
                ShootBullet();
                fireRateTimer = fireRate;
            }
            else
            {
                // Pending: empty clip sound or feedback...
            }
        }
    }

    private void ShootBullet()
    {
        currentClip--;
        UpdateAmmoUI();

        GameObject bulletObj = bulletPool.Get();
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
        if (ammoText != null) ammoText.text = "Recargando...";

        // Tiempo de espera para simular la animación de recarga, ajustaremos posteriormente según la animación que se use
        yield return new WaitForSeconds(1.5f);

        int ammoNeeded = maxClipSize - currentClip;
        int reserveAmmo = playerInventory != null ? playerInventory.GetItemCount(ammoItemName) : 0;

        // Calculamos cuántas balas reales podemos recargar
        int ammoToReload = Mathf.Min(ammoNeeded, reserveAmmo);

        if (ammoToReload > 0 && playerInventory != null)
        {
            // Consumimos las balas del inventario
            playerInventory.RemoveItem(ammoItemName, ammoToReload);
            currentClip += ammoToReload;
        }

        isReloading = false;
        UpdateAmmoUI();
    }

    // Función pública para que los consumibles/pickups te den más balas 
    // (Aviso: Si usas el inventario directamente con AddItem, esta función ya no es estrictamente necesaria, pero se deja por compatibilidad)
    public void AddAmmo(int amount)
    {
        if (playerInventory != null)
        {
            playerInventory.AddItem(ammoItemName, amount);
        }
    }

    private void UpdateAmmoUI()
    {
        if (ammoText != null)
        {
            // Consultamos la munición de reserva actual del inventario para la UI
            int reserveAmmo = playerInventory != null ? playerInventory.GetItemCount(ammoItemName) : 0;
            ammoText.text = $"{currentClip} / {reserveAmmo}";
        }
    }
}