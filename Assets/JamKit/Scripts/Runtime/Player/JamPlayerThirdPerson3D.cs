using UnityEngine;

namespace JamKit
{
    [JamHelp("Third-person 3D player (camera-relative: W walks away from the camera). Hold Shift to run. Swap the character on the 'Model' child; feel lives in the PlayerConfig asset.")]
    [RequireComponent(typeof(CharacterController))]
    [DisallowMultipleComponent]
    public class JamPlayerThirdPerson3D : JamPlayer
    {
        [Tooltip("The child holding the character model and its Animator. Found automatically if it is called 'Model'.")]
        public Transform model;
        [Tooltip("Point the camera orbits around (shoulder height). Found automatically if it is called 'CameraTarget'.")]
        public Transform cameraTarget;
        [Tooltip("Camera used for camera-relative movement. Empty = Main Camera.")]
        public Camera viewCamera;
        [Tooltip("Metres travelled between footstep events while walking (running uses a longer stride). Animation events named OnFootstep also work.")]
        public float footstepDistance = 1.6f;

        protected override PlayerConfigKind ConfigKind => PlayerConfigKind.ThirdPerson3D;

        public override Vector3 Velocity => horizontalVelocity + knockbackVelocity + platformVelocity + Vector3.up * verticalVelocity;
        public override bool IsGrounded => grounded;
        public override Vector2 Facing => new Vector2(transform.forward.x, transform.forward.z);

        /// <summary>0 idle → 1 walk → 2 run. Read by JamHumanoidAnimator.</summary>
        public float SpeedParam { get; private set; }
        public float VerticalVelocity => verticalVelocity;
        public CharacterController Controller => cc;

        CharacterController cc;
        Vector3 horizontalVelocity;
        Vector3 knockbackVelocity;
        Vector3 platformVelocity;
        float verticalVelocity;
        bool grounded;
        float lastGroundedTime = -10f;
        float lastJumpPressedTime = -10f;
        int jumpsUsed;
        float turnVelocity;
        float stepDistance;
        bool usingAnimationFootsteps;

        protected override void Awake()
        {
            base.Awake();
            cc = GetComponent<CharacterController>();
            if (model == null) model = FindChildNamed("Model");
            if (cameraTarget == null) cameraTarget = FindChildNamed("CameraTarget");
        }

        protected override void Update()
        {
            base.Update();
            var c = Cfg;
            float dt = Time.deltaTime;
            if (dt <= 0f) return;

            if (viewCamera == null) viewCamera = Camera.main;
            Vector2 input = InputEnabled ? JamInput.Move : Vector2.zero;
            bool sprint = InputEnabled && JamInput.SprintHeld;
            if (InputEnabled && JamInput.JumpPressed) lastJumpPressedTime = Time.time;

            // Camera-relative direction
            Vector3 fwd = Vector3.forward, right = Vector3.right;
            if (viewCamera != null)
            {
                fwd = Vector3.ProjectOnPlane(viewCamera.transform.forward, Vector3.up).normalized;
                right = Vector3.ProjectOnPlane(viewCamera.transform.right, Vector3.up).normalized;
            }
            Vector3 wish = Vector3.ClampMagnitude(fwd * input.y + right * input.x, 1f);

            // Ground check
            bool wasGrounded = grounded;
            JamMover mover = null;
            grounded = CheckGround(out mover);
            platformVelocity = mover != null ? mover.Velocity : Vector3.zero;
            if (grounded)
            {
                lastGroundedTime = Time.time;
                if (verticalVelocity <= 0f) jumpsUsed = 0;
            }
            else if (jumpsUsed == 0 && Time.time - lastGroundedTime > c.coyoteTime)
            {
                jumpsUsed = 1;
            }
            if (grounded && !wasGrounded && verticalVelocity <= 0f) OnLand.Invoke();

            // Horizontal speed
            float topSpeed = sprint ? c.runSpeed : c.moveSpeed;
            Vector3 target = wish * topSpeed;
            float accel = target.sqrMagnitude > 0.01f ? c.acceleration : c.deceleration;
            if (!grounded) accel *= c.airControl;
            horizontalVelocity = Vector3.MoveTowards(horizontalVelocity, target, accel * dt);

            // Turn toward movement
            if (wish.sqrMagnitude > 0.01f)
            {
                float targetYaw = Mathf.Atan2(wish.x, wish.z) * Mathf.Rad2Deg;
                float yaw = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetYaw, ref turnVelocity, c.turnSmoothTime);
                transform.rotation = Quaternion.Euler(0f, yaw, 0f);
            }

            // Jump
            bool buffered = Time.time - lastJumpPressedTime <= c.jumpBuffer;
            if (buffered && c.maxJumps > 0)
            {
                bool canGroundJump = (grounded || Time.time - lastGroundedTime <= c.coyoteTime) && jumpsUsed == 0;
                if (canGroundJump || jumpsUsed < c.maxJumps && !grounded && jumpsUsed > 0)
                {
                    verticalVelocity = Mathf.Sqrt(2f * Mathf.Abs(c.gravity) * Mathf.Max(0.01f, c.jumpHeight));
                    jumpsUsed++;
                    lastJumpPressedTime = -10f;
                    lastGroundedTime = -10f;
                    grounded = false;
                    OnJump.Invoke();
                }
            }

            // Gravity
            if (grounded && verticalVelocity < 0f) verticalVelocity = -2f; // keep the controller glued to slopes
            else verticalVelocity = Mathf.Max(verticalVelocity + c.gravity * dt, -c.terminalVelocity);

            // Knockback decays over ~0.2 s
            knockbackVelocity = Vector3.MoveTowards(knockbackVelocity, Vector3.zero, knockbackVelocity.magnitude * dt / 0.2f + 0.01f);

            Vector3 motion = (horizontalVelocity + knockbackVelocity + platformVelocity) * dt + Vector3.up * verticalVelocity * dt;
            if (cc.enabled) cc.Move(motion);
            if ((cc.collisionFlags & CollisionFlags.Above) != 0 && verticalVelocity > 0f) verticalVelocity = 0f;

            // Animation speed 0..1 walk, 1..2 run
            float speed = new Vector3(horizontalVelocity.x, 0f, horizontalVelocity.z).magnitude;
            SpeedParam = speed <= c.moveSpeed
                ? (c.moveSpeed > 0f ? speed / c.moveSpeed : 0f)
                : 1f + Mathf.InverseLerp(c.moveSpeed, c.runSpeed, speed);

            UpdateFootsteps(speed, dt);
        }

        bool CheckGround(out JamMover mover)
        {
            mover = null;
            float radius = cc.radius * 0.9f;
            Vector3 origin = transform.position + cc.center + Vector3.down * (cc.height * 0.5f - cc.radius) + Vector3.up * 0.05f;
            bool hit = JamPhysics.SphereCast3D(origin, radius, Vector3.down, cc.skinWidth + 0.15f, transform, out RaycastHit info);
            if (hit) mover = info.collider.GetComponentInParent<JamMover>();
            if (verticalVelocity > 0.1f) return false; // going up: not grounded
            return cc.isGrounded || hit;
        }

        void UpdateFootsteps(float speed, float dt)
        {
            if (usingAnimationFootsteps || !grounded || speed < 0.5f || footstepDistance <= 0f) return;
            stepDistance += speed * dt;
            float stride = speed > Cfg.moveSpeed + 0.1f ? footstepDistance * 1.4f : footstepDistance;
            if (stepDistance >= stride)
            {
                stepDistance = 0f;
                OnFootstep.Invoke();
            }
        }

        /// <summary>Called by JamAnimationEventRelay when a clip has footstep events (turns the distance timer off).</summary>
        public void AnimationFootstep()
        {
            usingAnimationFootsteps = true;
            if (grounded) OnFootstep.Invoke();
        }

        void OnControllerColliderHit(ControllerColliderHit hit)
        {
            var receiver = hit.collider.GetComponentInParent<JamTriggerBehaviour>();
            if (receiver != null) receiver.NotifyControllerTouch(gameObject);
        }

        public override void Bounce(float strength)
        {
            verticalVelocity = strength;
            jumpsUsed = Mathf.Max(1, jumpsUsed);
            grounded = false;
        }

        public override void ApplyKnockback(Vector3 direction, float force)
        {
            Vector3 flat = Vector3.ProjectOnPlane(direction, Vector3.up);
            if (flat.sqrMagnitude < 0.0001f) flat = -transform.forward;
            knockbackVelocity = flat.normalized * force;
            verticalVelocity = Mathf.Max(verticalVelocity, force * 0.4f);
            grounded = false;
            LockInput(0.2f);
        }

        public override void TeleportTo(Vector3 position)
        {
            if (cc == null) cc = GetComponent<CharacterController>();
            bool was = cc.enabled;
            cc.enabled = false;
            transform.position = position;
            cc.enabled = was;
            horizontalVelocity = knockbackVelocity = platformVelocity = Vector3.zero;
            verticalVelocity = 0f;
        }
    }
}
