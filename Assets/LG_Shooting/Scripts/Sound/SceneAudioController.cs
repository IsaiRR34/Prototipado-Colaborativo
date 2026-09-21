using UnityEngine;
using System.Collections;

public class SceneAudioController : MonoBehaviour
{
    [Header("Audio Inicial del Nivel")]
    [Tooltip("Nombre de la música inicial en SoundList (ej. BGM_Zona1)")]
    [SerializeField] private string startingBGMName;
    [SerializeField] private float fadeTime = 2.0f;

    [Header("Ambientes en Bucle con Pausas")]
    [Tooltip("Activa esto si quieres ambientes que se repiten tras una pausa")]
    [SerializeField] private bool useLoopingAmbients = false;

    [Tooltip("Nombres de los audios en SoundList (Ej: Amb_Viento, Amb_Lluvia)")]
    [SerializeField] private string[] loopingAmbientSounds;

    [Tooltip("Pausa mínima entre repeticiones (segundos)")]
    [SerializeField] private float minAmbientPause = 30f;

    [Tooltip("Pausa máxima entre repeticiones (segundos)")]
    [SerializeField] private float maxAmbientPause = 40f;

    [Header("Sonidos Aleatorios Intermitentes")]
    [Tooltip("Activa esto si quieres sonidos esporádicos al azar")]
    [SerializeField] private bool useRandomAmbientSounds = false;

    [Tooltip("Nombres de los audios en SoundList que sonarán al azar")]
    [SerializeField] private string[] randomAmbientSounds;

    [Tooltip("Tiempo mínimo de espera en segundos")]
    [SerializeField] private float minRandomWait = 40f;

    [Tooltip("Tiempo máximo de espera en segundos")]
    [SerializeField] private float maxRandomWait = 60f;

    private void Start()
    {
        if (SoundList.Instance == null) return;

        // Inicia la música de la Zona 1
        if (!string.IsNullOrEmpty(startingBGMName))
        {
            SoundList.Instance.SoundFadeIn(startingBGMName, fadeTime);
        }

        // Inicia las corrutinas de los ambientes con pausas
        if (useLoopingAmbients && loopingAmbientSounds != null && loopingAmbientSounds.Length > 0)
        {
            foreach (string ambient in loopingAmbientSounds)
            {
                if (!string.IsNullOrEmpty(ambient))
                {
                    StartCoroutine(AmbientLoopWithPauseRoutine(ambient));
                }
            }
        }

        // Inicia la rutina de sonidos aleatorios si está activada
        if (useRandomAmbientSounds && randomAmbientSounds != null && randomAmbientSounds.Length > 0)
        {
            StartCoroutine(RandomSoundRoutine());
        }
    }

    private IEnumerator AmbientLoopWithPauseRoutine(string soundName)
    {
        while (true)
        {
            SoundList.Instance.PlaySound(soundName);

            // Obtenemos la duración real del clip
            float clipLength = SoundList.Instance.GetClipDuration(soundName);
            if (clipLength <= 0f) clipLength = 10f; // Failsafe

            yield return new WaitForSeconds(clipLength);

            float pauseTime = Random.Range(minAmbientPause, maxAmbientPause);
            yield return new WaitForSeconds(pauseTime);
        }
    }

    private IEnumerator RandomSoundRoutine()
    {
        while (true)
        {
            float waitTime = Random.Range(minRandomWait, maxRandomWait);
            yield return new WaitForSeconds(waitTime);

            int randomIndex = Random.Range(0, randomAmbientSounds.Length);
            string soundToPlay = randomAmbientSounds[randomIndex];

            if (!string.IsNullOrEmpty(soundToPlay))
            {
                SoundList.Instance.PlaySoundRandomPitch(soundToPlay, 0.9f, 1.1f);
            }
        }
    }

    // Utilidad para extraer la duración del clip desde el SoundList
    private float GetClipLength(string soundName)
    {
        if (SoundList.Instance == null) return 5f; // Valor por defecto de seguridad

        // Accedemos a la lista interna del SoundList buscando el nombre
        // Como el diccionario en SoundList es privado, buscamos el componente AudioSource que tenga el mismo clip
        AudioSource[] sources = SoundList.Instance.GetComponents<AudioSource>();
        foreach (var source in sources)
        {
            if (source.clip != null && source.clip.name.Contains(soundName.Replace("Amb_", "")))
            {
                // Este método aproximado funciona, pero si necesitas exactitud, 
                // requeriremos hacer pública la búsqueda en SoundList.
                return source.clip.length;
            }
        }

        return 10f; // Asumimos 10 segundos si no lo encontramos para no trabar la corrutina
    }
}