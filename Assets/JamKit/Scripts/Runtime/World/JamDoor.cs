using UnityEngine;
using UnityEngine.Events;

namespace JamKit
{
    [JamHelp("A door or gate. Call Open / Close / Toggle from any event (e.g. a Trigger Zone that needs a key). When open its solid collider is switched off.")]
    public class JamDoor : JamTriggerBehaviour
    {
        [Tooltip("Sprite when open (2D). If empty, the Visual is hidden when open.")]
        public Sprite openSprite;
        [Tooltip("Sprite when closed (2D). Empty = keep the current sprite.")]
        public Sprite closedSprite;
        [Tooltip("Start the level with the door open.")]
        public bool startOpen = false;
        [Tooltip("Sound when it opens.")]
        public AudioClip openSound;
        [Tooltip("Sound when it closes.")]
        public AudioClip closeSound;

        [Header("Events")]
        public UnityEvent OnOpened = new UnityEvent();
        public UnityEvent OnClosed = new UnityEvent();

        public bool IsOpen { get; private set; }

        Transform visual;
        SpriteRenderer sprite;
        JamAnimatorParams anim;

        void Awake()
        {
            visual = transform.Find("Visual");
            if (visual != null)
            {
                sprite = visual.GetComponent<SpriteRenderer>();
                anim = new JamAnimatorParams(visual.GetComponent<Animator>());
            }
            Apply(startOpen, silent: true);
        }

        public void Open()
        {
            if (IsOpen) return;
            Apply(true, false);
        }

        public void Close()
        {
            if (!IsOpen) return;
            Apply(false, false);
        }

        public void Toggle() => Apply(!IsOpen, false);

        void Apply(bool open, bool silent)
        {
            IsOpen = open;
            foreach (var c in GetComponentsInChildren<Collider2D>()) if (!c.isTrigger) c.enabled = !open;
            foreach (var c in GetComponentsInChildren<Collider>()) if (!c.isTrigger) c.enabled = !open;

            if (anim != null && anim.Valid) anim.Bool("Open", open);
            else if (sprite != null)
            {
                if (open && openSprite != null) sprite.sprite = openSprite;
                else if (!open && closedSprite != null) sprite.sprite = closedSprite;
                sprite.enabled = !open || openSprite != null;
            }
            else if (visual != null)
            {
                foreach (var r in visual.GetComponentsInChildren<Renderer>()) r.enabled = !open;
            }

            if (silent) return;
            JamAudio.Play(open ? openSound : closeSound);
            (open ? OnOpened : OnClosed).Invoke();
        }

        void OnDrawGizmos()
        {
            Gizmos.color = new Color(0.9f, 0.6f, 0.2f, 0.8f);
            JamGizmos.DrawColliderOutline(gameObject);
        }
    }
}
