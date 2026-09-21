using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using System.Collections;

public class VolumeControl : MonoBehaviour
{
    [Header("Audio Mixer")]
    [SerializeField] private AudioMixer audioMixer;

    [Header("Exposed Parameters (Nombres exactos en el Mixer)")]
    [SerializeField] private string masterParam = "VolumeMaster";
    [SerializeField] private string musicParam = "VolumeMusic";
    [SerializeField] private string sfxParam = "VolumeSFX";
    [SerializeField] private string ambientParam = "VolumeAmbience"; // Nuevo parámetro

    [Header("Sliders")]
    [SerializeField] private Slider masterSlider;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private Slider ambientSlider; // Nuevo Slider

    private void Start()
    {
        // Retrasamos 1 frame la inicialización para asegurar que el AudioMixer esté listo.
        StartCoroutine(InitVolumesRoutine());
    }

    private IEnumerator InitVolumesRoutine()
    {
        yield return null;

        // Fijamos el "sweet spot" en 0.75f (75%)
        ConfigurarSlider(masterSlider, masterParam, "Pref_MasterVol", 0.75f);
        ConfigurarSlider(musicSlider, musicParam, "Pref_MusicVol", 0.75f);
        ConfigurarSlider(sfxSlider, sfxParam, "Pref_SFXVol", 0.75f);
        ConfigurarSlider(ambientSlider, ambientParam, "Pref_AmbientVol", 0.75f); // Configuramos el ambiental

        // Listeners automáticos
        if (masterSlider != null) masterSlider.onValueChanged.AddListener(SetMasterVolume);
        if (musicSlider != null) musicSlider.onValueChanged.AddListener(SetMusicVolume);
        if (sfxSlider != null) sfxSlider.onValueChanged.AddListener(SetSFXVolume);
        if (ambientSlider != null) ambientSlider.onValueChanged.AddListener(SetAmbientVolume);
    }

    private void ConfigurarSlider(Slider slider, string paramName, string prefKey, float defaultValue)
    {
        if (slider == null || audioMixer == null) return;

        slider.minValue = 0.0001f;
        slider.maxValue = 1f;

        float savedLinear = PlayerPrefs.GetFloat(prefKey, defaultValue);
        slider.SetValueWithoutNotify(savedLinear);
        AplicarVolumen(paramName, savedLinear);
    }

    public void SetMasterVolume(float value)
    {
        AplicarVolumen(masterParam, value);
        PlayerPrefs.SetFloat("Pref_MasterVol", value);
        PlayerPrefs.Save();
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

    public void SetAmbientVolume(float value)
    {
        AplicarVolumen(ambientParam, value);
        PlayerPrefs.SetFloat("Pref_AmbientVol", value);
        PlayerPrefs.Save();
    }

    private void AplicarVolumen(string parameter, float linearValue)
    {
        if (audioMixer == null) return;
        float db = Mathf.Log10(Mathf.Clamp(linearValue, 0.0001f, 1f)) * 20f;
        audioMixer.SetFloat(parameter, db);
    }
}