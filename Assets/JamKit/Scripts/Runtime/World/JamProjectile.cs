using UnityEngine;

namespace JamKit
{
    [JamHelp("A bullet / fireball fired by JamShooter. Flies straight, hurts what it hits, disappears on walls or after its lifetime.")]
    public class JamProjectile : JamTriggerBehaviour
    {
        [Tooltip("Hearts removed on hit.")]
        [Min(0)] public int damage = 1;
        [Tooltip("Who it can hurt. Player bullets: Enemies. Enemy bullets: Player.")]
        public JamTargets affects = JamTargets.Enemies;
        [Tooltip("Seconds before it disappears on its own.")]
        [Min(0.1f)] public float lifetime = 3f;
        [Tooltip("Push applied to what it hits.")]
        public float knockbackForce = 4f;
        [Tooltip("Optional effect spawned on impact.")]
        public GameObject hitEffect;

        Transform owner;
        Vector3 velocity;
        Rigidbody2D rb2;
        Rigidbody rb3;

        public void Launch(Vector3 direction, float speed, Transform shooter)
        {
            owner = shooter;
            velocity = direction.normalized * speed;
            rb2 = GetComponent<Rigidbody2D>();
            rb3 = GetComponent<Rigidbody>();
            if (rb2 != null)
            {
                rb2.gravityScale = 0f;
                rb2.linearVelocity = velocity;
            }
            if (rb3 != null)
            {
                rb3.useGravity = false;
                rb3.linearVelocity = velocity;
            }
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            if (rb3 == null) transform.rotation = Quaternion.Euler(0f, 0f, angle);
            else transform.rotation = Quaternion.LookRotation(direction);
            Destroy(gameObject, lifetime);
        }

        protected override void Update()
        {
            base.Update();
            if (rb2 == null && rb3 == null) transform.position += velocity * Time.deltaTime;
        }

        protected override void HandleEnter(GameObject other)
        {
            if (IsSelf(other)) return;
            if (owner != null && other.transform.IsChildOf(owner)) return;
            if (other.GetComponentInParent<JamProjectile>() != null) return;

            var health = other.GetComponentInParent<JamHealth>();
            if (health != null)
            {
                bool isPlayer = health.GetComponent<JamPlayer>() != null;
                if (affects == JamTargets.Player && !isPlayer) return;
                if (affects == JamTargets.Enemies && isPlayer) return;
                health.Damage(damage, transform.position, knockbackForce);
                Impact();
                return;
            }

            // Solid things stop it; other triggers (coins, zones) don't.
            var c2 = other.GetComponent<Collider2D>();
            var c3 = other.GetComponent<Collider>();
            bool solid = (c2 != null && !c2.isTrigger) || (c3 != null && !c3.isTrigger);
            if (solid) Impact();
        }

        void Impact()
        {
            if (hitEffect != null) Instantiate(hitEffect, transform.position, Quaternion.identity);
            Destroy(gameObject);
        }
    }
}
