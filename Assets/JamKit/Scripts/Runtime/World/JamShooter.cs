using UnityEngine;
using UnityEngine.Events;

namespace JamKit
{
    [JamHelp("Fires a projectile prefab, either every few seconds (turrets) or when the player presses Fire (J / Ctrl / left mouse). On the player it aims where the player faces.")]
    public class JamShooter : JamBehaviour
    {
        [Tooltip("The projectile prefab to fire (should have JamProjectile on it).")]
        public GameObject projectile;
        [Tooltip("Seconds between shots (automatic), or the minimum time between shots (on input).")]
        [Min(0.02f)] public float interval = 1f;
        [Tooltip("Fire direction. (0,0) = where the player faces, or this object's right (2D) / forward (3D).")]
        public Vector2 direction = Vector2.zero;
        [Tooltip("Fire when the player presses the Fire button instead of automatically.")]
        public bool fireOnInput = false;
        [Tooltip("Turrets: aim at the player instead of using Direction.")]
        public bool aimAtPlayer = false;
        [Tooltip("Projectile speed.")]
        public float projectileSpeed = 10f;
        [Tooltip("Where shots come from. Empty = a child called 'FirePoint', or this object.")]
        public Transform firePoint;
        [Tooltip("Only shoot when the player is closer than this (0 = always).")]
        [Min(0)] public float range = 0f;
        [Tooltip("Optional sound per shot.")]
        public AudioClip fireSound;

        [Header("Events")]
        public UnityEvent OnFire = new UnityEvent();

        JamPlayer owner;
        float nextShot;

        void Awake()
        {
            owner = GetComponentInParent<JamPlayer>();
            if (firePoint == null) firePoint = transform.Find("FirePoint");
            if (firePoint == null) firePoint = transform;
            nextShot = Time.time + (fireOnInput ? 0f : interval);
        }

        void Update()
        {
            if (!JamGameManager.IsPlaying || projectile == null || Time.time < nextShot) return;
            if (fireOnInput)
            {
                if (owner != null && !owner.InputEnabled) return;
                if (!JamInput.FirePressed && !(JamInput.FireHeld && interval < 0.3f)) return;
            }
            if (range > 0f)
            {
                var p = JamPlayer.Current;
                if (p == null || Vector3.Distance(p.transform.position, transform.position) > range) return;
            }
            Fire();
        }

        /// <summary>Fire one shot now (also callable from events).</summary>
        public void Fire()
        {
            if (projectile == null) return;
            nextShot = Time.time + interval;
            Vector3 dir = AimDirection();
            var go = Instantiate(projectile, firePoint.position, Quaternion.identity);
            var proj = go.GetComponent<JamProjectile>();
            if (proj != null) proj.Launch(dir, projectileSpeed, owner != null ? owner.transform : transform);
            JamAudio.Play(fireSound);
            OnFire.Invoke();
        }

        Vector3 AimDirection()
        {
            bool is3D = GetComponentInParent<Rigidbody>() != null || GetComponentInParent<CharacterController>() != null;
            if (aimAtPlayer && JamPlayer.Current != null)
            {
                Vector3 d = JamPlayer.Current.transform.position - firePoint.position;
                if (is3D) d.y = 0f;
                if (d.sqrMagnitude > 0.0001f) return d.normalized;
            }
            if (direction.sqrMagnitude > 0.0001f)
                return is3D ? transform.TransformDirection(new Vector3(direction.x, 0f, direction.y)).normalized : (Vector3)direction.normalized;
            if (owner != null)
            {
                Vector2 f = owner.Facing;
                if (is3D) return new Vector3(f.x, 0f, f.y).normalized;
                if (f.sqrMagnitude > 0.0001f) return f.normalized;
            }
            return is3D ? transform.forward : transform.right;
        }
    }
}
