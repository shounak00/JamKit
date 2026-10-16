using UnityEngine;

namespace JamKit
{
    [JamHelp("Background layer that scrolls slower than the camera for depth. 0 = moves with the world, 1 = stuck to the camera (far away). Use 0.2–0.8 for layers in between.")]
    public class JamParallax : JamBehaviour
    {
        [Tooltip("How much the layer follows the camera on X and Y. 0 = normal object, 1 = glued to the camera.")]
        public Vector2 parallaxFactor = new Vector2(0.5f, 0.3f);
        [Tooltip("Repeat the sprite sideways forever (needs a SpriteRenderer on this object or its 'Visual').")]
        public bool infiniteHorizontal = false;

        Transform cam;
        Vector3 lastCamPos;
        float spriteWidth;

        void Start()
        {
            var sr = GetComponentInChildren<SpriteRenderer>();
            if (sr != null) spriteWidth = sr.bounds.size.x;
        }

        void LateUpdate()
        {
            // Start measuring on the first LateUpdate, after the camera has snapped to the player.
            if (cam == null)
            {
                if (Camera.main == null) return;
                cam = Camera.main.transform;
                lastCamPos = cam.position;
                return;
            }
            Vector3 delta = cam.position - lastCamPos;
            transform.position += new Vector3(delta.x * parallaxFactor.x, delta.y * parallaxFactor.y, 0f);
            lastCamPos = cam.position;

            if (infiniteHorizontal && spriteWidth > 0f)
            {
                float diff = cam.position.x - transform.position.x;
                if (Mathf.Abs(diff) >= spriteWidth) transform.position += Vector3.right * (Mathf.Sign(diff) * spriteWidth);
            }
        }
    }
}
