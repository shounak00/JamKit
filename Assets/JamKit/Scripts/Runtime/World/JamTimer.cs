using UnityEngine;
using UnityEngine.Events;

namespace JamKit
{
    public enum JamTimerMode
    {
        CountUp,
        CountDown
    }

    public enum JamTimerEnd
    {
        Lose,
        Win,
        Nothing
    }

    [JamHelp("A level timer shown on the HUD. Count up for speedruns, or count down: lose at zero (race the clock) or win at zero (survive 60 seconds).")]
    public class JamTimer : JamBehaviour
    {
        /// <summary>The timer the HUD shows (the last one enabled).</summary>
        public static JamTimer Active { get; private set; }

        [Tooltip("Count up from 0, or down from 'Seconds'.")]
        public JamTimerMode mode = JamTimerMode.CountDown;
        [Tooltip("Starting time for a countdown (seconds).")]
        [Min(0)] public float seconds = 60f;
        [Tooltip("What happens when a countdown reaches zero.")]
        public JamTimerEnd onZero = JamTimerEnd.Lose;
        [Tooltip("Start ticking when the level starts. Off = wait for StartTimer().")]
        public bool startOnAwake = true;

        [Header("Events")]
        public UnityEvent OnTimerEnd = new UnityEvent();

        public float Time01 => mode == JamTimerMode.CountDown && seconds > 0f ? Mathf.Clamp01(current / seconds) : 0f;
        public float Current => current;
        public bool Running => running;

        float current;
        bool running;
        bool ended;

        void OnEnable() => Active = this;

        void OnDisable()
        {
            if (Active == this) Active = null;
        }

        void Start()
        {
            current = mode == JamTimerMode.CountDown ? seconds : 0f;
            running = startOnAwake;
        }

        public void StartTimer() => running = true;
        public void StopTimer() => running = false;

        public void ResetTimer()
        {
            current = mode == JamTimerMode.CountDown ? seconds : 0f;
            ended = false;
        }

        /// <summary>Add (or remove, with a negative number) seconds — time pickups.</summary>
        public void AddTime(float amount) => current = Mathf.Max(0f, current + amount);

        void Update()
        {
            if (!running || ended || !JamGameManager.IsPlaying) return;
            if (mode == JamTimerMode.CountUp)
            {
                current += Time.deltaTime;
                return;
            }
            current -= Time.deltaTime;
            if (current > 0f) return;
            current = 0f;
            ended = true;
            running = false;
            OnTimerEnd.Invoke();
            var gm = JamGameManager.Instance;
            if (gm == null) return;
            if (onZero == JamTimerEnd.Lose) gm.Lose();
            else if (onZero == JamTimerEnd.Win) gm.Win();
        }

        /// <summary>"m:ss" text for the HUD.</summary>
        public string Display
        {
            get
            {
                float t = mode == JamTimerMode.CountDown ? Mathf.Ceil(current) : Mathf.Floor(current);
                int m = (int)(t / 60f);
                int s = (int)(t % 60f);
                return $"{m}:{s:00}";
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics() => Active = null;
    }
}
