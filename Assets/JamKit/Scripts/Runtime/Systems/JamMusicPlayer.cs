using UnityEngine;

namespace JamKit
{
    [JamHelp("Background music that keeps playing between levels without restarting. Put one in every level; if the clip is the same it simply continues.")]
    [DisallowMultipleComponent]
    public class JamMusicPlayer : JamBehaviour
    {
        [Tooltip("The music loop to play. Leave empty for silence.")]
        public AudioClip clip;
        [Tooltip("Music volume (0–1). Keep it lower than sound effects.")]
        [Range(0f, 1f)] public float volume = 0.5f;

        static JamMusicPlayer current;
        AudioSource source;

        void Awake()
        {
            if (current != null && current != this)
            {
                if (current.clip == clip)
                {
                    current.SetVolume(volume);
                    Destroy(gameObject);
                    return;
                }
                Destroy(current.gameObject);
            }
            current = this;
            transform.SetParent(null); // DontDestroyOnLoad only works on root objects
            DontDestroyOnLoad(gameObject);
            source = GetComponent<AudioSource>();
            if (source == null) source = gameObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.loop = true;
            source.spatialBlend = 0f;
            source.clip = clip;
            source.volume = volume;
            if (clip != null) source.Play();
        }

        public void SetVolume(float v)
        {
            volume = Mathf.Clamp01(v);
            if (source != null) source.volume = volume;
        }

        public void Stop()
        {
            if (source != null) source.Stop();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics() => current = null;
    }
}
