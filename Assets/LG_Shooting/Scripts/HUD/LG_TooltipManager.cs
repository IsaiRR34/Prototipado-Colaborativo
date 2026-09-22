using UnityEngine;
using TMPro;
using System.Collections;

public class LG_TooltipManager : MonoBehaviour
{
    public static LG_TooltipManager Instance { get; private set; }

    [Header("UI Reference")]
    [SerializeField] private TextMeshProUGUI tooltipText;

    private Coroutine tempTooltipCoroutine;

    private void Awake()
    {
        // Fuerza la asignación limpia a esta nueva instancia
        Instance = this;
    }

    private void OnDestroy()
    {
        // Limpiamos el fantasma de la memoria al recargar la escena
        if (Instance == this) Instance = null;
    }

    private void Start()
    {
        HideTooltip();
    }

    // Para la Terminal y Puerta (Se queda visible hasta que te alejas)
    public void ShowTooltip(string message)
    {
        if (tempTooltipCoroutine != null)
        {
            StopCoroutine(tempTooltipCoroutine);
            tempTooltipCoroutine = null;
        }

        if (tooltipText != null)
        {
            tooltipText.text = message;
            tooltipText.gameObject.SetActive(true);
        }
    }

    public void HideTooltip()
    {
        if (tempTooltipCoroutine != null) return; // No ocultar si hay un mensaje temporal en pantalla

        if (tooltipText != null)
        {
            tooltipText.gameObject.SetActive(false);
            tooltipText.text = "";
        }
    }

    // Para los Pickups (Se oculta solo después de un tiempo)
    public void ShowTooltipTemporary(string message, float duration)
    {
        if (tempTooltipCoroutine != null) StopCoroutine(tempTooltipCoroutine);
        tempTooltipCoroutine = StartCoroutine(TemporaryTooltipRoutine(message, duration));
    }

    private IEnumerator TemporaryTooltipRoutine(string message, float duration)
    {
        if (tooltipText != null)
        {
            tooltipText.text = message;
            tooltipText.gameObject.SetActive(true);
        }

        yield return new WaitForSeconds(duration);

        if (tooltipText != null)
        {
            tooltipText.gameObject.SetActive(false);
            tooltipText.text = "";
        }
        tempTooltipCoroutine = null;
    }
}