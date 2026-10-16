using UnityEngine;
using UnityEngine.Events;

namespace JamKit
{
    [JamHelp("Walks back and forth, turning at walls and ledges. Jump on its head to defeat it ('Stompable'). Its body hurts through the JamDamageZone on the same object.")]
    public class JamPatrolEnemy : JamTriggerBehaviour
    {
        [Tooltip("Walking speed.")]
        [Min(0)] public float speed = 2f;
        [Tooltip("Turn around before walking off a ledge.")]
        public bool turnAtLedges = true;
        [Tooltip("Turn around when bumping into a wall.")]
        public bool turnAtWalls = true;
        [Tooltip("The player can defeat it by landing on top.")]
        public bool stompable = true;
        [Tooltip("How high the player bounces after a stomp.")]
        public float bounceForce = 14f;
        [Tooltip("Start walking to the left instead of the right (3D: backward instead of forward).")]
        public bool startLeft = false;
        [Tooltip("Turn around after walking this far from the start point (units). 0 = only turn at walls and ledges.")]
        [Min(0)] public float patrolRange = 3f;
        [Tooltip("Optional sound when stomped.")]
        public AudioClip stompSound;

        [Header("Events")]
        public UnityEvent OnStomped = new UnityEvent();

        Rigidbody2D rb2;
        Rigidbody rb3;
        Collider2D col2;
        Collider col3;
        SpriteRenderer sprite;
        float dir = 1f;
        float nextTurnTime;
        Vector3 home;

        /// <summary>True when it has walked past its patrol range and is still heading away from home.</summary>
        bool PastRange(Vector3 forward)
        {
            if (patrolRange <= 0f) return false;
            Vector3 offset = transform.position - home;
            offset.y = 0f;
            return offset.magnitude > patrolRange && Vector3.Dot(offset, forward) > 0f;
        }
        bool dead;

        void Awake()
        {
            rb2 = GetComponent<Rigidbody2D>();
            rb3 = GetComponent<Rigidbody>();
            col2 = GetComponent<Collider2D>();
            col3 = GetComponent<Collider>();
            var visual = transform.Find("Visual");
            if (visual != null) sprite = visual.GetComponent<SpriteRenderer>();
            dir = startLeft ? -1f : 1f;
            home = transform.position;
            if (rb2 != null) rb2.freezeRotation = true;
            if (rb3 != null) rb3.constraints = RigidbodyConstraints.FreezeRotation;
        }

        void FixedUpdate()
        {
            if (dead || !JamGameManager.IsPlaying)
            {
                if (rb2 != null && rb2.bodyType == RigidbodyType2D.Dynamic) rb2.linearVelocity = new Vector2(0f, rb2.linearVelocity.y);
                return;
            }
            if (rb2 != null) Patrol2D();
            else if (rb3 != null) Patrol3D();
        }

        void Patrol2D()
        {
            Bounds b = col2 != null ? col2.bounds : new Bounds(transform.position, Vector3.one);
            Vector2 forward = new Vector2(dir, 0f);
            if (Time.time >= nextTurnTime)
            {
                bool wall = turnAtWalls && JamPhysics.Raycast2D(b.center, forward, b.extents.x + 0.08f, transform, out RaycastHit2D wallHit)
                            && wallHit.collider.GetComponentInParent<JamPlayer>() == null;
                bool ledge = turnAtLedges && !(JamPhysics.Raycast2D(new Vector2(b.center.x + dir * (b.extents.x + 0.05f), b.min.y + 0.05f), Vector2.down, 0.4f, transform, out RaycastHit2D floor)
                                              && floor.collider.GetComponentInParent<JamMover>() == null); // moving platforms are not safe ground
                if (wall || ledge || PastRange(forward)) Turn();
            }
            rb2.linearVelocity = new Vector2(dir * speed, rb2.linearVelocity.y);
            if (sprite != null) sprite.flipX = dir < 0f;
        }

        void Patrol3D()
        {
            Bounds b = col3 != null ? col3.bounds : new Bounds(transform.position, Vector3.one);
            Vector3 forward = transform.forward * dir;
            if (Time.time >= nextTurnTime)
            {
                float reach = Mathf.Max(b.extents.x, b.extents.z) + 0.1f;
                bool wall = turnAtWalls && JamPhysics.Raycast3D(b.center, forward, reach, transform, out RaycastHit wallHit)
                            && wallHit.collider.GetComponentInParent<JamPlayer>() == null;
                bool ledge = turnAtLedges && !(JamPhysics.Raycast3D(b.center + forward * reach + Vector3.down * (b.extents.y - 0.05f), Vector3.down, 0.5f, transform, out RaycastHit floor)
                                              && floor.collider.GetComponentInParent<JamMover>() == null);
                if (wall || ledge || PastRange(forward)) Turn();
            }
            Vector3 v = transform.forward * dir * speed;
            v.y = rb3.isKinematic ? 0f : rb3.linearVelocity.y;
            if (rb3.isKinematic) rb3.MovePosition(rb3.position + v * Time.fixedDeltaTime);
            else rb3.linearVelocity = v;
        }

        void Turn()
        {
            dir = -dir;
            nextTurnTime = Time.time + 0.2f;
        }

        public bool IsDefeated => dead;

        /// <summary>True if the player is landing on top of this enemy.</summary>
        public bool IsStomp(JamPlayer player)
        {
            if (player == null) return false;
            Bounds b = col2 != null ? col2.bounds : col3 != null ? col3.bounds : new Bounds(transform.position, Vector3.one);
            // Compare the player's feet with the top half of the enemy (a side bump is not a stomp).
            var pc2 = player.GetComponent<Collider2D>();
            var pc3 = player.GetComponent<Collider>();
            float feet = pc2 != null ? pc2.bounds.min.y : pc3 != null ? pc3.bounds.min.y : player.transform.position.y;
            return feet > b.center.y + b.extents.y * 0.3f && player.Velocity.y <= 0.5f;
        }

        protected override void HandleEnter(GameObject other) => CheckStomp(other);
        protected override void HandleStay(GameObject other) => CheckStomp(other);

        void CheckStomp(GameObject other)
        {
            if (dead || !stompable) return;
            var player = PlayerOf(other);
            if (player == null || player.Health.IsDead || !IsStomp(player)) return;
            dead = true;
            player.Bounce(bounceForce);
            JamAudio.Play(stompSound);
            OnStomped.Invoke();
            var health = GetComponent<JamHealth>();
            if (health != null)
            {
                health.destroyOnDeath = true;
                health.Kill();
            }
            else Destroy(gameObject);
        }
    }
}
