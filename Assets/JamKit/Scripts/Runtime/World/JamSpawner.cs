using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace JamKit
{
    [JamHelp("Spawns a prefab every few seconds, up to a maximum alive at once: enemy waves, falling rocks, collectibles. Spawn points are the children of 'SpawnPoints' (or this object).")]
    public class JamSpawner : JamBehaviour
    {
        [Tooltip("What to spawn.")]
        public GameObject prefab;
        [Tooltip("Seconds between spawns.")]
        [Min(0.05f)] public float interval = 3f;
        [Tooltip("Never more than this many alive at once.")]
        [Min(1)] public int maxAlive = 5;
        [Tooltip("Total to spawn before stopping. 0 = forever.")]
        [Min(0)] public int totalToSpawn = 0;
        [Tooltip("Start spawning when the level starts. Off = wait for StartSpawning() (e.g. from a Trigger Zone).")]
        public bool spawnOnStart = true;
        [Tooltip("Random offset around each spawn point (units).")]
        [Min(0)] public float randomRadius = 0f;

        [Header("Events")]
        public UnityEvent OnSpawn = new UnityEvent();

        readonly List<GameObject> alive = new List<GameObject>();
        readonly List<Transform> points = new List<Transform>();
        bool running;
        float nextSpawn;
        int spawned;

        void Start()
        {
            Transform root = transform.Find("SpawnPoints");
            if (root != null) foreach (Transform t in root) points.Add(t);
            if (points.Count == 0) points.Add(transform);
            if (spawnOnStart) StartSpawning();
        }

        public void StartSpawning()
        {
            running = true;
            nextSpawn = Time.time;
        }

        public void StopSpawning() => running = false;

        void Update()
        {
            if (!running || prefab == null || !JamGameManager.IsPlaying || Time.time < nextSpawn) return;
            alive.RemoveAll(g => g == null);
            if (alive.Count >= maxAlive) return;
            if (totalToSpawn > 0 && spawned >= totalToSpawn)
            {
                running = false;
                return;
            }
            SpawnOne();
        }

        /// <summary>Spawn one right now (also callable from events).</summary>
        public void SpawnOne()
        {
            if (prefab == null) return;
            nextSpawn = Time.time + interval;
            Transform p = points[Random.Range(0, points.Count)];
            Vector3 offset = Random.insideUnitSphere * randomRadius;
            if (GetComponent<Collider>() == null && GetComponentInChildren<Collider>() == null) offset.z = 0f;
            var go = Instantiate(prefab, p.position + offset, p.rotation);
            alive.Add(go);
            spawned++;
            OnSpawn.Invoke();
        }

        void OnDrawGizmos()
        {
            Gizmos.color = new Color(1f, 0.4f, 0.8f, 0.8f);
            Transform root = transform.Find("SpawnPoints");
            if (root != null && root.childCount > 0)
                foreach (Transform t in root) Gizmos.DrawWireSphere(t.position, Mathf.Max(0.2f, randomRadius));
            else Gizmos.DrawWireSphere(transform.position, Mathf.Max(0.2f, randomRadius));
        }
    }
}
