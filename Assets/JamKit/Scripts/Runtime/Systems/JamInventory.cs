using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace JamKit
{
    /// <summary>
    /// Counters by item id ("coin", "key", anything you type). Filled by JamCollectible, read by the HUD,
    /// JamGoal, JamTriggerZone and JamDoor. Everything resets to zero when a level loads.
    /// </summary>
    public static class JamInventory
    {
        static readonly Dictionary<string, int> counts = new Dictionary<string, int>();

        /// <summary>Raised whenever a counter changes: (itemId, newCount).</summary>
        public static event Action<string, int> OnChanged;

        public static int Get(string itemId)
        {
            if (string.IsNullOrEmpty(itemId)) return 0;
            return counts.TryGetValue(Normalize(itemId), out int n) ? n : 0;
        }

        public static bool Has(string itemId, int amount = 1) => string.IsNullOrEmpty(itemId) || Get(itemId) >= amount;

        public static void Add(string itemId, int amount = 1)
        {
            if (string.IsNullOrEmpty(itemId) || amount == 0) return;
            string id = Normalize(itemId);
            int n = Mathf.Max(0, Get(id) + amount);
            counts[id] = n;
            OnChanged?.Invoke(id, n);
        }

        /// <summary>Removes <paramref name="amount"/> if there is enough. Returns false (and removes nothing) otherwise.</summary>
        public static bool TryConsume(string itemId, int amount = 1)
        {
            if (string.IsNullOrEmpty(itemId)) return true;
            if (Get(itemId) < amount) return false;
            Add(itemId, -amount);
            return true;
        }

        public static void Set(string itemId, int value)
        {
            if (string.IsNullOrEmpty(itemId)) return;
            string id = Normalize(itemId);
            counts[id] = Mathf.Max(0, value);
            OnChanged?.Invoke(id, counts[id]);
        }

        public static void ResetAll()
        {
            var ids = new List<string>(counts.Keys);
            counts.Clear();
            foreach (var id in ids) OnChanged?.Invoke(id, 0);
        }

        static string Normalize(string id) => id.Trim().ToLowerInvariant();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Init()
        {
            counts.Clear();
            OnChanged = null;
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (mode == LoadSceneMode.Single) ResetAll();
        }
    }
}
