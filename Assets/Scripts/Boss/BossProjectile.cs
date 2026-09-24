using UnityEngine;

public class BossProjectile : MonoBehaviour
{
    [SerializeField] private float speed = 14f;
    [SerializeField] private float damage = 15f;
    [SerializeField] private float lifetime = 5f;

    private Vector3 moveDirection;

    public void Initialize(Vector3 targetDirection)
    {
        moveDirection = targetDirection.normalized;
        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        transform.position += moveDirection * speed * Time.deltaTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        LG_PlayerHealth playerHealth = other.GetComponentInParent<LG_PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(damage);
            Destroy(gameObject);
            return;
        }

        if (!other.isTrigger && other.GetComponentInParent<BossBabyController>() == null)
        {
            Destroy(gameObject);
        }
    }
}
