using UnityEngine;

namespace JamKit
{
    public enum PlayerConfigKind
    {
        Platformer2D,
        TopDown2D,
        ThirdPerson3D
    }

    public enum TopDownFacing
    {
        FourWay,
        EightWay,
        RotateTowardMovement
    }

    /// <summary>
    /// All the "feel" numbers for a player controller, saved as an asset so you can swap presets
    /// (Floaty / Snappy / Heavy …) or tweak them while playing. Changes made in Play mode are kept.
    /// Create your own: right-click in the Project window → Create → JamKit → Player Config.
    /// </summary>
    [CreateAssetMenu(menuName = "JamKit/Player Config", fileName = "PlayerConfig_New", order = 0)]
    public class PlayerConfig : ScriptableObject
    {
        [Tooltip("Which controller this config is for. The Inspector only shows the settings that matter for it.")]
        public PlayerConfigKind kind = PlayerConfigKind.Platformer2D;

        // ------------------------------------------------------------------ Shared
        [Tooltip("Top speed in units per second (2D) or metres per second when walking (3D).")]
        public float moveSpeed = 8f;
        [Tooltip("How fast you reach top speed. Higher = snappier.")]
        public float acceleration = 70f;
        [Tooltip("How fast you stop when you let go. Higher = less sliding.")]
        public float deceleration = 80f;
        [Tooltip("How much control you have in the air (0 = none, 1 = same as on the ground).")]
        [Range(0f, 1f)] public float airControl = 0.65f;
        [Tooltip("How high a full jump goes, in units/metres. Easier to reason about than a force.")]
        public float jumpHeight = 3.2f;
        [Tooltip("Total jumps before landing. 1 = normal, 2 = double jump, 0 = no jumping.")]
        [Min(0)] public int maxJumps = 1;
        [Tooltip("Grace time (seconds) to still jump just after walking off a ledge.")]
        public float coyoteTime = 0.1f;
        [Tooltip("If you press jump this many seconds before landing, the jump still happens.")]
        public float jumpBuffer = 0.12f;
        [Tooltip("Allow a quick dash on Left Shift / gamepad West button (2D controllers).")]
        public bool canDash = false;
        [Tooltip("Dash speed in units per second.")]
        public float dashSpeed = 18f;
        [Tooltip("How long a dash lasts (seconds).")]
        public float dashDuration = 0.15f;
        [Tooltip("Seconds before you can dash again.")]
        public float dashCooldown = 0.6f;
        [Tooltip("Can't be hurt while dashing.")]
        public bool dashInvincible = true;
        [Tooltip("Seconds your controls are locked after being hit.")]
        public float hurtControlLock = 0.2f;

        // ------------------------------------------------------------------ Platformer 2D
        [Tooltip("Gravity multiplier while rising. Higher = faster, punchier jumps.")]
        public float gravityScale = 3.5f;
        [Tooltip("Extra gravity while falling. Above 1 makes jumps feel less floaty.")]
        public float fallGravityMultiplier = 1.6f;
        [Tooltip("Maximum falling speed.")]
        public float maxFallSpeed = 20f;
        [Tooltip("Letting go of jump early multiplies upward speed by this (short hop). 1 = no variable height.")]
        [Range(0.1f, 1f)] public float jumpCutMultiplier = 0.5f;
        [Tooltip("W and Up arrow also jump (on top of Space).")]
        public bool jumpWithUp = true;
        [Tooltip("Slide slowly down walls while pushing into them.")]
        public bool wallSlide = false;
        [Tooltip("Speed when sliding down a wall.")]
        public float wallSlideSpeed = 3f;
        [Tooltip("Jump off walls while sliding.")]
        public bool wallJump = false;
        [Tooltip("Wall-jump push: X = away from wall, Y = up.")]
        public Vector2 wallJumpVelocity = new Vector2(9f, 14f);

        // ------------------------------------------------------------------ Top-down 2D
        [Tooltip("How the character faces: 4 directions, 8 directions, or rotate the Visual toward movement.")]
        public TopDownFacing facing = TopDownFacing.FourWay;

        // ------------------------------------------------------------------ Third person 3D
        [Tooltip("Speed while holding Sprint (Left Shift). 'Move Speed' is the walking speed.")]
        public float runSpeed = 6.5f;
        [Tooltip("Seconds to turn toward the movement direction. Lower = snappier turning.")]
        public float turnSmoothTime = 0.1f;
        [Tooltip("Gravity in m/s² (negative). Earth is -9.81; games usually use more.")]
        public float gravity = -25f;
        [Tooltip("Fastest the character can fall.")]
        public float terminalVelocity = 30f;
        [Tooltip("Camera mouse / right-stick sensitivity.")]
        public float lookSensitivity = 1f;
        [Tooltip("Flip the camera's up/down look.")]
        public bool invertLookY = false;

        /// <summary>Default values for a kind, used by presets and by "Reset".</summary>
        public void ApplyDefaults(PlayerConfigKind k)
        {
            kind = k;
            switch (k)
            {
                case PlayerConfigKind.Platformer2D:
                    moveSpeed = 8f; acceleration = 70f; deceleration = 80f; airControl = 0.65f;
                    jumpHeight = 3.2f; maxJumps = 1; coyoteTime = 0.1f; jumpBuffer = 0.12f;
                    canDash = false; gravityScale = 3.5f; fallGravityMultiplier = 1.6f; maxFallSpeed = 20f;
                    jumpCutMultiplier = 0.5f; jumpWithUp = true;
                    break;
                case PlayerConfigKind.TopDown2D:
                    moveSpeed = 6f; acceleration = 60f; deceleration = 70f; airControl = 1f;
                    maxJumps = 0; canDash = true; dashSpeed = 18f; dashDuration = 0.15f; dashCooldown = 0.6f;
                    facing = TopDownFacing.FourWay;
                    break;
                case PlayerConfigKind.ThirdPerson3D:
                    moveSpeed = 3f; runSpeed = 6.5f; acceleration = 20f; deceleration = 25f; airControl = 0.6f;
                    jumpHeight = 1.4f; maxJumps = 1; coyoteTime = 0.12f; jumpBuffer = 0.15f;
                    turnSmoothTime = 0.1f; gravity = -25f; terminalVelocity = 30f; lookSensitivity = 1f;
                    break;
            }
        }

        void Reset() => ApplyDefaults(kind);
    }
}
