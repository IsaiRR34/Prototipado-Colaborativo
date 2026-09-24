using UnityEngine;

public class LG_Bullet : MonoBehaviour
{
    [Header("Movement Settings")]
    [Tooltip("The speed at which the bullet moves forward.")]
    [SerializeField] private float speed = 20f;

    [Header("Lifetime Settings")]
    [Tooltip("The maximum lifetime of the bullet in seconds before returning to the pool.")]
    [SerializeField] private float lifeTime = 3f;

    private LG_ObjectPool ownerPool;
    private float lifeTimer;

    /// <summary>
    /// Initializes the bullet with a reference to the Object Pool that spawned it.
    /// Resetting timers here allows the bullet to be reused correctly.
    /// </summary>
    /// <param name="pool">The LG_ObjectPool managing this bullet.</param>
    public void Initialize(LG_ObjectPool pool)
    {
        ownerPool = pool;
        lifeTimer = lifeTime;
    }

    private void Update()
    {
        // Move the bullet forward relative to its orientation
        transform.Translate(Vector3.forward * speed * Time.deltaTime);

        // Track lifetime and return to pool when expired
        lifeTimer -= Time.deltaTime;
        if (lifeTimer <= 0f)
        {
            ReturnToPool();
        }
    }

    // --- 3D Collision Handlers ---
    private void OnTriggerEnter(Collider other)
    {
        // Ignorar colisiones con el propio jugador que dispara
        if (other.GetComponentInParent<LG_PlayerHealth>() != null) return;

        // Apply damage if we hit the boss
        BossBabyController bossTrigger = other.GetComponentInParent<BossBabyController>();
        if (bossTrigger != null)
        {
            bool isHeadshot = other.name.ToLower().Contains("head");
            bossTrigger.TakeDamage(1f, isHeadshot);
            ReturnToPool();
            return;
        }

        // Apply damage if we hit an enemy
        LG_Enemy enemy = other.GetComponent<LG_Enemy>();
        Rigidbody rb = other.attachedRigidbody;
        if (enemy == null && rb != null)
        {
            enemy = rb.GetComponent<LG_Enemy>();
        }
        if (enemy != null)
        {
            if (rb != null && !rb.isKinematic)
            {
                rb.AddForce(transform.forward * 8f, ForceMode.Impulse);
            }
            enemy.TakeDamage(1f);
            ReturnToPool();
            return;
        }

        // Ignorar otros triggers que sean zonas de recogida o checkpoints
        if (other.isTrigger) return;

        if (rb != null && !rb.isKinematic)
        {
            rb.AddForce(transform.forward * 8f, ForceMode.Impulse);
        }

        // Return to pool when hitting solid collider
        ReturnToPool();
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Ignorar colisiones con el propio jugador
        if (collision.gameObject.GetComponentInParent<LG_PlayerHealth>() != null) return;
        Rigidbody rb = collision.rigidbody;
        if (rb != null && !rb.isKinematic)
        {
            rb.AddForce(transform.forward * 8f, ForceMode.Impulse);
        }

        // Apply damage if we hit the boss
        BossBabyController bossCol = collision.gameObject.GetComponentInParent<BossBabyController>();
        if (bossCol != null)
        {
            bool isHeadshot = collision.gameObject.name.ToLower().Contains("head");
            bossCol.TakeDamage(1f, isHeadshot);
            ReturnToPool();
            return;
        }

        // Apply damage if we hit an enemy
        LG_Enemy enemy = collision.gameObject.GetComponent<LG_Enemy>();
        if (enemy == null && rb != null)
        {
            enemy = rb.GetComponent<LG_Enemy>();
        }
        if (enemy != null)
        {
            enemy.TakeDamage(1f);
        }

        // Return to pool when hitting another collider physically
        ReturnToPool();
    }

    // --- 2D Collision Handlers (Optional Fallback) ---
    private void OnTriggerEnter2D(Collider2D other)
    {
        ReturnToPool();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        ReturnToPool();
    }

    /// <summary>
    /// Safely deactivates this object and returns it to its owning pool.
    /// </summary>
    private void ReturnToPool()
    {
        if (ownerPool != null)
        {
            ownerPool.ReturnToPool(gameObject);
        }
        else
        {
            // If spawned without a pool reference, just disable it
            gameObject.SetActive(false);
        }
    }
}
