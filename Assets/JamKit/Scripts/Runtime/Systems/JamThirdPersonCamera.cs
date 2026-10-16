using UnityEngine;

namespace JamKit
{
    [JamHelp("Orbit camera for the 3D player: mouse / right stick to look, pulls in so it never goes through walls, locks the cursor while playing (Esc frees it).")]
    [RequireComponent(typeof(Camera))]
    [DisallowMultipleComponent]
    public class JamThirdPersonCamera : JamBehaviour
    {
        [Tooltip("What to orbit around. Empty = the player's 'CameraTarget' child (found automatically).")]
        public Transform target;
        [Tooltip("Normal distance from the target, in metres.")]
        public float distance = 6f;
        [Tooltip("Closest the camera gets when pulled in by a wall.")]
        public float minDistance = 0.8f;
        [Tooltip("Extra height above the target.")]
        public float heightOffset = 0.4f;
        [Tooltip("Sideways offset for an over-the-shoulder view (0 = centred).")]
        public float shoulderOffset = 0f;
        [Tooltip("Look sensitivity. Multiplied by the player's PlayerConfig 'Look Sensitivity'.")]
        public float sensitivity = 1f;
        [Tooltip("Lowest the camera can look (degrees, negative = from below).")]
        public float minPitch = -30f;
        [Tooltip("Highest the camera can look (degrees).")]
        public float maxPitch = 70f;
        [Tooltip("Radius used to detect walls between the target and the camera.")]
        public float collisionRadius = 0.25f;
        [Tooltip("Seconds to catch up with the target. 0 = locked on.")]
        public float followSmoothTime = 0.05f;
        [Tooltip("Hide and lock the mouse cursor while playing. It is released on pause, win and lose.")]
        public bool lockCursor = true;

        float yaw;
        float pitch = 20f;
        float currentDistance;
        Vector3 pivot;
        Vector3 pivotVelocity;
        Transform playerRoot;
        PlayerConfig playerConfig;

        void Start()
        {
            FindTarget();
            if (target != null)
            {
                yaw = target.eulerAngles.y;
                pivot = target.position;
            }
            currentDistance = distance;
        }

        void FindTarget()
        {
            var player = JamPlayer.Current != null ? JamPlayer.Current : FindAnyObjectByType<JamPlayer>();
            if (player == null) return;
            playerRoot = player.transform;
            playerConfig = player.config;
            if (target == null)
            {
                var tp = player as JamPlayerThirdPerson3D;
                target = tp != null && tp.cameraTarget != null ? tp.cameraTarget : player.transform;
            }
            else if (target.GetComponentInParent<JamPlayer>() is JamPlayer p)
            {
                playerRoot = p.transform;
            }
        }

        void LateUpdate()
        {
            if (target == null)
            {
                FindTarget();
                if (target == null) return;
            }

            bool playing = JamGameManager.IsPlaying && !JamMessageUI.DialogueOpen;
            UpdateCursor(playing);

            if (playing)
            {
                float sens = sensitivity * (playerConfig != null ? playerConfig.lookSensitivity : 1f);
                bool invert = playerConfig != null && playerConfig.invertLookY;
                Vector2 look = JamInput.Look;
                yaw += look.x * sens;
                pitch += (invert ? look.y : -look.y) * sens;
                pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
            }

            Vector3 goal = target.position + Vector3.up * heightOffset;
            pivot = followSmoothTime > 0f ? Vector3.SmoothDamp(pivot, goal, ref pivotVelocity, followSmoothTime) : goal;

            Quaternion rot = Quaternion.Euler(pitch, yaw, 0f);
            Vector3 origin = pivot + rot * Vector3.right * shoulderOffset;
            Vector3 back = rot * Vector3.back;

            float wanted = distance;
            if (JamPhysics.SphereCast3D(origin, collisionRadius, back, distance, playerRoot, out RaycastHit hit))
                wanted = Mathf.Max(minDistance, hit.distance - 0.05f);
            // Pull in instantly, ease back out
            currentDistance = wanted < currentDistance ? wanted : Mathf.Lerp(currentDistance, wanted, 1f - Mathf.Exp(-6f * Time.unscaledDeltaTime));

            transform.SetPositionAndRotation(origin + back * currentDistance, rot);
        }

        void UpdateCursor(bool playing)
        {
            if (!lockCursor) return;
            if (playing)
            {
                if (Cursor.lockState != CursorLockMode.Locked)
                {
                    Cursor.lockState = CursorLockMode.Locked;
                    Cursor.visible = false;
                }
            }
            else if (Cursor.lockState != CursorLockMode.None)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }

        void OnDisable()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}
