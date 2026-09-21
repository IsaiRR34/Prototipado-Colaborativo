using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class Fader : MonoBehaviour
{
    public Image image;
    public float tweenTime = 0.35f;
    public bool fadeOutOnStart = false;
    public UnityEvent onEndFadeEvent;

    private void Awake()
    {
        if (image != null)
        {
            image.enabled = false;
            image.color = new Color(0f, 0f, 0f, 0f);
            image.raycastTarget = false;
        }
        gameObject.SetActive(false);
        Destroy(gameObject);
    }

    public void Fade(bool fadeIn)
    {
        if (image != null)
        {
            image.enabled = false;
            image.color = new Color(0f, 0f, 0f, 0f);
            image.raycastTarget = false;
        }
        onEndFadeEvent?.Invoke();
    }
}
