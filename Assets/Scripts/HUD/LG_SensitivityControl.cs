using UnityEngine;
using UnityEngine.UI;

public class LG_SensitivityControl : MonoBehaviour
{
    // Variable estática global: cualquier script puede leerla escribiendo LG_SensitivityControl.CurrentMultiplier
    public static float CurrentMultiplier { get; private set; } = 1f;

    [Header("Slider")]
    [SerializeField] private Slider sensitivitySlider;

    [Header("Ajustes")]
    [SerializeField] private float minMultiplier = 0.1f;
    [SerializeField] private float maxMultiplier = 3f;
    [SerializeField] private float defaultMultiplier = 1f;

    private const string PrefKey = "Pref_MouseSens";

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void LoadSensitivity()
    {
        // Carga la sensibilidad en memoria antes de que arranque la escena
        CurrentMultiplier = PlayerPrefs.GetFloat(PrefKey, 1f);
    }

    private void Start()
    {
        if (sensitivitySlider != null)
        {
            sensitivitySlider.minValue = minMultiplier;
            sensitivitySlider.maxValue = maxMultiplier;

            float savedValue = PlayerPrefs.GetFloat(PrefKey, defaultMultiplier);
            sensitivitySlider.SetValueWithoutNotify(savedValue);
            CurrentMultiplier = savedValue;

            sensitivitySlider.onValueChanged.AddListener(SetSensitivity);
        }
    }

    public void SetSensitivity(float value)
    {
        CurrentMultiplier = value;
        PlayerPrefs.SetFloat(PrefKey, value);
        PlayerPrefs.Save();
    }
}