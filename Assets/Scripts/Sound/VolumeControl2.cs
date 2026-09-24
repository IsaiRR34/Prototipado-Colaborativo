using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class VolumeControl2 : MonoBehaviour
{
    [Header("Audio Mixer")]
    [SerializeField] private AudioMixer audioMixer;

    [Header("Exposed Parameters (Nombres exactos en el Mixer)")]
    [SerializeField] private string masterParam = "VolumeMaster";
    [SerializeField] private string musicParam = "VolumeMusic";
    [SerializeField] private string sfxParam = "VolumeSFX";

    [Header("Sliders")]
    [SerializeField] private Slider masterSlider;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;

    private const float MinDb = -80f;

    private void Start()
    {
        // Configuramos sliders de 0.0001 a 1 para curva logarítmica real.
        // El "1f" al final asegura que si no hay guardado previo, empiece al máximo volumen.
        ConfigurarSlider(masterSlider, masterParam, "Pref_MasterVol", 1f);
        ConfigurarSlider(musicSlider, musicParam, "Pref_MusicVol", 1f);
        ConfigurarSlider(sfxSlider, sfxParam, "Pref_SFXVol", 1f);

        // Listeners automáticos
        if (masterSlider != null) masterSlider.onValueChanged.AddListener(SetMasterVolume);
        if (musicSlider != null) musicSlider.onValueChanged.AddListener(SetMusicVolume);
        if (sfxSlider != null) sfxSlider.onValueChanged.AddListener(SetSFXVolume);
    }

    private void ConfigurarSlider(Slider slider, string paramName, string prefKey, float defaultValue)
    {
        if (slider == null || audioMixer == null) return;

        slider.minValue = 0.0001f;
        slider.maxValue = 1f;

        // Obtenemos el valor guardado, o el valor por defecto (1f)
        float savedLinear = PlayerPrefs.GetFloat(prefKey, defaultValue);

        // Asignamos el valor al slider sin disparar el evento onValueChanged (para evitar bucles en Start)
        slider.SetValueWithoutNotify(savedLinear);

        // Aplicamos el volumen real al Mixer inmediatamente
        AplicarVolumen(paramName, savedLinear);
    }

    public void SetMasterVolume(float value)
    {
        AplicarVolumen(masterParam, value);
        PlayerPrefs.SetFloat("Pref_MasterVol", value);
        PlayerPrefs.Save(); // Forzamos el guardado
    }

    public void SetMusicVolume(float value)
    {
        AplicarVolumen(musicParam, value);
        PlayerPrefs.SetFloat("Pref_MusicVol", value);
        PlayerPrefs.Save();
    }

    public void SetSFXVolume(float value)
    {
        AplicarVolumen(sfxParam, value);
        PlayerPrefs.SetFloat("Pref_SFXVol", value);
        PlayerPrefs.Save();
    }

    private void AplicarVolumen(string parameter, float linearValue)
    {
        if (audioMixer == null) return;
        // Conversión estándar Lineal -> Logarítmico (Decibelios)
        float db = Mathf.Log10(Mathf.Clamp(linearValue, 0.0001f, 1f)) * 20f;
        audioMixer.SetFloat(parameter, db);
    }
}