using UnityEngine;

namespace JamKit
{
    public enum SpriteSwapperMode
    {
        Auto,
        Platformer,
        TopDown,
        Loop
    }

    [JamHelp("Animation for artists, no Animator needed: drag your frames into the lists below (Idle, Run, Jump…). Put it on the 'Visual' child next to the SpriteRenderer.")]
    [RequireComponent(typeof(SpriteRenderer))]
    [DisallowMultipleComponent]
    public class JamSpriteSwapper : JamBehaviour
    {
        [Tooltip("Auto picks Platformer or Top-Down from the player above it; anything else just loops 'Idle'.")]
        public SpriteSwapperMode mode = SpriteSwapperMode.Auto;
        [Tooltip("Frames per second.")]
        [Min(1)] public float fps = 10f;

        [Header("Platformer (and Loop mode uses Idle)")]
        [Tooltip("Frames while standing still. In Loop mode these frames just loop.")]
        public Sprite[] idle;
        [Tooltip("Frames while running.")]
        public Sprite[] run;
        [Tooltip("Frames while going up.")]
        public Sprite[] jump;
        [Tooltip("Frames while falling.")]
        public Sprite[] fall;
        [Tooltip("Frames while hurt / blinking (optional).")]
        public Sprite[] hurt;

        [Header("Top-Down (first frame of each = standing still)")]
        [Tooltip("Walking toward the camera / down the screen.")]
        public Sprite[] walkDown;
        [Tooltip("Walking away / up the screen.")]
        public Sprite[] walkUp;
        [Tooltip("Walking sideways (drawn facing right; flipped automatically for left).")]
        public Sprite[] walkSide;

        SpriteRenderer sr;
        JamPlayer player;
        Rigidbody2D body;
        Sprite[] currentSet;
        float timer;
        int frame;

        void Awake()
        {
            sr = GetComponent<SpriteRenderer>();
            player = GetComponentInParent<JamPlayer>();
            body = GetComponentInParent<Rigidbody2D>();
            if (mode == SpriteSwapperMode.Auto)
            {
                if (player is JamPlayerPlatformer2D) mode = SpriteSwapperMode.Platformer;
                else if (player is JamPlayerTopDown2D) mode = SpriteSwapperMode.TopDown;
                else mode = SpriteSwapperMode.Loop;
            }
        }

        void Update()
        {
            Sprite[] set = PickSet(out bool idleFrameOnly);
            if (set == null || set.Length == 0) return;
            if (set != currentSet)
            {
                currentSet = set;
                frame = 0;
                timer = 0f;
            }
            if (idleFrameOnly)
            {
                sr.sprite = set[0];
                return;
            }
            timer += Time.deltaTime;
            if (timer >= 1f / fps)
            {
                timer -= 1f / fps;
                frame = (frame + 1) % set.Length;
            }
            sr.sprite = set[Mathf.Clamp(frame, 0, set.Length - 1)];
        }

        Sprite[] PickSet(out bool idleFrameOnly)
        {
            idleFrameOnly = false;
            Vector2 v = player != null ? (Vector2)player.Velocity : body != null ? body.linearVelocity : Vector2.zero;
            bool hurting = player != null && player.Health.IsInvincible && !player.Health.IsDead;

            switch (mode)
            {
                case SpriteSwapperMode.Platformer:
                    if (hurting && Has(hurt)) return hurt;
                    bool grounded = player == null || player.IsGrounded;
                    if (!grounded) return v.y > 0.1f ? First(jump, fall, idle) : First(fall, jump, idle);
                    return Mathf.Abs(v.x) > 0.5f ? First(run, idle) : idle;

                case SpriteSwapperMode.TopDown:
                    Vector2 f = player != null ? player.Facing : v;
                    Sprite[] set = Mathf.Abs(f.x) > Mathf.Abs(f.y) ? First(walkSide, walkDown) : f.y > 0f ? First(walkUp, walkDown) : First(walkDown, walkSide);
                    idleFrameOnly = v.magnitude < 0.3f;
                    return Has(set) ? set : idle;

                default:
                    return idle;
            }
        }

        static bool Has(Sprite[] s) => s != null && s.Length > 0;

        static Sprite[] First(params Sprite[][] options)
        {
            foreach (var o in options) if (Has(o)) return o;
            return null;
        }
    }
}
