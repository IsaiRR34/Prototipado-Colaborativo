using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class VolumeControl : MonoBehaviour
{
    [Header("Audio Mixer")]
    [SerializeField] private AudioMixer audioMixer;

    [Header("Exposed Parameters")]
    [SerializeField] private string masterParam = "VolumeMaster";
    [SerializeField] private string musicParam = "VolumeMusic";
    [SerializeField] private string sfxParam = "VolumeSFX";

    [Header("Sliders")]
    [SerializeField] private Slider masterSlider;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;

    private void Start()
    {
        // 1. Obtener valores: si no hay partida guardada, forzamos el 75% (0.75f)
        float masterVol = PlayerPrefs.HasKey("Pref_MasterVol") ? PlayerPrefs.GetFloat("Pref_MasterVol") : 0.75f;
        float musicVol = PlayerPrefs.HasKey("Pref_MusicVol") ? PlayerPrefs.GetFloat("Pref_MusicVol") : 0.75f;
        float sfxVol = PlayerPrefs.HasKey("Pref_SFXVol") ? PlayerPrefs.GetFloat("Pref_SFXVol") : 0.75f;

        // 2. Actualizar los Sliders visualmente sin activar sus eventos automáticos
        if (masterSlider != null) { masterSlider.minValue = 0.0001f; masterSlider.maxValue = 1f; masterSlider.SetValueWithoutNotify(masterVol); }
        if (musicSlider != null) { musicSlider.minValue = 0.0001f; musicSlider.maxValue = 1f; musicSlider.SetValueWithoutNotify(musicVol); }
        if (sfxSlider != null) { sfxSlider.minValue = 0.0001f; sfxSlider.maxValue = 1f; sfxSlider.SetValueWithoutNotify(sfxVol); }

        // 3. LLAMAR A LAS FUNCIONES DIRECTAMENTE (Tu propuesta)
        SetMasterVolume(masterVol);
        SetMusicVolume(musicVol);
        SetSFXVolume(sfxVol);

        // 4. Suscribir los eventos para cuando el jugador mueva los sliders en pausa
        if (masterSlider != null) masterSlider.onValueChanged.AddListener(SetMasterVolume);
        if (musicSlider != null) musicSlider.onValueChanged.AddListener(SetMusicVolume);
        if (sfxSlider != null) sfxSlider.onValueChanged.AddListener(SetSFXVolume);
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

    private void AplicarVolumen(string parameter, float linearValue)
    {
        if (audioMixer == null) return;
        float db = Mathf.Log10(Mathf.Clamp(linearValue, 0.0001f, 1f)) * 20f;
        audioMixer.SetFloat(parameter, db);
    }
}