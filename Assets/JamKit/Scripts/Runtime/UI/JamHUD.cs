using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace JamKit
{
    [JamHelp("Shows hearts (or a health bar), a counter for any item id (coins by default) and the level timer. Parts hide themselves when the level doesn't use them.")]
    public class JamHUD : JamBehaviour
    {
        [Header("Health")]
        [Tooltip("Parent of the heart icons. Its first child is used as the heart template.")]
        public Transform hearts;
        [Tooltip("Show a bar instead of hearts.")]
        public bool useBar = false;
        [Tooltip("Filled Image used as the health bar (when 'Use Bar' is on).")]
        public Image healthBar;
        [Tooltip("Colour of a full heart.")]
        public Color heartFull = new Color(1f, 0.25f, 0.3f);
        [Tooltip("Colour of an empty heart.")]
        public Color heartEmpty = new Color(1f, 1f, 1f, 0.25f);

        [Header("Counter")]
        [Tooltip("Root of the counter (icon + text). Hidden if the level has none of this item.")]
        public GameObject coinCounter;
        [Tooltip("The counter's text.")]
        public TMP_Text coinText;
        [Tooltip("Which counter to show (same word as the collectibles' Item Id).")]
        public string itemId = "coin";
        [Tooltip("Show 'have / total in level' instead of just 'have'.")]
        public bool showTotal = true;

        [Header("Timer")]
        [Tooltip("Root of the timer. Hidden if the level has no JamTimer.")]
        public GameObject timer;
        [Tooltip("The timer's text.")]
        public TMP_Text timerText;

        readonly List<Image> heartImages = new List<Image>();
        JamHealth health;
        int totalInLevel;
        bool counterUsed;

        void Start()
        {
            foreach (var c in FindObjectsByType<JamCollectible>(FindObjectsSortMode.None))
                if (string.Equals(c.itemId, itemId, System.StringComparison.OrdinalIgnoreCase)) totalInLevel += c.amount;
            var goal = FindAnyObjectByType<JamGoal>();
            counterUsed = totalInLevel > 0 || (goal != null && string.Equals(goal.requiredItemId, itemId, System.StringComparison.OrdinalIgnoreCase));
            if (coinCounter != null) coinCounter.SetActive(counterUsed);
            if (hearts != null) hearts.gameObject.SetActive(!useBar);
            if (healthBar != null) healthBar.gameObject.SetActive(useBar);
        }

        void Update()
        {
            if (health == null && JamPlayer.Current != null) health = JamPlayer.Current.Health;
            UpdateHealth();
            UpdateCounter();
            UpdateTimer();
        }

        void UpdateHealth()
        {
            if (health == null) return;
            if (useBar)
            {
                if (healthBar != null) healthBar.fillAmount = (float)health.Current / Mathf.Max(1, health.maxHealth);
                return;
            }
            if (hearts == null || hearts.childCount == 0) return;
            if (heartImages.Count != health.maxHealth) BuildHearts(health.maxHealth);
            for (int i = 0; i < heartImages.Count; i++)
                heartImages[i].color = i < health.Current ? heartFull : heartEmpty;
        }

        void BuildHearts(int count)
        {
            var template = hearts.GetChild(0).gameObject;
            for (int i = hearts.childCount - 1; i >= 1; i--) Destroy(hearts.GetChild(i).gameObject);
            heartImages.Clear();
            heartImages.Add(template.GetComponent<Image>());
            for (int i = 1; i < count; i++)
                heartImages.Add(Instantiate(template, hearts).GetComponent<Image>());
            heartImages.RemoveAll(img => img == null);
        }

        void UpdateCounter()
        {
            if (!counterUsed || coinText == null) return;
            int have = JamInventory.Get(itemId);
            var goal = FindGoal();
            int total = goal != null && goal.RequiredTotal > 0 ? goal.RequiredTotal : totalInLevel;
            coinText.text = showTotal && total > 0 ? $"{have} / {total}" : have.ToString();
        }

        JamGoal cachedGoal;
        bool goalSearched;
        JamGoal FindGoal()
        {
            if (!goalSearched)
            {
                cachedGoal = FindAnyObjectByType<JamGoal>();
                if (cachedGoal != null && !string.Equals(cachedGoal.requiredItemId, itemId, System.StringComparison.OrdinalIgnoreCase)) cachedGoal = null;
                goalSearched = true;
            }
            return cachedGoal;
        }

        void UpdateTimer()
        {
            var t = JamTimer.Active;
            if (timer != null && timer.activeSelf != (t != null)) timer.SetActive(t != null);
            if (t != null && timerText != null) timerText.text = t.Display;
        }
    }
}
