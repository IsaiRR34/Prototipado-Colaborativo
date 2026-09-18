using UnityEngine;

public class SafeRoomCheckpoint : MonoBehaviour
{
    public static Vector3 LastSafePosition { get; set; } = new Vector3(0f, 1f, -11f);
    public static bool HasCheckpoint { get; set; } = false;

    [Header("Configuración")]
    [SerializeField] private Transform respawnPoint;
    [SerializeField] private Light safeLight;
    [SerializeField] private Color safeColor = new Color(0.2f, 0.9f, 0.4f, 1f);

    private bool activated = false;

    private void Start()
    {
        if (respawnPoint == null) respawnPoint = transform;

        if (safeLight != null)
        {
            safeLight.color = safeColor;
            safeLight.intensity = 1.8f;
        }

        // Si es el primer checkpoint de la escena, registrarlo por defecto
        if (!HasCheckpoint)
        {
            SetAsActiveCheckpoint();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        LG_PlayerHealth player = other.GetComponentInParent<LG_PlayerHealth>();
        if (player != null && !activated)
        {
            SetAsActiveCheckpoint();
            LG_TooltipManager.Instance?.ShowTooltipTemporary("SALA SEGURA - PUNTO DE CONTROL GUARDADO", 2f);
        }
    }

    public void SetAsActiveCheckpoint()
    {
        activated = true;
        HasCheckpoint = true;
        LastSafePosition = respawnPoint != null ? respawnPoint.position : transform.position;

        if (safeLight != null)
        {
            safeLight.color = safeColor;
        }
    }
}
