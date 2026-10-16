using System;
using UnityEngine;
using UnityEngine.Events;

namespace JamKit
{
    [JamHelp("Hit points. Damage zones and enemies take these away; at zero the player loses (or loses a life) and enemies die. Works for players and enemies.")]
    [DisallowMultipleComponent]
    public class JamHealth : JamBehaviour
    {
        [Tooltip("Hit points at the start (hearts on the HUD).")]
        [Min(1)] public int maxHealth = 3;
        [Tooltip("Seconds of invincibility after being hit (the character blinks).")]
        [Min(0)] public float invincibilityTime = 1f;
        [Tooltip("Blink the character's sprites/meshes while invincible.")]
        public bool flashWhenHurt = true;
        [Tooltip("Remove this object when it dies. Turn ON for enemies, leave OFF for the player.")]
        public bool destroyOnDeath = false;
        [Tooltip("Optional effect prefab spawned on death (particles, a puff…).")]
        public GameObject deathEffect;
        [Tooltip("Optional sound when hurt.")]
        public AudioClip hurtSound;
        [Tooltip("Optional sound on death.")]
        public AudioClip deathSound;

        [Header("Events")]
        public UnityEvent OnDamaged = new UnityEvent();
        public UnityEvent OnHealed = new UnityEvent();
        public UnityEvent OnDied = new UnityEvent();

        /// <summary>Raised for any JamHealth change (the HUD listens to this).</summary>
        public static event Action<JamHealth> AnyChanged;

        public int Current { get; private set; }
        public bool IsDead { get; private set; }
        public bool IsInvincible => Time.time < invincibleUntil;

        float invincibleUntil;
        Renderer[] renderers;
        bool flashing;

        void Awake()
        {
            Current = maxHealth;
            renderers = GetComponentsInChildren<Renderer>(true);
        }

        /// <summary>Deal damage. Returns true if it landed (not invincible or already dead).</summary>
        public bool Damage(int amount, Vector3 sourcePosition, float knockback = 0f)
        {
            if (IsDead || amount <= 0 || IsInvincible) return false;
            Current = Mathf.Max(0, Current - amount);
            AnyChanged?.Invoke(this);
            JamAudio.Play(hurtSound);

            if (knockback > 0f)
            {
                Vector3 dir = transform.position - sourcePosition;
                if (dir.sqrMagnitude < 0.0001f) dir = Vector3.up;
                var player = GetComponent<JamPlayer>();
                if (player != null) player.ApplyKnockback(dir.normalized, knockback);
                else Push(dir.normalized * knockback);
            }

            if (Current <= 0)
            {
                Die();
                return true;
            }
            OnDamaged.Invoke();
            SetInvincible(invincibilityTime);
            return true;
        }

        /// <summary>Simple version for UnityEvents: 1 damage, no knockback.</summary>
        public void TakeDamage(int amount) => Damage(amount, transform.position);

        public void Heal(int amount)
        {
            if (IsDead || amount <= 0 || Current >= maxHealth) return;
            Current = Mathf.Min(maxHealth, Current + amount);
            AnyChanged?.Invoke(this);
            OnHealed.Invoke();
        }

        public void Kill()
        {
            if (IsDead) return;
            Current = 0;
            AnyChanged?.Invoke(this);
            Die();
        }

        /// <summary>Back to full health and alive (used on respawn).</summary>
        public void ResetHealth()
        {
            IsDead = false;
            Current = maxHealth;
            AnyChanged?.Invoke(this);
        }

        public void SetInvincible(float seconds)
        {
            invincibleUntil = Mathf.Max(invincibleUntil, Time.time + seconds);
            if (flashWhenHurt && seconds > 0f && !flashing) StartCoroutine(Flash());
        }

        void Die()
        {
            IsDead = true;
            JamAudio.Play(deathSound);
            if (deathEffect != null) Instantiate(deathEffect, transform.position, Quaternion.identity);
            OnDied.Invoke();

            var player = GetComponent<JamPlayer>();
            if (player != null)
            {
                player.NotifyDeath();
                return;
            }
            if (destroyOnDeath)
            {
                JamCollectible.PlayAndDetachFX(transform);
                Destroy(gameObject);
            }
        }

        void Push(Vector3 impulse)
        {
            var rb2 = GetComponent<Rigidbody2D>();
            if (rb2 != null && rb2.bodyType == RigidbodyType2D.Dynamic)
            {
                rb2.AddForce(impulse, ForceMode2D.Impulse);
                return;
            }
            var rb = GetComponent<Rigidbody>();
            if (rb != null && !rb.isKinematic) rb.AddForce(impulse, ForceMode.Impulse);
        }

        System.Collections.IEnumerator Flash()
        {
            flashing = true;
            bool visible = true;
            while (IsInvincible && !IsDead)
            {
                visible = !visible;
                SetRenderers(visible);
                yield return new WaitForSeconds(0.08f);
            }
            SetRenderers(true);
            flashing = false;
        }

        void SetRenderers(bool on)
        {
            if (renderers == null) return;
            foreach (var r in renderers)
                if (r != null && !(r is ParticleSystemRenderer)) r.enabled = on;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics() => AnyChanged = null;
    }
}
