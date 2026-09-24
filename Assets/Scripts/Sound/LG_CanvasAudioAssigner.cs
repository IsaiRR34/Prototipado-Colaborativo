using UnityEngine;
using UnityEngine.UI;

public class LG_CanvasAudioAssigner : MonoBehaviour
{
    private void Awake()
    {
        // Busca todos los botones dentro de este Canvas y les inyecta el feedback de audio automáticamente
        Button[] allButtons = GetComponentsInChildren<Button>(true);
        foreach (var btn in allButtons)
        {
            if (btn.GetComponent<LG_UIAudioFeedback>() == null)
            {
                btn.gameObject.AddComponent<LG_UIAudioFeedback>();
            }
        }
    }
}