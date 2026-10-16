using UnityEngine;

namespace JamKit
{
    [JamHelp("Top-down player: moves in 8 directions with an optional dash. Pairs with JamShooter (aims where you face) and JamChaser enemies. Feel lives in the PlayerConfig asset.")]
    [RequireComponent(typeof(Rigidbody2D), typeof(CircleCollider2D))]
    [DisallowMultipleComponent]
    public class JamPlayerTopDown2D : JamPlayer
    {
        [Tooltip("The child that holds the art (SpriteRenderer / Animator). Found automatically if it is called 'Visual'.")]
        public Transform visual;
        [Tooltip("Seconds between footstep events while moving.")]
        public float footstepInterval = 0.35f;

        protected override PlayerConfigKind ConfigKind => PlayerConfigKind.TopDown2D;

        public override Vector3 Velocity => rb != null ? (Vector3)rb.linearVelocity : Vector3.zero;
        public override bool IsGrounded => true;
        public override Vector2 Facing => facing;

        Rigidbody2D rb;
        SpriteRenderer sprite;
        JamAnimatorParams anim;
        Vector2 move;
        Vector2 facing = Vector2.down;
        Vector2 dashDir;
        float dashUntil = -10f;
        float nextDashTime;
        float knockbackUntil;
        float footstepTimer;

        protected override void Awake()
        {
            base.Awake();
            rb = GetComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.gravityScale = 0f;
            rb.freezeRotation = true;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            if (visual == null) visual = FindChildNamed("Visual");
            if (visual != null)
            {
                sprite = visual.GetComponent<SpriteRenderer>();
                anim = new JamAnimatorParams(visual.GetComponent<Animator>());
            }
        }

        protected override void Update()
        {
            base.Update();
            move = InputEnabled ? JamInput.Move : Vector2.zero;
            if (move.sqrMagnitude > 0.01f) facing = Quantize(move.normalized);
            if (InputEnabled && Cfg.canDash && JamInput.DashPressed && Time.time >= nextDashTime) StartDash();
            UpdateVisual();
            UpdateFootsteps();
        }

        void FixedUpdate()
        {
            var c = Cfg;
            if (Time.time < dashUntil)
            {
                rb.linearVelocity = dashDir * c.dashSpeed;
                return;
            }
            if (Time.time < knockbackUntil) return;
            Vector2 target = move * c.moveSpeed;
            float accel = target.sqrMagnitude > 0.01f ? c.acceleration : c.deceleration;
            rb.linearVelocity = Vector2.MoveTowards(rb.linearVelocity, target, accel * Time.fixedDeltaTime);
        }

        void StartDash()
        {
            dashDir = move.sqrMagnitude > 0.01f ? move.normalized : facing.normalized;
            dashUntil = Time.time + Cfg.dashDuration;
            nextDashTime = Time.time + Cfg.dashCooldown;
            if (Cfg.dashInvincible) Health.SetInvincible(Cfg.dashDuration);
            OnDash.Invoke();
        }

        Vector2 Quantize(Vector2 dir)
        {
            switch (Cfg.facing)
            {
                case TopDownFacing.FourWay:
                    return Mathf.Abs(dir.x) >= Mathf.Abs(dir.y) ? new Vector2(Mathf.Sign(dir.x), 0f) : new Vector2(0f, Mathf.Sign(dir.y));
                case TopDownFacing.EightWay:
                    float angle = Mathf.Round(Mathf.Atan2(dir.y, dir.x) / (Mathf.PI / 4f)) * (Mathf.PI / 4f);
                    return new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                default:
                    return dir;
            }
        }

        void UpdateVisual()
        {
            if (visual == null) return;
            if (Cfg.facing == TopDownFacing.RotateTowardMovement)
            {
                float a = Mathf.Atan2(facing.y, facing.x) * Mathf.Rad2Deg - 90f; // sprite art faces up
                visual.rotation = Quaternion.Euler(0f, 0f, a);
            }
            else if (sprite != null && Mathf.Abs(facing.x) > 0.01f)
            {
                sprite.flipX = facing.x < 0f;
            }
            if (anim != null)
            {
                anim.Float("MoveX", facing.x);
                anim.Float("MoveY", facing.y);
                anim.Float("Speed", rb.linearVelocity.magnitude);
            }
        }

        void UpdateFootsteps()
        {
            if (rb.linearVelocity.magnitude < 1f) { footstepTimer = 0f; return; }
            footstepTimer += Time.deltaTime;
            if (footstepTimer >= footstepInterval)
            {
                footstepTimer = 0f;
                OnFootstep.Invoke();
            }
        }

        public override void Bounce(float strength) { }

        public override void ApplyKnockback(Vector3 direction, float force)
        {
            rb.linearVelocity = (Vector2)direction.normalized * force;
            knockbackUntil = Time.time + 0.15f;
            if (anim != null) anim.Trigger("Hurt");
        }

        public override void TeleportTo(Vector3 position)
        {
            if (rb == null) rb = GetComponent<Rigidbody2D>();
            rb.position = position;
            transform.position = position;
            rb.linearVelocity = Vector2.zero;
        }
    }
}
