using UnityEngine;
using UnityEngine.Events;
using DG.Tweening;
using UnityEngine.UI;

public class Fader : MonoBehaviour
{
    public Image image;
    public float tweenTime = 0.35f;
    public bool fadeOutOnStart = true;
    public UnityEvent onEndFadeEvent;

    private void Start()
    {
        if (fadeOutOnStart)
        {
            Fade(false); // Inicia en negro y se desvanece a transparente (entrar a la escena)
        }
    }

    /// <summary>
    /// Realiza un fundido.
    /// fadeIn = true: La pantalla se pone negra.
    /// fadeIn = false: La pantalla se aclara (transparente).
    /// </summary>
    public void Fade(bool fadeIn)
    {
        // 1. Bloqueamos al jugador al iniciar la transición (excepto si estamos aclarando la pantalla para empezar a jugar)
        if (fadeIn)
        {
            TimeManager.Instance.LockPlayerForTransition(true);
        }

        float targetValue = fadeIn ? 1f : 0f;

        if (image.type == Image.Type.Filled)
        {
            image.DOFillAmount(targetValue, tweenTime)
                 .SetUpdate(true) // Asegura que funcione en pausa
                 .OnComplete(() => OnFadeComplete(fadeIn));
            return;
        }

        image.DOFade(targetValue, tweenTime)
             .SetUpdate(true) // Asegura que funcione en pausa
             .OnComplete(() => OnFadeComplete(fadeIn));
    }

    private void OnFadeComplete(bool fadeIn)
    {
        // 2. Si la pantalla se aclaró por completo (empezamos a jugar), desbloqueamos al jugador.
        if (!fadeIn)
        {
            TimeManager.Instance.LockPlayerForTransition(false);
        }

        // Ejecutar cualquier evento adicional configurado en el Inspector
        onEndFadeEvent?.Invoke();
    }
}