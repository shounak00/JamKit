using UnityEngine;

namespace JamKit
{
    [JamHelp("Feeds the 3D player's movement into the humanoid Animator (Speed, Grounded, VerticalVelocity, Jump, Hurt, Dead, Victory). Root motion stays off: the controller moves, the animation only shows.")]
    [RequireComponent(typeof(JamPlayerThirdPerson3D))]
    [DisallowMultipleComponent]
    public class JamHumanoidAnimator : JamBehaviour
    {
        [Tooltip("The Animator on the Model child. Found automatically.")]
        public Animator animator;
        [Tooltip("Smoothing for the Speed parameter so the blend between idle, walk and run is soft.")]
        public float speedDamping = 0.1f;
        [Tooltip("How fast the walk clip moves its feet, in metres per second. Setup Mixamo Character fills this in so the feet don't slide. 0 = play the clip at its normal speed.")]
        public float walkClipSpeed;
        [Tooltip("Same as Walk Clip Speed, for the run clip.")]
        public float runClipSpeed;

        static readonly int LocomotionState = Animator.StringToHash("Locomotion");

        JamPlayerThirdPerson3D player;
        JamAnimatorParams p;
        bool subscribedWin;

        void Awake()
        {
            player = GetComponent<JamPlayerThirdPerson3D>();
            if (animator == null)
            {
                Transform model = player.model != null ? player.model : transform;
                animator = model.GetComponentInChildren<Animator>();
            }
            p = new JamAnimatorParams(animator);
            if (animator != null)
            {
                animator.applyRootMotion = false;
                if (animator.GetComponent<JamAnimationEventRelay>() == null) animator.gameObject.AddComponent<JamAnimationEventRelay>();
            }

            player.OnJump.AddListener(() => p.Trigger("Jump"));
            player.OnHurt.AddListener(() => p.Trigger("Hurt"));
            player.OnDeath.AddListener(() => p.Bool("Dead", true));
        }

        void OnEnable() => JamGameManager.StateChanged += OnStateChanged;
        void OnDisable() => JamGameManager.StateChanged -= OnStateChanged;

        void OnStateChanged(JamGameState s)
        {
            if (s == JamGameState.Won) p.Trigger("Victory");
            // The end screen sets timeScale to 0: keep the victory / death animation playing behind it.
            if ((s == JamGameState.Won || s == JamGameState.Lost) && animator != null)
                animator.updateMode = AnimatorUpdateMode.UnscaledTime;
        }

        void Update()
        {
            if (!p.Valid) return;
            p.Float("Speed", player.SpeedParam, speedDamping);
            p.Bool("Grounded", player.IsGrounded);
            p.Float("VerticalVelocity", player.VerticalVelocity);
            if (!player.Health.IsDead) p.Bool("Dead", false);
            animator.speed = LocomotionPlaybackSpeed();
        }

        /// <summary>Plays walk / run faster or slower so the feet keep up with the real movement speed (Locomotion state only).</summary>
        float LocomotionPlaybackSpeed()
        {
            var c = player.config;
            if (c == null || walkClipSpeed <= 0f || animator.IsInTransition(0)
                || animator.GetCurrentAnimatorStateInfo(0).shortNameHash != LocomotionState) return 1f;
            float s = player.SpeedParam; // 0–1 walk, 1–2 run
            float clipSpeed = s <= 1f || runClipSpeed <= 0f ? walkClipSpeed : Mathf.Lerp(walkClipSpeed, runClipSpeed, s - 1f);
            float realSpeed = s <= 1f ? s * c.moveSpeed : Mathf.Lerp(c.moveSpeed, c.runSpeed, s - 1f);
            // Near standing still the idle clip dominates the blend, so fade the correction in with speed.
            return Mathf.Lerp(1f, Mathf.Clamp(realSpeed / clipSpeed, 0.5f, 2f), Mathf.Clamp01(s));
        }
    }
}
