using UnityEngine;
using UnityEngine.Audio;
using DG.Tweening;
using System.Collections.Generic;

public class SoundList : MonoBehaviour
{
    public static SoundList Instance { get; private set; }

    [SerializeField] private Sound[] soundList;
    private Dictionary<string, Sound> soundDictionary = new Dictionary<string, Sound>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeSounds();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void InitializeSounds()
    {
        soundDictionary.Clear();
        foreach (var sound in soundList)
        {
            if (string.IsNullOrEmpty(sound.audioName)) continue;

            AudioSource source = gameObject.AddComponent<AudioSource>();
            sound.SetAudioSource(source);
            soundDictionary[sound.audioName] = sound;
        }
    }

    public void PlaySound(string audioName)
    {
        if (soundDictionary.TryGetValue(audioName, out Sound sound))
        {
            sound.source.Play();
        }
        else
        {
            Debug.LogWarning($"[SoundList] Sonido no encontrado: '{audioName}'");
        }
    }

    public void PlaySoundRandomPitch(string audioName, float minPitch = 0.85f, float maxPitch = 1.15f)
    {
        if (soundDictionary.TryGetValue(audioName, out Sound sound))
        {
            sound.source.pitch = Random.Range(minPitch, maxPitch);
            sound.source.Play();
        }
    }

    public void PlaySoundAtPosition(string audioName, Vector3 position, float spatialBlend = 1.0f)
    {
        if (soundDictionary.TryGetValue(audioName, out Sound sound))
        {
            // Instanciar un emisor temporal en la posición para sonido 3D posicional en el mundo
            GameObject tempGO = new GameObject($"SFX_{audioName}");
            tempGO.transform.position = position;

            AudioSource tempSource = tempGO.AddComponent<AudioSource>();
            tempSource.clip = sound.clip;
            tempSource.volume = sound.volume;
            tempSource.pitch = sound.pitch;
            tempSource.outputAudioMixerGroup = sound.mixer;
            tempSource.spatialBlend = spatialBlend;
            tempSource.minDistance = 2f;
            tempSource.maxDistance = 20f;
            tempSource.rolloffMode = AudioRolloffMode.Linear;

            tempSource.Play();
            Destroy(tempGO, sound.clip.length / Mathf.Max(0.1f, sound.pitch));
        }
    }

    public void StopSound(string audioName)
    {
        if (soundDictionary.TryGetValue(audioName, out Sound sound))
        {
            sound.source.Stop();
        }
    }

    public void SoundFadeIn(string audioName, float fadeTime, float delay = 0f)
    {
        if (soundDictionary.TryGetValue(audioName, out Sound sound))
        {
            sound.source.volume = 0f;
            sound.source.Play();
            sound.source.DOFade(sound.volume, fadeTime).SetUpdate(true).SetDelay(delay);
        }
    }
    public float GetClipDuration(string audioName)
    {
        if (soundDictionary.TryGetValue(audioName, out Sound sound))
        {
            return sound.clip != null ? sound.clip.length : 0f;
        }
        return 0f;
    }
}

[System.Serializable]
public class Sound
{
    public string audioName;
    public AudioClip clip;
    [Range(0f, 1f)] public float volume = 1f;
    [Range(0.1f, 3f)] public float pitch = 1f;
    public bool loop = false;
    public AudioMixerGroup mixer;

    [HideInInspector] public AudioSource source;

    public void SetAudioSource(AudioSource audioSource)
    {
        source = audioSource;
        source.clip = clip;
        source.volume = volume;
        source.pitch = pitch;
        source.outputAudioMixerGroup = mixer;
        source.loop = loop;
        source.playOnAwake = false;
        source.spatialBlend = 0f; // 2D estéreo base
    }
}