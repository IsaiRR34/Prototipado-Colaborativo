using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class VolumeControl : MonoBehaviour
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
        // Configuramos sliders de 0.0001 a 1 para curva logarítmica real
        ConfigurarSlider(masterSlider, masterParam, "Pref_MasterVol");
        ConfigurarSlider(musicSlider, musicParam, "Pref_MusicVol");
        ConfigurarSlider(sfxSlider, sfxParam, "Pref_SFXVol");

        // Listeners automáticos
        if (masterSlider != null) masterSlider.onValueChanged.AddListener(SetMasterVolume);
        if (musicSlider != null) musicSlider.onValueChanged.AddListener(SetMusicVolume);
        if (sfxSlider != null) sfxSlider.onValueChanged.AddListener(SetSFXVolume);
    }

    private void ConfigurarSlider(Slider slider, string paramName, string prefKey)
    {
        if (slider == null || audioMixer == null) return;

        slider.minValue = 0.0001f;
        slider.maxValue = 1f;

        float savedLinear = PlayerPrefs.GetFloat(prefKey, 0.75f);
        slider.value = savedLinear;
        AplicarVolumen(paramName, savedLinear);
    }

    public void SetMasterVolume(float value)
    {
        AplicarVolumen(masterParam, value);
        PlayerPrefs.SetFloat("Pref_MasterVol", value);
    }

    public void SetMusicVolume(float value)
    {
        AplicarVolumen(musicParam, value);
        PlayerPrefs.SetFloat("Pref_MusicVol", value);
    }

    public void SetSFXVolume(float value)
    {
        AplicarVolumen(sfxParam, value);
        PlayerPrefs.SetFloat("Pref_SFXVol", value);
    }

    private void AplicarVolumen(string parameter, float linearValue)
    {
        if (audioMixer == null) return;
        // Conversión estándar Lineal -> Logarítmico (Decibelios)
        float db = Mathf.Log10(Mathf.Clamp(linearValue, 0.0001f, 1f)) * 20f;
        audioMixer.SetFloat(parameter, db);
    }
}