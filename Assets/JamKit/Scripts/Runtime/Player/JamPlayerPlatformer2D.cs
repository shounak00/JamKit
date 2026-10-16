using UnityEngine;

namespace JamKit
{
    [JamHelp("Side-view run & jump player. All the feel (speed, jump height, gravity, double jump, wall jump) lives in the PlayerConfig asset. Swap the art on the 'Visual' child.")]
    [RequireComponent(typeof(Rigidbody2D), typeof(CapsuleCollider2D))]
    [DisallowMultipleComponent]
    public class JamPlayerPlatformer2D : JamPlayer
    {
        [Tooltip("The child that holds the art (SpriteRenderer / Animator). Found automatically if it is called 'Visual'.")]
        public Transform visual;
        [Tooltip("Seconds between footstep events while running on the ground.")]
        public float footstepInterval = 0.3f;

        protected override PlayerConfigKind ConfigKind => PlayerConfigKind.Platformer2D;

        public override Vector3 Velocity => rb != null ? (Vector3)rb.linearVelocity : Vector3.zero;
        public override bool IsGrounded => grounded;
        public override Vector2 Facing => new Vector2(facing, 0f);

        Rigidbody2D rb;
        CapsuleCollider2D col;
        SpriteRenderer sprite;
        Animator animator;

        readonly ContactPoint2D[] contacts = new ContactPoint2D[16];
        bool grounded;
        bool wasGrounded;
        int wallDir;              // -1 wall on the left, +1 on the right, 0 none
        JamMover groundMover;
        float lastGroundedTime = -10f;
        float lastJumpPressedTime = -10f;
        int jumpsUsed;
        bool jumpCutDone = true;
        float facing = 1f;
        float moveX;
        bool jumpHeld;
        float knockbackUntil;
        float dashUntil = -10f;
        float nextDashTime;
        float footstepTimer;

        protected override void Awake()
        {
            base.Awake();
            rb = GetComponent<Rigidbody2D>();
            col = GetComponent<CapsuleCollider2D>();
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            rb.freezeRotation = true;
            if (col.sharedMaterial == null)
                col.sharedMaterial = new PhysicsMaterial2D("JamPlayerNoFriction") { friction = 0f, bounciness = 0f };
            if (visual == null) visual = FindChildNamed("Visual");
            if (visual != null)
            {
                sprite = visual.GetComponent<SpriteRenderer>();
                animator = visual.GetComponent<Animator>();
            }
        }

        protected override void Update()
        {
            base.Update();
            if (InputEnabled)
            {
                moveX = JamInput.Move.x;
                bool pressed = JamInput.JumpPressed || (Cfg.jumpWithUp && JamInput.UpPressed);
                jumpHeld = JamInput.JumpHeld || (Cfg.jumpWithUp && JamInput.UpHeld);
                if (pressed) lastJumpPressedTime = Time.time;
                if (Cfg.canDash && JamInput.DashPressed && Time.time >= nextDashTime) StartDash();
            }
            else
            {
                moveX = 0f;
                jumpHeld = false;
            }

            if (Mathf.Abs(moveX) > 0.1f && Time.time >= knockbackUntil) facing = Mathf.Sign(moveX);
            UpdateVisual();
            UpdateFootsteps();
        }

        void FixedUpdate()
        {
            ReadContacts();
            var c = Cfg;
            Vector2 v = rb.linearVelocity;
            Vector2 platformVel = groundMover != null ? groundMover.Velocity2D : Vector2.zero;

            if (grounded)
            {
                lastGroundedTime = Time.time;
                if (v.y <= platformVel.y + 0.01f)
                {
                    jumpsUsed = 0;
                    jumpCutDone = true;
                }
            }
            else if (jumpsUsed == 0 && Time.time - lastGroundedTime > c.coyoteTime)
            {
                jumpsUsed = 1; // walked off a ledge: the first air jump counts as a double jump
            }
            if (grounded && !wasGrounded) OnLand.Invoke();
            wasGrounded = grounded;

            bool dashing = Time.time < dashUntil;
            if (dashing)
            {
                v = new Vector2(facing * c.dashSpeed, 0f);
                rb.gravityScale = 0f;
                rb.linearVelocity = v;
                return;
            }

            // Horizontal
            if (Time.time >= knockbackUntil)
            {
                float target = moveX * c.moveSpeed;
                float accel = Mathf.Abs(target) > 0.01f ? c.acceleration : c.deceleration;
                if (!grounded) accel *= c.airControl;
                float relative = v.x - platformVel.x;
                relative = Mathf.MoveTowards(relative, target, accel * Time.fixedDeltaTime);
                v.x = relative + platformVel.x;
            }

            // Jump
            float g = Mathf.Abs(Physics2D.gravity.y) * c.gravityScale;
            float jumpVelocity = Mathf.Sqrt(2f * g * Mathf.Max(0.01f, c.jumpHeight));
            bool buffered = Time.time - lastJumpPressedTime <= c.jumpBuffer;
            if (buffered && c.maxJumps > 0)
            {
                bool canGroundJump = (grounded || Time.time - lastGroundedTime <= c.coyoteTime) && jumpsUsed == 0;
                if (canGroundJump)
                {
                    v.y = jumpVelocity + Mathf.Max(0f, platformVel.y);
                    DoJump();
                }
                else if (!grounded && wallDir != 0 && c.wallJump)
                {
                    v = new Vector2(-wallDir * c.wallJumpVelocity.x, c.wallJumpVelocity.y);
                    facing = -wallDir;
                    knockbackUntil = Time.time + 0.15f; // keep the push for a moment
                    jumpsUsed = Mathf.Max(jumpsUsed, 1);
                    DoJump(countJump: false);
                }
                else if (jumpsUsed < c.maxJumps)
                {
                    v.y = jumpVelocity;
                    DoJump();
                }
            }

            // Variable jump height
            if (!jumpHeld && !jumpCutDone && v.y > 0f)
            {
                v.y *= c.jumpCutMultiplier;
                jumpCutDone = true;
            }

            // Gravity & fall speed
            rb.gravityScale = v.y < 0f ? c.gravityScale * c.fallGravityMultiplier : c.gravityScale;
            if (wallDir != 0 && !grounded && c.wallSlide && Mathf.Sign(moveX) == wallDir && Mathf.Abs(moveX) > 0.1f)
                v.y = Mathf.Max(v.y, -c.wallSlideSpeed);
            v.y = Mathf.Max(v.y, -c.maxFallSpeed);

            // Stick to platforms moving down
            if (grounded && groundMover != null && platformVel.y < 0f && Time.time - lastJumpPressedTime > 0.1f)
                v.y = Mathf.Min(v.y, platformVel.y);

            rb.linearVelocity = v;
        }

        void DoJump(bool countJump = true)
        {
            if (countJump) jumpsUsed++;
            lastJumpPressedTime = -10f;
            lastGroundedTime = -10f;
            jumpCutDone = false;
            grounded = false;
            OnJump.Invoke();
            if (animator != null && animator.runtimeAnimatorController != null) SafeTrigger("Jump");
        }

        void StartDash()
        {
            dashUntil = Time.time + Cfg.dashDuration;
            nextDashTime = Time.time + Cfg.dashCooldown;
            if (Cfg.dashInvincible) Health.SetInvincible(Cfg.dashDuration);
            OnDash.Invoke();
        }

        void ReadContacts()
        {
            grounded = false;
            wallDir = 0;
            groundMover = null;
            var filter = ContactFilter2D.noFilter;
            filter.useTriggers = false;
            int n = rb.GetContacts(filter, contacts);
            for (int i = 0; i < n; i++)
            {
                var cp = contacts[i];
                if (cp.collider == null || cp.collider.isTrigger) continue;
                Vector2 normal = cp.normal;
                if (normal.y > 0.65f)
                {
                    grounded = true;
                    var mover = cp.collider.GetComponentInParent<JamMover>();
                    if (mover != null) groundMover = mover;
                }
                else if (Mathf.Abs(normal.x) > 0.9f)
                {
                    wallDir = normal.x > 0f ? -1 : 1;
                }
            }
        }

        void UpdateVisual()
        {
            if (sprite != null) sprite.flipX = facing < 0f;
            else if (visual != null)
            {
                Vector3 s = visual.localScale;
                s.x = Mathf.Abs(s.x) * (facing < 0f ? -1f : 1f);
                visual.localScale = s;
            }

            if (animator != null && animator.runtimeAnimatorController != null)
            {
                SafeFloat("Speed", Mathf.Abs(rb.linearVelocity.x));
                SafeBool("Grounded", grounded);
                SafeFloat("VelocityY", rb.linearVelocity.y);
            }
        }

        void UpdateFootsteps()
        {
            if (!grounded || Mathf.Abs(rb.linearVelocity.x) < 1f) { footstepTimer = 0f; return; }
            footstepTimer += Time.deltaTime;
            if (footstepTimer >= footstepInterval)
            {
                footstepTimer = 0f;
                OnFootstep.Invoke();
            }
        }

        public override void Bounce(float strength)
        {
            Vector2 v = rb.linearVelocity;
            v.y = strength;
            rb.linearVelocity = v;
            jumpsUsed = Mathf.Max(1, jumpsUsed);
            jumpCutDone = true;
            grounded = false;
        }

        public override void ApplyKnockback(Vector3 direction, float force)
        {
            float side = Mathf.Abs(direction.x) > 0.01f ? Mathf.Sign(direction.x) : -facing;
            rb.linearVelocity = new Vector2(side * force, force * 0.6f);
            knockbackUntil = Time.time + 0.2f;
            jumpCutDone = true;
            if (animator != null && animator.runtimeAnimatorController != null) SafeTrigger("Hurt");
        }

        public override void TeleportTo(Vector3 position)
        {
            if (rb == null) rb = GetComponent<Rigidbody2D>();
            rb.position = position;
            transform.position = position;
            rb.linearVelocity = Vector2.zero;
        }

        // Animator parameters are optional: only set the ones that exist.
        void SafeFloat(string p, float v) { if (HasParam(p)) animator.SetFloat(p, v); }
        void SafeBool(string p, bool v) { if (HasParam(p)) animator.SetBool(p, v); }
        void SafeTrigger(string p) { if (HasParam(p)) animator.SetTrigger(p); }
        System.Collections.Generic.HashSet<string> paramNames;
        bool HasParam(string p)
        {
            if (paramNames == null)
            {
                paramNames = new System.Collections.Generic.HashSet<string>();
                foreach (var param in animator.parameters) paramNames.Add(param.name);
            }
            return paramNames.Contains(p);
        }
    }
}
