using UnityEngine;

public class LG_PlayerPusher : MonoBehaviour
{
    [Header("Configuración de Físicas")]
    [Tooltip("Fuerza base con la que el jugador empuja los objetos.")]
    [SerializeField] private float pushPower = 2.5f;

    [Tooltip("El límite máximo de peso que el jugador puede mover (kg).")]
    [SerializeField] private float weightLimit = 80f;

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        Rigidbody rb = hit.collider.attachedRigidbody;

        // Si el objeto no tiene física, está estático o es demasiado pesado, lo ignoramos
        if (rb == null || rb.isKinematic || rb.mass > weightLimit) return;

        // Evitamos empujar el suelo u objetos sobre los que estamos parados
        if (hit.moveDirection.y < -0.3f) return;

        // Calculamos la dirección del empuje (limitada al plano X, Z)
        Vector3 pushDir = new Vector3(hit.moveDirection.x, 0f, hit.moveDirection.z);

        // La fuerza aplicada es inversamente proporcional a la masa del objeto.
        // Las cajas ligeras saldrán volando, las bancas pesadas se moverán muy lento.
        float appliedForce = pushPower * (weightLimit / rb.mass);

        rb.AddForce(pushDir * appliedForce, ForceMode.Impulse);
    }
}