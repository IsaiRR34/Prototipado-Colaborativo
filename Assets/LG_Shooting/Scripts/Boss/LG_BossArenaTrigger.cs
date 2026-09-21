using UnityEngine;

public class LG_BossArenaTrigger : MonoBehaviour
{
    [Header("Referencia al Jefe")]
    [Tooltip("Arrastra aquí al Jefe 'El Bebé Maldito'")]
    [SerializeField] private BossBabyController bossController;

    private void OnTriggerEnter(Collider other)
    {
        if (bossController != null && !bossController.isActivated)
        {
            // Verificamos si el que entró a la zona es el jugador
            if (other.GetComponentInParent<LG_PlayerHealth>() != null || other.CompareTag("Player"))
            {
                bossController.ActivateBoss();

                // Destruimos el trigger porque ya cumplió su función y no necesitamos activarlo dos veces
                Destroy(gameObject);
            }
        }
    }
}