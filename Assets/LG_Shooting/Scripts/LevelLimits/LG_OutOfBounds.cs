using UnityEngine;

public class LG_OutOfBounds : MonoBehaviour
{
    [Header("Penalización por Caída")]
    [Tooltip("Cantidad de vida que pierde el jugador al salir del mapa.")]
    [SerializeField] private float fallDamage = 25f;

    private void OnTriggerEnter(Collider other)
    {
        // Detectamos si el jugador es el que cayó
        LG_PlayerHealth pHealth = other.GetComponentInParent<LG_PlayerHealth>();

        if (pHealth != null)
        {
            // 1. Aplicar penalización de vida
            pHealth.TakeDamage(fallDamage);

            // 2. Si sobrevivió a la caída, lo teletransportamos. 
            // (Si murió por el daño, el propio LG_PlayerHealth ya maneja su respawn).
            if (pHealth.GetCurrentHealth() > 0f)
            {
                // Misma lógica de coordenadas que en LG_PlayerHealth
                Vector3 respawnPos = SafeRoomCheckpoint.HasCheckpoint ? SafeRoomCheckpoint.LastSafePosition : new Vector3(0f, 1f, 0f);

                CharacterController cc = pHealth.GetComponent<CharacterController>();
                if (cc != null)
                {
                    cc.enabled = false;
                    pHealth.transform.position = respawnPos;
                    cc.enabled = true;
                }
                else
                {
                    pHealth.transform.position = respawnPos;
                }

                // Feedback visual
                LG_TooltipManager.Instance?.ShowTooltipTemporary("<color=#EF4444><b>Caíste en el vacío. (-25 Salud)</b></color>", 3f);
            }
        }
    }
}