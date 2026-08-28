using UnityEngine;
using UnityEngine.AI; // Necesario para NavMesh

[RequireComponent(typeof(NavMeshAgent))] // Asegura que el enemigo tenga el componente
public class LG_Enemy : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private float maxHealth = 3f;
    private float currentHealth;

    [Header("Movement & NavMesh Settings")]
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

    private NavMeshAgent navAgent; // Referencia al NavMeshAgent
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
        navAgent = GetComponent<NavMeshAgent>();

        // Configurar NavMeshAgent con variables de inspector
        navAgent.stoppingDistance = stopDistance;

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
            originalColors[i] = childRenderers[i].material.color;
        }

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
        // Smooth pursuit behavior usando NavMesh
        if (target != null)
        {
            float distance = Vector3.Distance(transform.position, target.position);

            if (distance <= chaseRange)
            {
                navAgent.isStopped = false;
                navAgent.SetDestination(target.position); // NavMeshAgent maneja el camino

                if (distance <= stopDistance && Time.time >= nextAttackTime)
                {
                    AttackPlayer();
                }
            }
            else
            {
                // Si el jugador se aleja, detener la persecución
                navAgent.isStopped = true;
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
            // Usamos la velocidad del NavMeshAgent para determinar si camina
            bool isChasing = navAgent.velocity.sqrMagnitude > 0.1f;

            if (isChasing)
            {
                float timeFactor = Time.time * 6f;
                float armBob = Mathf.Sin(timeFactor) * 10f;
                float armSway = Mathf.Cos(timeFactor) * 5f;

                leftArm.localRotation = originalLeftArmRot * Quaternion.Euler(leftArmWalkOffset + new Vector3(armBob, armSway, 0f));
                rightArm.localRotation = originalRightArmRot * Quaternion.Euler(rightArmWalkOffset + new Vector3(-armBob, -armSway, 0f));

                if (visualChild != null)
                {
                    float bodySway = Mathf.Sin(timeFactor) * 4f;
                    visualChild.localRotation = Quaternion.Euler(0f, 0f, bodySway);
                }
            }
            else
            {
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
        GameObject dropGO = GameObject.CreatePrimitive(PrimitiveType.Cube);
        dropGO.name = $"{dropItemName}_Drop";
        dropGO.transform.position = transform.position + Vector3.up * 0.5f;
        dropGO.transform.rotation = Quaternion.Euler(45f, 45f, 45f);
        dropGO.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);

        LG_Collectible collectible = dropGO.AddComponent<LG_Collectible>();
        collectible.Initialize(dropItemName, dropAmount);

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
            Renderer r = dropGO.GetComponent<Renderer>();
            if (r != null)
            {
                r.material.color = new Color(0.2f, 0.8f, 0.2f);
            }
        }

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
            float thrust = Mathf.Sin(t * Mathf.PI) * 0.4f;

            leftArm.localPosition = originalLeftPos + Vector3.forward * thrust;
            rightArm.localPosition = originalRightPos + Vector3.forward * thrust;

            yield return null;
        }

        leftArm.localPosition = originalLeftPos;
        rightArm.localPosition = originalRightPos;
    }

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