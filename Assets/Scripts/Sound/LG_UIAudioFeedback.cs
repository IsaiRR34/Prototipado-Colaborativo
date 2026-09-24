using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Selectable))]
public class LG_UIAudioFeedback : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler, ISelectHandler, ISubmitHandler
{
    [Header("Nombres en SoundList")]
    [SerializeField] private string hoverSound = "SFX_UI_Hover";
    [SerializeField] private string clickSound = "SFX_UI_Click";

    // Detecta el ratón pasando por encima
    public void OnPointerEnter(PointerEventData eventData) => PlayHover();

    // Detecta la navegación por teclado/mando (W, S, Flechas)
    public void OnSelect(BaseEventData eventData) => PlayHover();

    // Detecta el clic izquierdo del ratón
    public void OnPointerClick(PointerEventData eventData) => PlayClick();

    // Detecta la confirmación por teclado (Space, Enter, F)
    public void OnSubmit(BaseEventData eventData) => PlayClick();

    private void PlayHover()
    {
        if (SoundList.Instance != null) SoundList.Instance.PlaySound(hoverSound);
    }

    private void PlayClick()
    {
        if (SoundList.Instance != null) SoundList.Instance.PlaySound(clickSound);
    }
}