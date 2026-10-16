using UnityEngine;
using UnityEngine.Events;

namespace JamKit
{
    [JamHelp("Something to pick up. Adds 'Amount' to the counter named 'Item Id' (coin, key, gem… any word). The HUD, Goal, Door and Trigger Zone can read that counter.")]
    public class JamCollectible : JamTriggerBehaviour
    {
        [Tooltip("Name of the counter to add to. Use the same word in JamGoal / JamTriggerZone 'Required Item Id'.")]
        public string itemId = "coin";
        [Tooltip("How much to add.")]
        public int amount = 1;
        [Tooltip("Optional sound when picked up.")]
        public AudioClip pickupSound;
        [Tooltip("Optional effect prefab spawned when picked up (particles…).")]
        public GameObject pickupEffect;
        [Tooltip("Hearts restored when picked up (health pickups). 0 = none.")]
        [Min(0)] public int healAmount = 0;
        [Tooltip("Gently bob (2D) or spin (3D) the 'Visual' child so it reads as a pickup.")]
        public bool animate = true;

        [Header("Events")]
        public UnityEvent OnCollected = new UnityEvent();

        Transform visual;
        Vector3 visualStart;
        bool collected;
        bool is3D;

        void Awake()
        {
            visual = transform.Find("Visual");
            if (visual != null) visualStart = visual.localPosition;
            is3D = GetComponent<Collider>() != null;
        }

        protected override void Update()
        {
            base.Update();
            if (!animate || visual == null) return;
            if (is3D) visual.Rotate(0f, 120f * Time.deltaTime, 0f, Space.World);
            visual.localPosition = visualStart + Vector3.up * (Mathf.Sin(Time.time * 3f + transform.position.x) * 0.08f);
        }

        protected override void HandleEnter(GameObject other)
        {
            if (collected) return;
            var player = PlayerOf(other);
            if (player == null) return;
            if (healAmount > 0 && string.IsNullOrEmpty(itemId) && player.Health.Current >= player.Health.maxHealth) return; // pure heart pickup at full health: leave it

            collected = true;
            JamInventory.Add(itemId, amount);
            if (healAmount > 0) player.Health.Heal(healAmount);
            JamAudio.Play(pickupSound);
            if (pickupEffect != null) Instantiate(pickupEffect, transform.position, Quaternion.identity);
            PlayAndDetachFX(transform);
            OnCollected.Invoke();
            Destroy(gameObject);
        }

        /// <summary>Lets the particles in an 'FX' child finish playing after the object is gone.</summary>
        public static void PlayAndDetachFX(Transform root)
        {
            var fx = root.Find("FX");
            if (fx == null) return;
            var systems = fx.GetComponentsInChildren<ParticleSystem>(true);
            if (systems.Length == 0) return;
            fx.SetParent(null, true);
            foreach (var ps in systems) ps.Play(true);
            Destroy(fx.gameObject, 3f);
        }

        void OnDrawGizmos()
        {
            Gizmos.color = new Color(1f, 0.85f, 0.1f, 0.6f);
            JamGizmos.DrawColliderOutline(gameObject);
        }
    }
}
