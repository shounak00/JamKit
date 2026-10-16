using UnityEngine;

namespace JamKit
{
    /// <summary>
    /// Sits next to the Animator (on the Model) and forwards animation events to the player.
    /// Add an animation event named OnFootstep (or Footstep / FootL / FootR) to a walk or run clip.
    /// </summary>
    public class JamAnimationEventRelay : MonoBehaviour
    {
        JamPlayerThirdPerson3D player;

        void Awake() => player = GetComponentInParent<JamPlayerThirdPerson3D>();

        public void OnFootstep() { if (player != null) player.AnimationFootstep(); }
        public void Footstep() => OnFootstep();
        public void FootL() => OnFootstep();
        public void FootR() => OnFootstep();
    }
}
