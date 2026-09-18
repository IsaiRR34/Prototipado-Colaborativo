using UnityEngine;
using System.Collections;

public class Hand : MonoBehaviour
{
    [Header("Referencias de Armas")]
    [SerializeField] private LG_Shoot shootScript;
    public GameObject sword; // Bate de beisbol
    public GameObject flashLight;
    public GameObject gun;

    [Header("Configuracion de Ataque Melee")]
    [SerializeField] private float attackDamage = 2f;
    [SerializeField] private float attackRange = 2.5f;
    [SerializeField] private float attackRate = 0.45f;
    [SerializeField] private float hitForce = 12f;

    //[Header("Efectos de Audio (SFX)")]
    //[SerializeField] private AudioSource audioSource;
    //[SerializeField] private AudioClip swingSound;
    //[SerializeField] private AudioClip hitSound;

    private bool canHit;
    private float nextAttackTime = 0f;
    private bool isSwinging = false;
    private Quaternion swordOriginalRot;
    private Vector3 swordOriginalPos;
    private Camera playerCamera;

    void Start()
    {
//        if (audioSource == null)
//        {
//            audioSource = GetComponent<AudioSource>();
//            if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
//            audioSource.playOnAwake = false;
//        }

//#if UNITY_EDITOR
//        if (swingSound == null) swingSound = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/LG_Shooting/LGAssets/Audio/SFX_Melee_Swing.wav");
//        if (hitSound == null) hitSound = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/LG_Shooting/LGAssets/Audio/SFX_Melee_Hit.wav");
//#endif

        if (shootScript == null)
        {
            shootScript = GetComponentInChildren<LG_Shoot>();
            if (shootScript == null) shootScript = GetComponentInParent<LG_Shoot>();
        }

        playerCamera = Camera.main;
        if (playerCamera == null) playerCamera = GetComponentInParent<Camera>();

        if (sword != null)
        {
            swordOriginalRot = sword.transform.localRotation;
            swordOriginalPos = sword.transform.localPosition;
        }

        DefaultGun(); // Iniciamos con la pistola por defecto
    }

    void Update()
    {
        // Cambio de armas
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            DefaultGun();
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            TurnOnLight();
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            GetSword();
        }

        // Ataque Melee con el Bate
        if (canHit && !isSwinging && Time.time >= nextAttackTime)
        {
            if (Input.GetMouseButtonDown(0))
            {
                nextAttackTime = Time.time + attackRate;
                StartCoroutine(PerformMeleeSwing());
            }
        }
    }

    void DefaultGun()
    {
        CancelSwing();
        canHit = false;

        if (sword != null) sword.SetActive(false);
        if (flashLight != null) flashLight.SetActive(false);
        if (gun != null) gun.SetActive(true);

        if (shootScript != null) shootScript.EnableShooting(true);
    }

    void TurnOnLight()
    {
        CancelSwing();
        canHit = false;

        if (flashLight != null) flashLight.SetActive(true);
        if (gun != null) gun.SetActive(false);
        if (sword != null) sword.SetActive(false);

        if (shootScript != null) shootScript.EnableShooting(false);
    }

    void GetSword()
    {
        canHit = true;

        if (sword != null) sword.SetActive(true);
        if (gun != null) gun.SetActive(false);
        if (flashLight != null) flashLight.SetActive(false);

        if (shootScript != null) shootScript.EnableShooting(false);
    }

    private void CancelSwing()
    {
        if (isSwinging)
        {
            StopAllCoroutines();
            if (sword != null)
            {
                sword.transform.localRotation = swordOriginalRot;
                sword.transform.localPosition = swordOriginalPos;
            }
            isSwinging = false;
        }
    }

    private IEnumerator PerformMeleeSwing()
    {
        isSwinging = true;

        //if (audioSource != null && swingSound != null)
        //{
        //    audioSource.PlayOneShot(swingSound, 0.85f);
        //}

        if (SoundList.Instance != null)
        {
            SoundList.Instance.PlaySound("SFX_Melee_Swing");
        }

        if (playerCamera == null) playerCamera = Camera.main;

        float duration = 0.25f;
        float elapsed = 0f;
        bool hitRegistered = false;

        // Animacion procedimental: golpe diagonal rapido hacia el frente y retorno
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            // Arco senoidal para la rotacion y desplazamiento
            float swingAngle = Mathf.Sin(t * Mathf.PI) * 70f;
            float forwardThrust = Mathf.Sin(t * Mathf.PI) * 0.25f;

            if (sword != null)
            {
                sword.transform.localRotation = swordOriginalRot * Quaternion.Euler(swingAngle, -swingAngle * 0.5f, 0f);
                sword.transform.localPosition = swordOriginalPos + Vector3.forward * forwardThrust;
            }

            // Registrar impacto a mitad del golpe
            if (t >= 0.4f && !hitRegistered)
            {
                hitRegistered = true;
                CheckMeleeHit();
            }

            yield return null;
        }

        // Restablecer posicion original del bate
        if (sword != null)
        {
            sword.transform.localRotation = swordOriginalRot;
            sword.transform.localPosition = swordOriginalPos;
        }

        isSwinging = false;
    }

    private void CheckMeleeHit()
    {
        Vector3 origin = (playerCamera != null) ? playerCamera.transform.position : transform.position;
        Vector3 direction = (playerCamera != null) ? playerCamera.transform.forward : transform.forward;

        RaycastHit hit;
        // SphereCast para tener un area de impacto generosa y satisfactoria
        if (Physics.SphereCast(origin, 0.4f, direction, out hit, attackRange))
        {
            //if (audioSource != null && hitSound != null)
            //{
            //    audioSource.PlayOneShot(hitSound, 0.95f);
            //}

            if (SoundList.Instance != null)
            {
                SoundList.Instance.PlaySound("SFX_Melee_Hit");
            }

            // Daño al enemigo
            LG_Enemy enemy = hit.collider.GetComponent<LG_Enemy>();
            if (enemy == null && hit.rigidbody != null)
            {
                enemy = hit.rigidbody.GetComponent<LG_Enemy>();
            }
            if (enemy == null)
            {
                enemy = hit.collider.GetComponentInParent<LG_Enemy>();
            }

            if (enemy != null)
            {
                enemy.TakeDamage(attackDamage);
                Debug.Log($"[Hand] ¡Golpe de bate a {enemy.name}! Daño: {attackDamage}");
            }

            // Empuje de fisicas a cubos y objetivos
            Rigidbody rb = hit.rigidbody != null ? hit.rigidbody : hit.collider.attachedRigidbody;
            if (rb != null && !rb.isKinematic)
            {
                rb.AddForce(direction * hitForce, ForceMode.Impulse);
            }
        }
    }
}
