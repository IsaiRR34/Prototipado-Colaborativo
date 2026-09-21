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
    [SerializeField] private string ambientParam = "VolumeAmbience";

    [Header("Sliders")]
    [SerializeField] private Slider masterSlider;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private Slider ambientSlider;

    private void Start()
    {
        // 1. Definimos el 75% como valor fijo
        float sweetSpot = 0.75f;

        // 2. Configuramos los límites de los sliders y les inyectamos el 75% visualmente
        if (masterSlider != null) { masterSlider.minValue = 0.0001f; masterSlider.maxValue = 1f; masterSlider.SetValueWithoutNotify(sweetSpot); }
        if (musicSlider != null) { musicSlider.minValue = 0.0001f; musicSlider.maxValue = 1f; musicSlider.SetValueWithoutNotify(sweetSpot); }
        if (sfxSlider != null) { sfxSlider.minValue = 0.0001f; sfxSlider.maxValue = 1f; sfxSlider.SetValueWithoutNotify(sweetSpot); }
        if (ambientSlider != null) { ambientSlider.minValue = 0.0001f; ambientSlider.maxValue = 1f; ambientSlider.SetValueWithoutNotify(sweetSpot); }

        // 3. LLAMAMOS A LAS FUNCIONES DIRECTAMENTE (Tu método propuesto)
        SetMasterVolume(sweetSpot);
        SetMusicVolume(sweetSpot);
        SetSFXVolume(sweetSpot);
        SetAmbientVolume(sweetSpot);

        // 4. Suscribimos los eventos de la UI para cuando decidas moverlos en pausa
        if (masterSlider != null) masterSlider.onValueChanged.AddListener(SetMasterVolume);
        if (musicSlider != null) musicSlider.onValueChanged.AddListener(SetMusicVolume);
        if (sfxSlider != null) sfxSlider.onValueChanged.AddListener(SetSFXVolume);
        if (ambientSlider != null) ambientSlider.onValueChanged.AddListener(SetAmbientVolume);
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