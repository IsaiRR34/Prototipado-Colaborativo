using UnityEngine;
using UnityEngine.InputSystem; // Required for the new Input System

public class LG_Shoot : MonoBehaviour
{
    [Header("Pool & Spawn Settings")]
    [Tooltip("The Object Pool containing bullet instances.")]
    [SerializeField] private LG_ObjectPool bulletPool;

    [Tooltip("The spawn point from which bullets will be fired.")]
    [SerializeField] private Transform firePoint;

    [Tooltip("Minimum time (in seconds) between consecutive shots.")]
    [SerializeField] private float fireRate = 0.2f;

    [Header("Input Settings (Optional)")]
    [Tooltip("Input action reference to trigger shooting. If left empty, legacy inputs will be used instead.")]
    [SerializeField] private InputActionReference shootAction;

    private InputAction shootActionInstance;
    private float nextFireTime;

    private void Start()
    {
        // Detect PlayerInput automatically from parent or self
        var playerInput = GetComponentInParent<PlayerInput>();
        if (playerInput != null && playerInput.actions != null)
        {
            shootActionInstance = playerInput.actions.FindAction("Shoot");
        }
    }

    private void OnEnable()
    {
        if (shootActionInstance != null)
        {
            shootActionInstance.Enable();
        }
        else if (shootAction != null && shootAction.action != null)
        {
            shootAction.action.Enable();
        }
    }

    private void OnDisable()
    {
        if (shootActionInstance != null)
        {
            shootActionInstance.Disable();
        }
        else if (shootAction != null && shootAction.action != null)
        {
            shootAction.action.Disable();
        }
    }

    private void Update()
    {
        bool isShooting = false;

        // Try reading input from the new Input System Action (auto-detected or referenced)
        if (shootActionInstance != null)
        {
            isShooting = shootActionInstance.IsPressed();
        }
        else if (shootAction != null && shootAction.action != null)
        {
            isShooting = shootAction.action.IsPressed();
        }
        // Fallback to legacy input manager for easy out-of-the-box testing
        else
        {
            isShooting = Input.GetButton("Fire1") || Input.GetKey(KeyCode.Space) || Input.GetMouseButton(0);
        }

        // Fire if shooting input is detected and the cooldown has elapsed
        if (isShooting && Time.time >= nextFireTime)
        {
            nextFireTime = Time.time + fireRate;
            ShootBullet();
        }
    }

    /// <summary>
    /// Spawns a bullet from the object pool, positions it, and initializes its references.
    /// </summary>
    private void ShootBullet()
    {
        if (bulletPool == null)
        {
            Debug.LogWarning($"[LG_Shoot] Bullet Pool is not assigned on '{gameObject.name}'!", this);
            return;
        }

        if (firePoint == null)
        {
            Debug.LogWarning($"[LG_Shoot] Fire Point is not assigned on '{gameObject.name}'! Defaulting to player position.", this);
        }

        // Get an inactive object from the pool
        GameObject bulletObj = bulletPool.Get();

        if (bulletObj != null)
        {
            // Position the bullet at the fire point (or player transform if fire point is null)
            Transform spawnSource = firePoint != null ? firePoint : transform;
            bulletObj.transform.position = spawnSource.position;
            bulletObj.transform.rotation = spawnSource.rotation;

            // Initialize the bullet script with the pool reference
            LG_Bullet bullet = bulletObj.GetComponent<LG_Bullet>();
            if (bullet != null)
            {
                bullet.Initialize(bulletPool);
            }
            else
            {
                Debug.LogWarning($"[LG_Shoot] Spawned object '{bulletObj.name}' does not have an LG_Bullet component attached.", this);
            }
        }
    }
}
