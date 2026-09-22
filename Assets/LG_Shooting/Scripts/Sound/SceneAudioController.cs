using UnityEngine;
using System.Collections;

public class SceneAudioController : MonoBehaviour
{
    [Header("Audio Inicial del Nivel")]
    [Tooltip("Nombre de la música inicial en SoundList (ej. BGM_Zona1)")]
    [SerializeField] private string startingBGMName;
    [SerializeField] private float fadeTime = 2.0f;

    [Header("Ambientes en Bucle con Pausas")]
    [SerializeField] private bool useLoopingAmbients = false;
    [SerializeField] private string[] loopingAmbientSounds;
    [SerializeField] private float minAmbientPause = 30f;
    [SerializeField] private float maxAmbientPause = 40f;

    [Header("Sonidos Aleatorios Intermitentes")]
    [SerializeField] private bool useRandomAmbientSounds = false;
    [SerializeField] private string[] randomAmbientSounds;
    [SerializeField] private float minRandomWait = 40f;
    [SerializeField] private float maxRandomWait = 60f;

    private void Start()
    {
        if (SoundList.Instance == null) return;

        // 1. LIMPIEZA ABSOLUTA
        // Detiene absolutamente todos los audios y transiciones pendientes sin importar cómo se llamen.
        SoundList.Instance.StopAllSounds();

        // 2. Inicia la música de la Zona 1 (limpia y sin superposiciones)
        if (!string.IsNullOrEmpty(startingBGMName))
        {
            SoundList.Instance.SoundFadeIn(startingBGMName, fadeTime);
        }

        // 3. Inicia las corrutinas de los ambientes con pausas
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

        // 4. Inicia la rutina de sonidos aleatorios si está activada
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

            float clipLength = SoundList.Instance.GetClipDuration(soundName);
            if (clipLength <= 0f) clipLength = 10f;

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
}