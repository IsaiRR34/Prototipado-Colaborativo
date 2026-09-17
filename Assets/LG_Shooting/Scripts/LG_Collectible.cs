using UnityEngine;

public class LG_Collectible : MonoBehaviour
{
    [Header("Collectible Settings")]
    [Tooltip("Nombre del ítem a añadir al inventario.")]
    [SerializeField] private string itemName = "Munición";

    [Tooltip("Cantidad del ítem a añadir.")]
    [SerializeField] private int amount = 5;

    [Header("Movement Animation")]
    [Tooltip("Velocidad de rotación.")]
    [SerializeField] private float rotationSpeed = 50f;

    [Tooltip("Frecuencia de oscilación vertical.")]
    [SerializeField] private float bobFrequency = 2f;

    [Tooltip("Amplitud de oscilación vertical.")]
    [SerializeField] private float bobAmplitude = 0.15f;

    [Header("Efectos de Audio (SFX)")]
    [SerializeField] private AudioClip pickupSound;

    private Vector3 startPos;

    private void Start()
    {
        AutoAssignSoundIfMissing();

        startPos = transform.position;

        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.isTrigger = true;
        }
    }

    private void Update()
    {
        transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime, Space.World);

        Vector3 tempPos = startPos;
        tempPos.y += Mathf.Sin(Time.time * bobFrequency) * bobAmplitude;
        transform.position = tempPos;
    }

    private void OnTriggerEnter(Collider other)
    {
        LG_Inventory inventory = other.GetComponentInParent<LG_Inventory>();

        if (inventory != null)
        {
            if (pickupSound != null)
            {
                AudioSource.PlayClipAtPoint(pickupSound, transform.position, 1.0f);
            }

            inventory.AddItem(itemName, amount);
            Debug.Log($"[LG_Collectible] ¡Jugador recogió {amount}x {itemName}!");
            Destroy(gameObject);
        }
    }

    public void Initialize(string name, int qty)
    {
        itemName = name;
        amount = qty;
        AutoAssignSoundIfMissing();
    }

    public void SetPickupSound(AudioClip clip)
    {
        pickupSound = clip;
    }

    private void AutoAssignSoundIfMissing()
    {
#if UNITY_EDITOR
        if (pickupSound == null)
        {
            string clean = (itemName != null) ? itemName.ToLower() : "";
            string dir = "Assets/LG_Shooting/LGAssets/Audio";
            if (clean.Contains("muni"))
            {
                pickupSound = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>($"{dir}/SFX_Pickup_Ammo.wav");
            }
            else if (clean.Contains("bater") || clean.Contains("battery"))
            {
                pickupSound = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>($"{dir}/SFX_Pickup_Battery.wav");
            }
            else if (clean.Contains("llave") || clean.Contains("key"))
            {
                pickupSound = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>($"{dir}/SFX_Pickup_Key.wav");
            }

            if (pickupSound == null)
            {
                pickupSound = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>($"{dir}/SFX_Pickup.wav");
            }
        }
#endif
    }
}
