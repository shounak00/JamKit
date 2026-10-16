using System.Collections.Generic;
using UnityEngine;

namespace JamKit
{
    /// <summary>
    /// Base for every World component. Turns the 2D and 3D trigger/collision callbacks into one
    /// HandleEnter / HandleStay / HandleExit, so the same prefab logic works in 2D and in 3D.
    /// </summary>
    public abstract class JamTriggerBehaviour : JamBehaviour
    {
        // CharacterController does not send OnCollision messages; the 3D player forwards its hits here instead.
        readonly Dictionary<GameObject, float> controllerTouches = new Dictionary<GameObject, float>();
        const float ControllerTouchTimeout = 0.15f;

        protected virtual void HandleEnter(GameObject other) { }
        protected virtual void HandleStay(GameObject other) { }
        protected virtual void HandleExit(GameObject other) { }

        // 2D
        protected virtual void OnTriggerEnter2D(Collider2D c) { if (enabled) HandleEnter(c.gameObject); }
        protected virtual void OnTriggerStay2D(Collider2D c) { if (enabled) HandleStay(c.gameObject); }
        protected virtual void OnTriggerExit2D(Collider2D c) { if (enabled) HandleExit(c.gameObject); }
        protected virtual void OnCollisionEnter2D(Collision2D c) { if (enabled) HandleEnter(c.gameObject); }
        protected virtual void OnCollisionStay2D(Collision2D c) { if (enabled) HandleStay(c.gameObject); }
        protected virtual void OnCollisionExit2D(Collision2D c) { if (enabled) HandleExit(c.gameObject); }

        // 3D
        protected virtual void OnTriggerEnter(Collider c) { if (enabled) HandleEnter(c.gameObject); }
        protected virtual void OnTriggerStay(Collider c) { if (enabled) HandleStay(c.gameObject); }
        protected virtual void OnTriggerExit(Collider c) { if (enabled) HandleExit(c.gameObject); }
        protected virtual void OnCollisionEnter(Collision c) { if (enabled) HandleEnter(c.gameObject); }
        protected virtual void OnCollisionStay(Collision c) { if (enabled) HandleStay(c.gameObject); }
        protected virtual void OnCollisionExit(Collision c) { if (enabled) HandleExit(c.gameObject); }

        /// <summary>Called by JamPlayerThirdPerson3D when its CharacterController bumps a solid collider.</summary>
        public void NotifyControllerTouch(GameObject who)
        {
            if (!enabled) return;
            bool isNew = !controllerTouches.ContainsKey(who);
            controllerTouches[who] = Time.time;
            if (isNew) HandleEnter(who);
            else HandleStay(who);
        }

        protected virtual void Update()
        {
            if (controllerTouches.Count == 0) return;
            List<GameObject> expired = null;
            foreach (var pair in controllerTouches)
            {
                if (pair.Key == null || Time.time - pair.Value > ControllerTouchTimeout)
                    (expired ??= new List<GameObject>()).Add(pair.Key);
            }
            if (expired == null) return;
            foreach (var go in expired)
            {
                controllerTouches.Remove(go);
                if (go != null) HandleExit(go);
            }
        }

        /// <summary>The player this object belongs to, or null.</summary>
        protected static JamPlayer PlayerOf(GameObject go) => go != null ? go.GetComponentInParent<JamPlayer>() : null;

        /// <summary>True if <paramref name="go"/> is part of this object (so we never react to ourselves).</summary>
        protected bool IsSelf(GameObject go) => go != null && go.transform.IsChildOf(transform);
    }

    /// <summary>Small physics helpers that need no layers: they skip triggers and the caller's own colliders.</summary>
    public static class JamPhysics
    {
        static readonly RaycastHit2D[] hits2D = new RaycastHit2D[16];
        static readonly RaycastHit[] hits3D = new RaycastHit[16];

        public static bool Raycast2D(Vector2 origin, Vector2 dir, float distance, Transform ignoreRoot, out RaycastHit2D hit)
        {
            var filter = ContactFilter2D.noFilter;
            filter.useTriggers = false;
            int n = Physics2D.Raycast(origin, dir, filter, hits2D, distance);
            float best = float.MaxValue;
            hit = default;
            bool found = false;
            for (int i = 0; i < n; i++)
            {
                var h = hits2D[i];
                if (h.collider == null || h.collider.isTrigger) continue;
                if (ignoreRoot != null && h.collider.transform.IsChildOf(ignoreRoot)) continue;
                if (h.distance < best) { best = h.distance; hit = h; found = true; }
            }
            return found;
        }

        public static bool Raycast3D(Vector3 origin, Vector3 dir, float distance, Transform ignoreRoot, out RaycastHit hit)
        {
            int n = Physics.RaycastNonAlloc(origin, dir, hits3D, distance, ~0, QueryTriggerInteraction.Ignore);
            return Closest(n, ignoreRoot, out hit);
        }

        public static bool SphereCast3D(Vector3 origin, float radius, Vector3 dir, float distance, Transform ignoreRoot, out RaycastHit hit)
        {
            int n = Physics.SphereCastNonAlloc(origin, radius, dir, hits3D, distance, ~0, QueryTriggerInteraction.Ignore);
            return Closest(n, ignoreRoot, out hit);
        }

        static bool Closest(int n, Transform ignoreRoot, out RaycastHit hit)
        {
            float best = float.MaxValue;
            hit = default;
            bool found = false;
            for (int i = 0; i < n; i++)
            {
                var h = hits3D[i];
                if (h.collider == null) continue;
                if (ignoreRoot != null && h.collider.transform.IsChildOf(ignoreRoot)) continue;
                if (h.distance <= 0f && h.point == Vector3.zero) continue; // started inside
                if (h.distance < best) { best = h.distance; hit = h; found = true; }
            }
            return found;
        }
    }
}
