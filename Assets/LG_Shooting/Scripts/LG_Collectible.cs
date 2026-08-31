using UnityEngine;

public class LG_Collectible : MonoBehaviour
{
    [Header("Collectible Settings")]
    [Tooltip("Name of the item to add to the inventory.")]
    [SerializeField] private string itemName = "Ammo"; // Cambiado a Ammo por seguridad y compatibilidad

    [Tooltip("Quantity of the item to add.")]
    [SerializeField] private int amount = 5;

    [Header("Movement Animation")]
    [Tooltip("Speed of object rotation.")]
    [SerializeField] private float rotationSpeed = 50f;

    [Tooltip("Frequency of bobbing up and down.")]
    [SerializeField] private float bobFrequency = 2f;

    [Tooltip("Amplitude of bobbing up and down.")]
    [SerializeField] private float bobAmplitude = 0.15f;

    private Vector3 startPos;

    private void Start()
    {
        startPos = transform.position;

        // Ensure collider is set to trigger
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.isTrigger = true;
        }
    }

    private void Update()
    {
        // Rotate item
        transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime, Space.World);

        // Bob up and down
        Vector3 tempPos = startPos;
        tempPos.y += Mathf.Sin(Time.time * bobFrequency) * bobAmplitude;
        transform.position = tempPos;
    }

    private void OnTriggerEnter(Collider other)
    {
        // Optimizamos la búsqueda: GetComponentInParent busca en el objeto mismo y si no lo tiene, sube por la jerarquía.
        // Esto es ideal por si el collider del jugador está en un objeto hijo, que ya nos pasó, por ahora funciona bien así.
        LG_Inventory inventory = other.GetComponentInParent<LG_Inventory>();

        if (inventory != null)
        {
            // Add item to inventory
            inventory.AddItem(itemName, amount);

            // Log pick up for feedback
            Debug.Log($"[LG_Collectible] Player picked up {amount}x {itemName}!");

            // Destroy the collectible object
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Programmatically sets up the collectible item properties.
    /// </summary>
    public void Initialize(string name, int qty)
    {
        itemName = name;
        amount = qty;
    }
}