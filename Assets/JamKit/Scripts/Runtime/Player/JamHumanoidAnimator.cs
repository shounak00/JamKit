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
        }

        void Update()
        {
            if (!p.Valid) return;
            p.Float("Speed", player.SpeedParam, speedDamping);
            p.Bool("Grounded", player.IsGrounded);
            p.Float("VerticalVelocity", player.VerticalVelocity);
            if (!player.Health.IsDead) p.Bool("Dead", false);
        }
    }
}
