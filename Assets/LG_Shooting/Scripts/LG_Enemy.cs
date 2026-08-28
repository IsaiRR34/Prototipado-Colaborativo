using UnityEngine;

public class LG_Enemy : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private float maxHealth = 3f;
    private float currentHealth;

    [Header("Movement Settings")]
    [SerializeField] private float speed = 2.5f;
    [SerializeField] private float chaseRange = 15f;
    [SerializeField] private float stopDistance = 1.5f;
    [SerializeField] private Transform target; // The Player to follow

    [Header("Drop Settings")]
    [SerializeField] private bool dropItemOnDeath = true;
    [SerializeField] private string dropItemName = "Bateria";
    [SerializeField] private int dropAmount = 1;
    [SerializeField] private Material dropMaterial;

    [Header("Visual Feedback (Multi-Renderer)")]
    [SerializeField] private Color flashColor = new Color(1f, 0.2f, 0.2f);
    [SerializeField] private float flashDuration = 0.15f;

    [Header("Procedural Animation Settings")]
    [SerializeField] private bool useProceduralWalk = true;
    [SerializeField] private Transform leftArm;
    [SerializeField] private Transform rightArm;
    [SerializeField] private Vector3 leftArmWalkOffset = new Vector3(-60f, 0f, 60f);
    [SerializeField] private Vector3 rightArmWalkOffset = new Vector3(60f, 0f, -60f);

    [Header("Attack Settings")]
    [SerializeField] private float attackDamage = 10f;
    [SerializeField] private float attackCooldown = 1.5f;
    private float nextAttackTime;

    private Renderer[] childRenderers;
    private Color[] originalColors;
    private float flashTimer;
    private bool isFlashing;

    private Quaternion originalLeftArmRot;
    private Quaternion originalRightArmRot;
    private Transform visualChild;

    private void Start()
    {
        currentHealth = maxHealth;

        // Automatically locate Player if none is assigned
        if (target == null)
        {
            GameObject player = GameObject.Find("Player (RI + LG)");
            if (player != null)
            {
                target = player.transform;
            }
        }

        // Initialize multi-renderer visual references
        childRenderers = GetComponentsInChildren<Renderer>();
        originalColors = new Color[childRenderers.Length];
        for (int i = 0; i < childRenderers.Length; i++)
        {
            // Use instance material so we don't flash other objects using the same material
            originalColors[i] = childRenderers[i].material.color;
        }

        // Find the visual model child object (typically the first child)
        if (transform.childCount > 0)
        {
            visualChild = transform.GetChild(0);
        }

        // Set up procedural animation arm references
        if (useProceduralWalk)
        {
            if (leftArm == null) leftArm = FindDeepChild(transform, "upperarm_l");
            if (rightArm == null) rightArm = FindDeepChild(transform, "upperarm_r");

            if (leftArm != null) originalLeftArmRot = leftArm.localRotation;
            if (rightArm != null) originalRightArmRot = rightArm.localRotation;
        }
    }

    private void Update()
    {
        // Smooth pursuit behavior
        if (target != null)
        {
            float distance = Vector3.Distance(transform.position, target.position);
            if (distance <= chaseRange && distance > stopDistance)
            {
                // Face towards the player (only rotating horizontally on Y axis)
                Vector3 direction = (target.position - transform.position).normalized;
                direction.y = 0f;
                if (direction.sqrMagnitude > 0.001f)
                {
                    transform.rotation = Quaternion.LookRotation(direction);
                }

                // Move forward towards the player
                transform.position = Vector3.MoveTowards(transform.position, target.position, speed * Time.deltaTime);
            }
            else if (distance <= stopDistance && Time.time >= nextAttackTime)
            {
                AttackPlayer();
            }
        }

        // Color flash effect recovery timer
        if (isFlashing)
        {
            flashTimer -= Time.deltaTime;
            if (flashTimer <= 0f)
            {
                RevertColor();
            }
        }
    }

    private void LateUpdate()
    {
        // Perform procedural zombie movement animations
        if (useProceduralWalk && leftArm != null && rightArm != null)
        {
            bool isChasing = target != null && 
                             Vector3.Distance(transform.position, target.position) <= chaseRange && 
                             Vector3.Distance(transform.position, target.position) > stopDistance;

            if (isChasing)
            {
                // Classic zombie walk: Arms raised forward, bobbing up and down, body swaying
                float timeFactor = Time.time * 6f;
                float armBob = Mathf.Sin(timeFactor) * 10f; // 10 degree bobbing amplitude
                float armSway = Mathf.Cos(timeFactor) * 5f;  // 5 degree horizontal sway

                // Apply walk rotation relative to original pose
                leftArm.localRotation = originalLeftArmRot * Quaternion.Euler(leftArmWalkOffset + new Vector3(armBob, armSway, 0f));
                rightArm.localRotation = originalRightArmRot * Quaternion.Euler(rightArmWalkOffset + new Vector3(-armBob, -armSway, 0f));

                // Sway the entire visual mesh side to side
                if (visualChild != null)
                {
                    float bodySway = Mathf.Sin(timeFactor) * 4f; // 4 degrees body wobble
                    visualChild.localRotation = Quaternion.Euler(0f, 0f, bodySway);
                }
            }
            else
            {
                // Idle posture: Arms down, slow breathing animation cycles
                float breatheFactor = Time.time * 2f;
                float breatheBob = Mathf.Sin(breatheFactor) * 3f;

                leftArm.localRotation = originalLeftArmRot * Quaternion.Euler(breatheBob, 0f, 10f);
                rightArm.localRotation = originalRightArmRot * Quaternion.Euler(-breatheBob, 0f, -10f);

                if (visualChild != null)
                {
                    visualChild.localRotation = Quaternion.identity;
                }
            }
        }
    }

    /// <summary>
    /// Processes damage taken by the enemy.
    /// </summary>
    /// <param name="damage">Amount of damage to apply.</param>
    public void TakeDamage(float damage)
    {
        if (currentHealth <= 0f) return;

        currentHealth -= damage;
        Debug.Log($"[LG_Enemy] {gameObject.name} hit! Health: {currentHealth}/{maxHealth}", this);

        FlashRed();

        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    private void FlashRed()
    {
        for (int i = 0; i < childRenderers.Length; i++)
        {
            if (childRenderers[i] != null)
            {
                childRenderers[i].material.color = flashColor;
            }
        }
        flashTimer = flashDuration;
        isFlashing = true;
    }

    private void RevertColor()
    {
        for (int i = 0; i < childRenderers.Length; i++)
        {
            if (childRenderers[i] != null)
            {
                childRenderers[i].material.color = originalColors[i];
            }
        }
        isFlashing = false;
    }

    private void Die()
    {
        Debug.Log($"[LG_Enemy] {gameObject.name} defeated!", this);

        if (dropItemOnDeath)
        {
            SpawnDrop();
        }

        Destroy(gameObject);
    }

    private void SpawnDrop()
    {
        // Instantiate a physical drop cube dynamically
        GameObject dropGO = GameObject.CreatePrimitive(PrimitiveType.Cube);
        dropGO.name = $"{dropItemName}_Drop";
        dropGO.transform.position = transform.position + Vector3.up * 0.5f;
        dropGO.transform.rotation = Quaternion.Euler(45f, 45f, 45f); // Diamante
        dropGO.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);

        // Add the collectible script
        LG_Collectible collectible = dropGO.AddComponent<LG_Collectible>();
        collectible.Initialize(dropItemName, dropAmount);

        // Assign custom drop material if available
        if (dropMaterial != null)
        {
            Renderer r = dropGO.GetComponent<Renderer>();
            if (r != null)
            {
                r.sharedMaterial = dropMaterial;
            }
        }
        else
        {
            // Fallback default green color
            Renderer r = dropGO.GetComponent<Renderer>();
            if (r != null)
            {
                r.material.color = new Color(0.2f, 0.8f, 0.2f);
            }
        }

        // Set collider as Trigger so it can be picked up
        Collider c = dropGO.GetComponent<Collider>();
        if (c != null)
        {
            c.isTrigger = true;
        }
    }

    private void AttackPlayer()
    {
        nextAttackTime = Time.time + attackCooldown;

        LG_PlayerHealth playerHealth = target.GetComponent<LG_PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(attackDamage);
        }

        if (useProceduralWalk && leftArm != null && rightArm != null)
        {
            StartCoroutine(PerformProceduralAttackThrust());
        }
    }

    private System.Collections.IEnumerator PerformProceduralAttackThrust()
    {
        float duration = 0.3f;
        float elapsed = 0f;

        Vector3 originalLeftPos = leftArm.localPosition;
        Vector3 originalRightPos = rightArm.localPosition;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            // Sine wave for forward-thrust motion
            float thrust = Mathf.Sin(t * Mathf.PI) * 0.4f;

            leftArm.localPosition = originalLeftPos + Vector3.forward * thrust;
            rightArm.localPosition = originalRightPos + Vector3.forward * thrust;

            yield return null;
        }

        leftArm.localPosition = originalLeftPos;
        rightArm.localPosition = originalRightPos;
    }

    // Helper utility to recursively find a named child bone
    private Transform FindDeepChild(Transform parent, string name)
    {
        foreach (Transform child in parent)
        {
            if (child.name == name) return child;
            Transform result = FindDeepChild(child, name);
            if (result != null) return result;
        }
        return null;
    }
}
