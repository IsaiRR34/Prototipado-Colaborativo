using UnityEngine;
using UnityEngine.InputSystem; // Required for the new Input System

public class LG_Shoot : MonoBehaviour
{

    [SerializeField] private LG_ObjectPool bulletPool;

    [SerializeField] private Transform firePoint;

    [SerializeField] private float fireRate = 1;
    private float fireRateTimer = 0f;
    private bool canShoot = true;

    [SerializeField] private InputActionReference shootAction;

    private InputAction shootActionInstance;

    private void Awake()
    {

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

    private void OnEnable()
    {
        shootActionInstance?.Enable();
    }

    private void OnDisable()
    {
        shootActionInstance?.Disable();
    }

    public void EnableShooting(bool enable)
    {
        canShoot = enable;
    }

    private void Update()
    {
        fireRateTimer -= Time.deltaTime;

        if (!canShoot || shootActionInstance == null) return;

        if (shootActionInstance.IsPressed() && fireRateTimer <= 0f)
        {
            ShootBullet();
            string soundName = "GunShot"; // Replace with the actual sound name
            fireRateTimer = fireRate;
        }
    }

    private void ShootBullet()
    {

        GameObject bulletObj = bulletPool.Get();

        if (bulletObj != null)
        {

            Transform spawnSource = firePoint != null ? firePoint : transform;
            bulletObj.transform.position = spawnSource.position;
            bulletObj.transform.rotation = spawnSource.rotation;

            LG_Bullet bullet = bulletObj.GetComponent<LG_Bullet>();
            if (bullet != null)
            {
                bullet.Initialize(bulletPool);
            }

        }
    }
}