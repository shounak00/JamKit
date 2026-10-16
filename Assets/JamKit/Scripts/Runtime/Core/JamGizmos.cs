using UnityEngine;

namespace JamKit
{
    /// <summary>Draws a component's collider outline in the Scene view, in the current Gizmos colour.</summary>
    public static class JamGizmos
    {
        public static void DrawColliderOutline(GameObject go)
        {
            var c2 = go.GetComponent<Collider2D>();
            if (c2 != null)
            {
                Bounds b = c2.bounds;
                if (b.size == Vector3.zero) return;
                Gizmos.DrawWireCube(b.center, new Vector3(b.size.x, b.size.y, 0.01f));
                return;
            }
            var c3 = go.GetComponent<Collider>();
            if (c3 != null)
            {
                Bounds b = c3.bounds;
                if (b.size == Vector3.zero) return;
                Gizmos.DrawWireCube(b.center, b.size);
            }
        }
    }
}
