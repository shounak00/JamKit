using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace JamKit
{
    [JamHelp("The universal glue. When the player walks in (or presses E inside), run the events below: open a door, start a platform, play a sound, spawn enemies. Can require an item, fire once, or wait.")]
    public class JamTriggerZone : JamTriggerBehaviour
    {
        [Tooltip("Only react to the player (ignore enemies, projectiles…).")]
        public bool onlyPlayer = true;
        [Tooltip("Fire OnEnter only the first time.")]
        public bool triggerOnce = false;
        [Tooltip("Wait for the Interact key (E / gamepad North) while inside, instead of firing on enter.")]
        public bool requireInteract = false;
        [Tooltip("Hint shown when the player is inside and must press Interact.")]
        public string interactPrompt = "Press E";

        [Header("Requirement (optional)")]
        [Tooltip("Item the player must have (e.g. 'key'). Leave empty for none.")]
        public string requiredItemId = "";
        [Tooltip("How many of that item.")]
        [Min(1)] public int requiredAmount = 1;
        [Tooltip("Use up the item when the zone fires (keys get used).")]
        public bool consumeItem = false;
        [Tooltip("Hint shown when the requirement isn't met. {need} and {item} are replaced.")]
        public string lockedMessage = "You need a {item}.";

        [Header("Timing")]
        [Tooltip("Seconds to wait before OnEnter fires.")]
        [Min(0)] public float delay = 0f;

        [Header("Events")]
        public UnityEvent OnEnter = new UnityEvent();
        public UnityEvent OnExit = new UnityEvent();

        bool fired;
        int inside;
        bool playerInside;

        protected override void HandleEnter(GameObject other)
        {
            if (!Accepts(other)) return;
            inside++;
            if (PlayerOf(other) != null)
            {
                playerInside = true;
                if (requireInteract && CanFire(false)) JamMessageUI.Toast(interactPrompt, 1.5f);
            }
            if (!requireInteract) TryFire();
        }

        protected override void HandleExit(GameObject other)
        {
            if (!Accepts(other)) return;
            inside = Mathf.Max(0, inside - 1);
            if (PlayerOf(other) != null) playerInside = false;
            if (inside == 0) OnExit.Invoke();
        }

        protected override void Update()
        {
            base.Update();
            if (requireInteract && playerInside && JamInput.InteractPressed && JamGameManager.IsPlaying) TryFire();
        }

        bool Accepts(GameObject other)
        {
            if (IsSelf(other)) return false;
            return !onlyPlayer || PlayerOf(other) != null;
        }

        bool CanFire(bool showMessage)
        {
            if (triggerOnce && fired) return false;
            if (!string.IsNullOrEmpty(requiredItemId) && !JamInventory.Has(requiredItemId, requiredAmount))
            {
                if (showMessage)
                    JamMessageUI.Toast(lockedMessage.Replace("{need}", requiredAmount.ToString()).Replace("{item}", requiredItemId));
                return false;
            }
            return true;
        }

        void TryFire()
        {
            if (!CanFire(true)) return;
            if (consumeItem && !string.IsNullOrEmpty(requiredItemId)) JamInventory.TryConsume(requiredItemId, requiredAmount);
            fired = true;
            if (delay > 0f) StartCoroutine(FireLater());
            else OnEnter.Invoke();
        }

        IEnumerator FireLater()
        {
            yield return new WaitForSeconds(delay);
            OnEnter.Invoke();
        }

        /// <summary>Fire the zone's OnEnter from another event (ignores the requirement).</summary>
        public void ForceFire() => OnEnter.Invoke();

        void OnDrawGizmos()
        {
            Gizmos.color = new Color(0.7f, 0.3f, 1f, 0.8f);
            JamGizmos.DrawColliderOutline(gameObject);
            for (int i = 0; i < OnEnter.GetPersistentEventCount(); i++)
            {
                var t = OnEnter.GetPersistentTarget(i);
                Transform target = t is Component c ? c.transform : t is GameObject g ? g.transform : null;
                if (target != null) Gizmos.DrawLine(transform.position, target.position);
            }
        }
    }
}
