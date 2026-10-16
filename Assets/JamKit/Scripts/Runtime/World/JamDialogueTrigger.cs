using UnityEngine;
using UnityEngine.Events;

namespace JamKit
{
    [JamHelp("Signposts, tutorials and story: shows a text box with your lines when the player walks in (or presses E). Space / E advances.")]
    public class JamDialogueTrigger : JamTriggerBehaviour
    {
        [Tooltip("Name shown above the text (leave empty for a sign).")]
        public string speakerName = "";
        [Tooltip("The lines, shown one at a time.")]
        [TextArea(2, 4)] public string[] lines = { "Use A/D or the arrows to move.", "Press Space to jump." };
        [Tooltip("Freeze the game while the text is open.")]
        public bool pauseGame = true;
        [Tooltip("Only show the first time.")]
        public bool triggerOnce = true;
        [Tooltip("Wait for the Interact key (E) instead of opening on enter.")]
        public bool requireInteract = false;

        [Header("Events")]
        public UnityEvent OnFinished = new UnityEvent();

        bool shown;
        bool playerInside;

        protected override void HandleEnter(GameObject other)
        {
            if (PlayerOf(other) == null) return;
            playerInside = true;
            if (requireInteract) JamMessageUI.Toast("Press E", 1.2f);
            else Show();
        }

        protected override void HandleExit(GameObject other)
        {
            if (PlayerOf(other) != null) playerInside = false;
        }

        protected override void Update()
        {
            base.Update();
            if (requireInteract && playerInside && !JamMessageUI.DialogueOpen && JamInput.InteractPressed) Show();
        }

        public void Show()
        {
            if (triggerOnce && shown) return;
            if (JamMessageUI.DialogueOpen) return;
            shown = true;
            JamMessageUI.ShowDialogue(speakerName, lines, pauseGame, () => OnFinished.Invoke());
        }

        void OnDrawGizmos()
        {
            Gizmos.color = new Color(1f, 1f, 1f, 0.6f);
            JamGizmos.DrawColliderOutline(gameObject);
        }
    }
}
