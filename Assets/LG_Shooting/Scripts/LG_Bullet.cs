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
        Rigidbody rb = other.attachedRigidbody;
        if (rb != null && !rb.isKinematic)
        {
            rb.AddForce(transform.forward * 8f, ForceMode.Impulse);
        }
        // Return to pool when hitting another collider
        ReturnToPool();
    }

    private void OnCollisionEnter(Collision collision)
    {
        Rigidbody rb = collision.rigidbody;
        if (rb != null && !rb.isKinematic)
        {
            rb.AddForce(transform.forward * 8f, ForceMode.Impulse);
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
