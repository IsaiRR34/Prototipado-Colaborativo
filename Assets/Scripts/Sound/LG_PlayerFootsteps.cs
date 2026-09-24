using UnityEngine;
using UnityEngine.InputSystem;

public class LG_PlayerFootsteps : MonoBehaviour
{
    [Header("Sonidos en SoundList")]
    [SerializeField] private string walkSound = "SFX_Player_Walk";
    [SerializeField] private string runSound = "SFX_Player_Run";

    [Header("Ritmo de Pasos (Segundos)")]
    [SerializeField] private float walkInterval = 0.5f;
    [SerializeField] private float runInterval = 0.32f;

    private float stepTimer;
    private Vector3 lastPosition;

    private void Start()
    {
        lastPosition = transform.position;
    }

    private void Update()
    {
        // Calculamos la velocidad plana (X, Z) para evitar que suenen pasos al caer o saltar
        Vector3 currentPos = new Vector3(transform.position.x, 0f, transform.position.z);
        Vector3 lastPos = new Vector3(lastPosition.x, 0f, lastPosition.z);

        float currentSpeed = Vector3.Distance(currentPos, lastPos) / Time.deltaTime;
        lastPosition = transform.position;

        // Si el jugador se está moviendo físicamente por el mundo
        if (currentSpeed > 0.5f)
        {
            stepTimer -= Time.deltaTime;

            if (stepTimer <= 0f)
            {
                bool isRunning = Keyboard.current != null && Keyboard.current.leftShiftKey.isPressed;

                string soundToPlay = isRunning ? runSound : walkSound;

                if (SoundList.Instance != null)
                {
                    // Variamos levemente el tono para que el suelo no suene artificial y repetitivo
                    SoundList.Instance.PlaySoundRandomPitch(soundToPlay, 0.92f, 1.08f);
                }

                stepTimer = isRunning ? runInterval : walkInterval;
            }
        }
        else
        {
            stepTimer = 0f; // Reinicia el ciclo al frenar
        }
    }
}