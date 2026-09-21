using UnityEngine;
using UnityEngine.EventSystems;

public class LG_EndScreenManager : MonoBehaviour
{
    private void Start()
    {
        // Forzar la liberación del ratón para poder presionar botones de UI
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // El tiempo debe fluir normal
        Time.timeScale = 1f;

        // Asegurarse de que exista un EventSystem para poder hacer click en los botones
        if (Object.FindFirstObjectByType<EventSystem>() == null)
        {
            GameObject esGO = new GameObject("EventSystem");
            esGO.AddComponent<EventSystem>();
            esGO.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
        }
    }
}