using UnityEngine;
using UnityEngine.Events;

namespace JamKit
{
    [JamHelp("Hurts whatever touches it (the player by default): spikes, lava, saws, an enemy's body. Works with trigger and solid colliders, 2D and 3D.")]
    public class JamDamageZone : JamTriggerBehaviour
    {
        [Tooltip("Hearts removed per hit.")]
        [Min(0)] public int damage = 1;
        [Tooltip("Kill instantly, whatever the health (bottomless pits, lava).")]
        public bool instantKill = false;
        [Tooltip("Who gets hurt: the player, enemies (anything else with JamHealth), or both.")]
        public JamTargets affects = JamTargets.Player;
        [Tooltip("How hard the victim is pushed away. 0 = no push.")]
        [Min(0)] public float knockbackForce = 8f;
        [Tooltip("Optional sound when it hits.")]
        public AudioClip hitSound;

        [Header("Events")]
        public UnityEvent OnHit = new UnityEvent();

        JamPatrolEnemy ownerEnemy;

        void Awake() => ownerEnemy = GetComponentInParent<JamPatrolEnemy>();

        protected override void HandleEnter(GameObject other) => TryHurt(other);
        protected override void HandleStay(GameObject other) => TryHurt(other);

        void TryHurt(GameObject other)
        {
            if (IsSelf(other)) return;
            if (ownerEnemy != null && ownerEnemy.IsDefeated) return;
            var health = other.GetComponentInParent<JamHealth>();
            if (health == null || health.IsDead) return;
            if (health.transform == transform || transform.IsChildOf(health.transform)) return; // never hurt ourselves

            var player = health.GetComponent<JamPlayer>();
            bool isPlayer = player != null;
            if (affects == JamTargets.Player && !isPlayer) return;
            if (affects == JamTargets.Enemies && isPlayer) return;

            // Landing on top of a stompable enemy is a stomp, not a hit.
            if (isPlayer && ownerEnemy != null && ownerEnemy.stompable && ownerEnemy.IsStomp(player)) return;

            bool landed = instantKill ? KillNow(health) : health.Damage(damage, transform.position, knockbackForce);
            if (!landed) return;
            JamAudio.Play(hitSound);
            OnHit.Invoke();
        }

        static bool KillNow(JamHealth h)
        {
            if (h.IsDead) return false;
            h.Kill();
            return true;
        }

        void OnDrawGizmos()
        {
            Gizmos.color = new Color(1f, 0.2f, 0.2f, 0.6f);
            JamGizmos.DrawColliderOutline(gameObject);
        }
    }
}
