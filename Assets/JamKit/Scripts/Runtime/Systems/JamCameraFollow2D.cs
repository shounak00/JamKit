using UnityEngine;

namespace JamKit
{
    [JamHelp("Smoothly follows the player in 2D, looking a little ahead. Turn on 'Use Bounds' to stop the camera at the edges of your level (yellow box).")]
    [RequireComponent(typeof(Camera))]
    [DisallowMultipleComponent]
    public class JamCameraFollow2D : JamBehaviour
    {
        [Tooltip("What to follow. Empty = the player (found automatically).")]
        public Transform target;
        [Tooltip("Seconds to catch up. Lower = tighter, higher = floatier.")]
        public float smoothTime = 0.15f;
        [Tooltip("How far (units) to look ahead in the direction the player is moving/facing.")]
        public float lookAhead = 1.5f;
        [Tooltip("Offset from the target (e.g. Y = 1 shows more above the player).")]
        public Vector2 offset = new Vector2(0f, 1f);
        [Tooltip("Keep the camera inside the Bounds rectangle.")]
        public bool useBounds = false;
        [Tooltip("Level area the camera may show (world units). Drawn as a yellow box in the Scene view.")]
        public Rect bounds = new Rect(-10f, -6f, 60f, 24f);

        Camera cam;
        Vector3 velocity;
        float currentLookAhead;
        JamPlayer player;

        void Awake() => cam = GetComponent<Camera>();

        void Start()
        {
            FindTarget();
            if (target != null) transform.position = Clamp(Desired(snapLookAhead: true));
        }

        void FindTarget()
        {
            if (target != null)
            {
                player = target.GetComponentInParent<JamPlayer>();
                return;
            }
            player = JamPlayer.Current != null ? JamPlayer.Current : FindAnyObjectByType<JamPlayer>();
            if (player != null) target = player.transform;
        }

        void LateUpdate()
        {
            if (target == null)
            {
                FindTarget();
                if (target == null) return;
            }
            Vector3 desired = Clamp(Desired(snapLookAhead: false));
            transform.position = Vector3.SmoothDamp(transform.position, desired, ref velocity, smoothTime);
        }

        Vector3 Desired(bool snapLookAhead)
        {
            float dir = 0f;
            if (player != null)
            {
                float vx = player.Velocity.x;
                dir = Mathf.Abs(vx) > 0.5f ? Mathf.Sign(vx) : player.Facing.x;
            }
            float goal = dir * lookAhead;
            currentLookAhead = snapLookAhead ? goal : Mathf.MoveTowards(currentLookAhead, goal, lookAhead * 2f * Time.deltaTime);
            Vector3 p = target.position + (Vector3)offset + Vector3.right * currentLookAhead;
            p.z = transform.position.z;
            return p;
        }

        Vector3 Clamp(Vector3 p)
        {
            if (!useBounds || cam == null || !cam.orthographic) return p;
            float halfH = cam.orthographicSize;
            float halfW = halfH * cam.aspect;
            p.x = bounds.width < halfW * 2f ? bounds.center.x : Mathf.Clamp(p.x, bounds.xMin + halfW, bounds.xMax - halfW);
            p.y = bounds.height < halfH * 2f ? bounds.center.y : Mathf.Clamp(p.y, bounds.yMin + halfH, bounds.yMax - halfH);
            return p;
        }

        void OnDrawGizmosSelected()
        {
            if (!useBounds) return;
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(bounds.center, bounds.size);
        }
    }
}
