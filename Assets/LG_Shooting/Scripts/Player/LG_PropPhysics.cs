using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class LG_PropPhysics : MonoBehaviour
{
    [Header("Audio de Impacto")]
    [Tooltip("El nombre del SFX a reproducir. Puedes usar 'SFX_Melee_Hit' por ahora.")]
    [SerializeField] private string impactSoundName = "SFX_Melee_Hit";

    [Tooltip("Velocidad mínima del impacto para que suene (evita ruidos al deslizarse suavemente).")]
    [SerializeField] private float minVelocityToPlay = 2.5f;

    private float lastImpactTime;

    private void OnCollisionEnter(Collision collision)
    {
        // Evitamos saturar el audio reproduciéndolo múltiples veces por segundo
        if (Time.time - lastImpactTime < 0.2f) return;

        if (collision.relativeVelocity.magnitude > minVelocityToPlay)
        {
            if (SoundList.Instance != null)
            {
                SoundList.Instance.PlaySoundAtPosition(impactSoundName, transform.position, 1.0f);
            }
            lastImpactTime = Time.time;
        }
    }
}