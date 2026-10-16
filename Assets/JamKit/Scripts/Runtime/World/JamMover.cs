using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace JamKit
{
    public enum JamMoverMode
    {
        PingPong,
        Loop,
        Once
    }

    [JamHelp("Moves between waypoints: moving platforms, elevators, flying enemies. Waypoints are the children of the 'Waypoints' child; drag them in the Scene view. Carries the player.")]
    public class JamMover : JamBehaviour
    {
        [Tooltip("Speed in units per second.")]
        [Min(0)] public float speed = 2f;
        [Tooltip("PingPong: back and forth. Loop: last point back to the first. Once: stop at the end.")]
        public JamMoverMode mode = JamMoverMode.PingPong;
        [Tooltip("Seconds to pause at each waypoint.")]
        [Min(0)] public float waitAtPoint = 0.5f;
        [Tooltip("Don't move until StartMoving() is called (from a Trigger Zone) or the player stands on it.")]
        public bool startOnTrigger = false;
        [Tooltip("Optional: the object holding the waypoints. Empty = a child called 'Waypoints'.")]
        public Transform waypointsRoot;

        [Header("Events")]
        public UnityEvent OnArrived = new UnityEvent();

        /// <summary>Current velocity (world units per second).</summary>
        public Vector3 Velocity { get; private set; }
        public Vector2 Velocity2D => Velocity;

        readonly List<Vector3> points = new List<Vector3>();
        int index;
        int direction = 1;
        float waitUntil;
        bool moving;
        Rigidbody2D rb2;
        Rigidbody rb3;

        void Awake()
        {
            rb2 = GetComponent<Rigidbody2D>();
            rb3 = GetComponent<Rigidbody>();
            if (rb2 != null)
            {
                rb2.bodyType = RigidbodyType2D.Kinematic;
                rb2.interpolation = RigidbodyInterpolation2D.Interpolate;
            }
            if (rb3 != null)
            {
                rb3.isKinematic = true;
                rb3.interpolation = RigidbodyInterpolation.Interpolate;
            }
        }

        void Start()
        {
            points.Clear();
            points.Add(transform.position);
            Transform root = Root();
            if (root != null)
                foreach (Transform t in root) points.Add(t.position); // cached in world space: they move with us after this
            moving = !startOnTrigger;
            index = points.Count > 1 ? 1 : 0;
        }

        Transform Root() => waypointsRoot != null ? waypointsRoot : transform.Find("Waypoints");

        public void StartMoving() => moving = true;
        public void StopMoving() => moving = false;
        public void ToggleMoving() => moving = !moving;

        void FixedUpdate()
        {
            if (startOnTrigger && !moving && PlayerOnTop()) moving = true;

            Velocity = Vector3.zero;
            if (moving && points.Count > 1 && Time.time >= waitUntil)
            {
                Vector3 pos = CurrentPosition();
                Vector3 target = points[index];
                Vector3 next = Vector3.MoveTowards(pos, target, speed * Time.fixedDeltaTime);
                MoveTo(next);
                Velocity = (next - pos) / Time.fixedDeltaTime;
                if ((next - target).sqrMagnitude < 0.0001f) Arrive();
            }
        }

        Vector3 CurrentPosition()
        {
            if (rb2 != null) return rb2.position;
            if (rb3 != null) return rb3.position;
            return transform.position;
        }

        void MoveTo(Vector3 p)
        {
            if (rb2 != null) rb2.MovePosition(p);
            else if (rb3 != null) rb3.MovePosition(p);
            else transform.position = p;
        }

        void Arrive()
        {
            waitUntil = Time.time + waitAtPoint;
            OnArrived.Invoke();
            switch (mode)
            {
                case JamMoverMode.Loop:
                    index = (index + 1) % points.Count;
                    break;
                case JamMoverMode.Once:
                    if (index < points.Count - 1) index++;
                    else moving = false;
                    break;
                default:
                    if (index + direction >= points.Count || index + direction < 0) direction = -direction;
                    index += direction;
                    break;
            }
        }

        bool PlayerOnTop()
        {
            var p = JamPlayer.Current;
            if (p == null || !p.IsGrounded) return false;
            Vector3 d = p.transform.position - transform.position;
            return d.y > 0f && d.y < 2f && Mathf.Abs(d.x) < 2f && Mathf.Abs(d.z) < 2f;
        }

        void OnDrawGizmos()
        {
            Transform root = Root();
            if (root == null || root.childCount == 0) return;
            Gizmos.color = new Color(0.2f, 0.8f, 1f, 0.9f);
            Vector3 prev = Application.isPlaying && points.Count > 0 ? points[0] : transform.position;
            Vector3 first = prev;
            if (Application.isPlaying)
            {
                for (int i = 1; i < points.Count; i++)
                {
                    Gizmos.DrawLine(prev, points[i]);
                    Gizmos.DrawWireSphere(points[i], 0.15f);
                    prev = points[i];
                }
            }
            else
            {
                foreach (Transform t in root)
                {
                    Gizmos.DrawLine(prev, t.position);
                    Gizmos.DrawWireSphere(t.position, 0.15f);
                    prev = t.position;
                }
            }
            if (mode == JamMoverMode.Loop) Gizmos.DrawLine(prev, first);
        }
    }
}
