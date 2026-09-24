using UnityEngine;

public class LG_ZoneMusicTrigger : MonoBehaviour
{
    [Header("Música de esta Zona")]
    [Tooltip("El nombre del audio BGM en tu SoundList (Ej. BGM_Zona2)")]
    [SerializeField] private string musicToPlay;
    [SerializeField] private float fadeTime = 2.0f;

    [Header("Limpieza")]
    [Tooltip("Nombres de las canciones de las zonas anteriores para detenerlas")]
    [SerializeField] private string[] musicToStop;

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponentInParent<LG_PlayerHealth>() != null || other.CompareTag("Player"))
        {
            if (SoundList.Instance != null)
            {
                // 1. Detener las canciones de las zonas viejas
                foreach (string oldMusic in musicToStop)
                {
                    if (!string.IsNullOrEmpty(oldMusic))
                    {
                        SoundList.Instance.StopSound(oldMusic);
                    }
                }

                // 2. Iniciar la música de la nueva zona con un fade in suave
                if (!string.IsNullOrEmpty(musicToPlay))
                {
                    SoundList.Instance.SoundFadeIn(musicToPlay, fadeTime);
                }
            }

            // Destruimos el trigger para que la transición no se repita
            Destroy(gameObject);
        }
    }
}