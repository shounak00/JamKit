using UnityEngine;
using UnityEngine.Events;

namespace JamKit
{
    [JamHelp("Touch it and you respawn here after falling or losing a life. Turn on 'Use Lives' on the GameManager for lives mode.")]
    public class JamCheckpoint : JamTriggerBehaviour
    {
        [Tooltip("Sprite shown once activated (2D). Optional.")]
        public Sprite activatedSprite;
        [Tooltip("Tint applied to the Visual once activated (2D sprite or 3D mesh).")]
        public Color activatedColor = new Color(0.3f, 1f, 0.4f);
        [Tooltip("Where the player reappears, relative to this object.")]
        public Vector3 respawnOffset = new Vector3(0f, 0.2f, 0f);
        [Tooltip("Optional sound when activated.")]
        public AudioClip activateSound;
        [Tooltip("Heal the player to full when activated.")]
        public bool healOnActivate = false;

        [Header("Events")]
        public UnityEvent OnActivated = new UnityEvent();

        bool activated;

        protected override void HandleEnter(GameObject other)
        {
            var player = PlayerOf(other);
            if (player == null) return;
            player.SetCheckpoint(transform.position + respawnOffset);
            if (healOnActivate) player.Health.Heal(player.Health.maxHealth);
            if (activated) return;
            activated = true;

            var visual = transform.Find("Visual");
            if (visual != null)
            {
                var sr = visual.GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    if (activatedSprite != null) sr.sprite = activatedSprite;
                    sr.color = activatedColor;
                }
                foreach (var r in visual.GetComponentsInChildren<MeshRenderer>())
                    if (r.material.HasProperty("_BaseColor")) r.material.SetColor("_BaseColor", activatedColor);
            }
            JamAudio.Play(activateSound);
            OnActivated.Invoke();
        }

        void OnDrawGizmos()
        {
            Gizmos.color = new Color(0.3f, 0.8f, 1f, 0.7f);
            JamGizmos.DrawColliderOutline(gameObject);
        }
    }
}
