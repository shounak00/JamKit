using UnityEngine;
using UnityEngine.Events;

namespace JamKit
{
    [JamHelp("Touch it to win the level. Optionally locked until you have enough of an item (e.g. 10 coins); shows a hint if you arrive too early.")]
    public class JamGoal : JamTriggerBehaviour
    {
        [Tooltip("Item needed to use the goal (e.g. 'coin' or 'key'). Leave empty for no requirement.")]
        public string requiredItemId = "";
        [Tooltip("How many of that item are needed.")]
        [Min(0)] public int requiredAmount = 0;
        [Tooltip("Need ALL of that item that exist in the level (counted at start). Overrides 'Required Amount'.")]
        public bool requireAllInLevel = false;
        [Tooltip("Hint shown when the player arrives without enough. {have} and {need} are replaced with numbers.")]
        public string lockedMessage = "Collect {need} {item} first! ({have}/{need})";
        [Tooltip("Optional sound on reaching the goal.")]
        public AudioClip reachSound;
        [Tooltip("Optional object shown while locked (e.g. a closed gate). Hidden when unlocked.")]
        public GameObject lockedVisual;

        [Header("Events")]
        public UnityEvent OnReached = new UnityEvent();

        int need;
        bool reached;

        void Start()
        {
            need = requiredAmount;
            if (requireAllInLevel && !string.IsNullOrEmpty(requiredItemId))
            {
                need = 0;
                foreach (var c in FindObjectsByType<JamCollectible>(FindObjectsSortMode.None))
                    if (string.Equals(c.itemId, requiredItemId, System.StringComparison.OrdinalIgnoreCase)) need += c.amount;
            }
        }

        /// <summary>Items still needed in the level (used by the HUD as "x / total").</summary>
        public int RequiredTotal => need;

        public bool IsUnlocked => string.IsNullOrEmpty(requiredItemId) || JamInventory.Get(requiredItemId) >= need;

        protected override void Update()
        {
            base.Update();
            if (lockedVisual != null && lockedVisual.activeSelf == IsUnlocked) lockedVisual.SetActive(!IsUnlocked);
        }

        protected override void HandleEnter(GameObject other)
        {
            if (reached || PlayerOf(other) == null) return;
            if (!IsUnlocked)
            {
                string msg = lockedMessage.Replace("{have}", JamInventory.Get(requiredItemId).ToString())
                                          .Replace("{need}", need.ToString())
                                          .Replace("{item}", requiredItemId + (need == 1 ? "" : "s"));
                JamMessageUI.Toast(msg);
                return;
            }
            reached = true;
            JamAudio.Play(reachSound);
            OnReached.Invoke();
            if (JamGameManager.Instance != null) JamGameManager.Instance.Win();
            else Debug.Log("[JamKit] Goal reached! Add a GameManager to show the Win screen.");
        }

        void OnDrawGizmos()
        {
            Gizmos.color = new Color(0.2f, 1f, 0.3f, 0.7f);
            JamGizmos.DrawColliderOutline(gameObject);
            Gizmos.DrawIcon(transform.position + Vector3.up * 1.2f, "sv_icon_dot3_pix16_gizmo", true);
        }
    }
}
