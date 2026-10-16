using UnityEngine;

namespace JamKit
{
    [JamHelp("Plays sound effects from any event. In an event list, drag this object in and pick JamAudio → PlayOneShot, then drop your sound in.")]
    public class JamAudio : JamBehaviour
    {
        [Tooltip("Volume of sounds played through this component (0–1).")]
        [Range(0f, 1f)] public float volume = 0.8f;
        [Tooltip("Random pitch change per play, so repeated sounds don't feel robotic. 0 = always the same pitch.")]
        [Range(0f, 0.5f)] public float randomPitch = 0.08f;

        /// <summary>Plays a clip once. Hook this into any UnityEvent.</summary>
        public void PlayOneShot(AudioClip clip) => Play(clip, volume, randomPitch);

        // ------------------------------------------------------------------ Static helper used by every JamKit component

        static AudioSource source;

        /// <summary>Plays a 2D sound once. Safe to call with a null clip.</summary>
        public static void Play(AudioClip clip, float volume = 0.8f, float randomPitch = 0.08f)
        {
            if (clip == null) return;
            if (source == null)
            {
                var go = new GameObject("JamAudio (one-shots)");
                DontDestroyOnLoad(go);
                source = go.AddComponent<AudioSource>();
                source.playOnAwake = false;
                source.spatialBlend = 0f;
            }
            source.pitch = 1f + Random.Range(-randomPitch, randomPitch);
            source.PlayOneShot(clip, volume);
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics() => source = null;
    }
}
