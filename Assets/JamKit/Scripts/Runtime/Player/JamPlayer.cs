using System;
using UnityEngine;
using UnityEngine.Events;

namespace JamKit
{
    /// <summary>
    /// Shared base of the three player controllers. Every World component finds "the player" through this,
    /// in 2D and 3D alike. Holds health, respawn, fall-off-the-world handling and the designer events.
    /// </summary>
    [RequireComponent(typeof(JamHealth))]
    public abstract class JamPlayer : JamBehaviour
    {
        /// <summary>The active player in the scene (null if none).</summary>
        public static JamPlayer Current { get; private set; }

        [Tooltip("All the feel numbers (speed, jump, gravity…). Swap presets here or edit the asset while playing.")]
        public PlayerConfig config;

        [Tooltip("Falling below this height costs 1 heart and respawns you at the last checkpoint.")]
        public float killY = -20f;

        [Header("Events (add sounds and particles here)")]
        public UnityEvent OnJump = new UnityEvent();
        public UnityEvent OnLand = new UnityEvent();
        public UnityEvent OnDash = new UnityEvent();
        public UnityEvent OnFootstep = new UnityEvent();
        public UnityEvent OnHurt = new UnityEvent();
        public UnityEvent OnDeath = new UnityEvent();

        public JamHealth Health { get; private set; }
        public Vector3 RespawnPoint { get; private set; }

        /// <summary>World-space velocity.</summary>
        public abstract Vector3 Velocity { get; }
        public abstract bool IsGrounded { get; }
        /// <summary>Direction the player faces (2D: x/y, 3D: x/z packed as x/y).</summary>
        public virtual Vector2 Facing => Vector2.right;

        /// <summary>True when input should be read (playing, not hurt-locked, no dialogue open).</summary>
        public bool InputEnabled => JamGameManager.IsPlaying && !JamMessageUI.DialogueOpen && Time.time >= inputLockedUntil && !Health.IsDead;

        protected float inputLockedUntil;
        protected PlayerConfig Cfg => config != null ? config : (fallbackConfig ??= CreateFallback());
        PlayerConfig fallbackConfig;

        protected abstract PlayerConfigKind ConfigKind { get; }

        /// <summary>Launch the player upward (stomping an enemy, springs).</summary>
        public abstract void Bounce(float strength);
        /// <summary>Push the player away from a hit.</summary>
        public abstract void ApplyKnockback(Vector3 direction, float force);
        /// <summary>Move the player instantly and stop all motion.</summary>
        public abstract void TeleportTo(Vector3 position);

        protected virtual void Awake()
        {
            Health = GetComponent<JamHealth>();
            Health.OnDamaged.AddListener(() =>
            {
                inputLockedUntil = Time.time + Cfg.hurtControlLock;
                OnHurt.Invoke();
            });
        }

        protected virtual void OnEnable() => Current = this;

        protected virtual void OnDisable()
        {
            if (Current == this) Current = null;
        }

        protected virtual void Start()
        {
            // A "SpawnPoint" object in the scene sets where the level starts.
            var spawn = GameObject.Find("SpawnPoint");
            if (spawn != null) TeleportTo(spawn.transform.position);
            RespawnPoint = transform.position;
        }

        protected virtual void Update()
        {
            if (transform.position.y < killY && !Health.IsDead && JamGameManager.IsPlaying)
            {
                bool stillAlive = Health.Current > 1;
                Health.Damage(1, transform.position);
                if (stillAlive || Health.IsInvincible) Respawn(false);
            }
        }

        /// <summary>Checkpoints call this.</summary>
        public void SetCheckpoint(Vector3 position) => RespawnPoint = position;

        /// <summary>Teleport to the last checkpoint (or the start). Optionally refill health.</summary>
        public void Respawn(bool refillHealth)
        {
            if (refillHealth) Health.ResetHealth();
            TeleportTo(RespawnPoint);
        }

        /// <summary>Called by JamHealth when it reaches zero.</summary>
        internal void NotifyDeath()
        {
            OnDeath.Invoke();
            if (JamGameManager.Instance != null) JamGameManager.Instance.PlayerDied(this);
            else
            {
                Debug.Log("[JamKit] Player died (no JamGameManager in the scene, so respawning).");
                Respawn(true);
            }
        }

        protected void LockInput(float seconds) => inputLockedUntil = Mathf.Max(inputLockedUntil, Time.time + seconds);

        PlayerConfig CreateFallback()
        {
            var c = ScriptableObject.CreateInstance<PlayerConfig>();
            c.ApplyDefaults(ConfigKind);
            c.name = "PlayerConfig (default)";
            Debug.LogWarning($"[JamKit] {name} has no PlayerConfig assigned; using defaults. Drag one from JamKit/Configs into the Config field.", this);
            return c;
        }

        /// <summary>Finds a child by name anywhere below this object.</summary>
        protected Transform FindChildNamed(string childName)
        {
            foreach (var t in GetComponentsInChildren<Transform>(true))
                if (t != transform && t.name == childName) return t;
            return null;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics() => Current = null;
    }
}
