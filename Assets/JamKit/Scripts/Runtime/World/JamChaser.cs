using UnityEngine;

namespace JamKit
{
    [JamHelp("Moves toward the player when they come within range (top-down enemies, ghosts). Add a JamDamageZone so touching it hurts.")]
    public class JamChaser : JamBehaviour
    {
        [Tooltip("Chase speed.")]
        [Min(0)] public float speed = 3f;
        [Tooltip("Starts chasing when the player is this close (orange circle).")]
        [Min(0)] public float detectRange = 6f;
        [Tooltip("Gives up when the player gets this far away.")]
        [Min(0)] public float loseRange = 10f;
        [Tooltip("Walk back to the start point after giving up.")]
        public bool returnHome = true;

        Rigidbody2D rb2;
        Rigidbody rb3;
        SpriteRenderer sprite;
        Vector3 home;
        bool chasing;

        void Awake()
        {
            rb2 = GetComponent<Rigidbody2D>();
            rb3 = GetComponent<Rigidbody>();
            var visual = transform.Find("Visual");
            if (visual != null) sprite = visual.GetComponent<SpriteRenderer>();
            if (rb2 != null)
            {
                rb2.gravityScale = 0f;
                rb2.freezeRotation = true;
            }
            if (rb3 != null) rb3.constraints = RigidbodyConstraints.FreezeRotation;
            home = transform.position;
        }

        void FixedUpdate()
        {
            Vector3 velocity = Vector3.zero;
            var player = JamPlayer.Current;
            if (player != null && JamGameManager.IsPlaying && !player.Health.IsDead)
            {
                float d = Vector3.Distance(transform.position, player.transform.position);
                if (!chasing && d <= detectRange) chasing = true;
                else if (chasing && d > loseRange) chasing = false;
                if (chasing) velocity = Direction(player.transform.position) * speed;
            }
            else chasing = false;

            if (!chasing && returnHome && Vector3.Distance(transform.position, home) > 0.1f)
                velocity = Direction(home) * speed * 0.6f;

            if (rb2 != null) rb2.linearVelocity = velocity;
            else if (rb3 != null)
            {
                velocity.y = rb3.isKinematic ? 0f : rb3.linearVelocity.y;
                if (rb3.isKinematic) rb3.MovePosition(rb3.position + velocity * Time.fixedDeltaTime);
                else rb3.linearVelocity = velocity;
                Vector3 flat = new Vector3(velocity.x, 0f, velocity.z);
                if (flat.sqrMagnitude > 0.01f) rb3.MoveRotation(Quaternion.LookRotation(flat));
            }
            else transform.position += velocity * Time.fixedDeltaTime;

            if (sprite != null && Mathf.Abs(velocity.x) > 0.05f) sprite.flipX = velocity.x < 0f;
        }

        Vector3 Direction(Vector3 to)
        {
            Vector3 d = to - transform.position;
            if (rb3 != null) d.y = 0f; // 3D chasers stay on the ground
            return d.sqrMagnitude > 0.0001f ? d.normalized : Vector3.zero;
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(1f, 0.6f, 0.1f, 0.8f);
            Gizmos.DrawWireSphere(transform.position, detectRange);
            Gizmos.color = new Color(1f, 0.6f, 0.1f, 0.3f);
            Gizmos.DrawWireSphere(transform.position, loseRange);
        }
    }
}
