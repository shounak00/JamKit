using UnityEngine;
using UnityEngine.EventSystems;

namespace JamKit
{
    /// <summary>
    /// Makes sure UI buttons work whatever input backend the project uses: adds the right input module
    /// to the EventSystem at runtime (scenes ship with a bare EventSystem so they import cleanly anywhere).
    /// </summary>
    public static class JamUISetup
    {
        public static void EnsureEventSystem()
        {
            var es = Object.FindAnyObjectByType<EventSystem>(FindObjectsInactive.Include);
            if (es == null)
            {
                var go = new GameObject("EventSystem");
                es = go.AddComponent<EventSystem>();
            }

#if ENABLE_INPUT_SYSTEM
            var legacy = es.GetComponent<StandaloneInputModule>();
            if (legacy != null) Object.Destroy(legacy);
            if (es.GetComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>() == null)
                es.gameObject.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
#else
            if (es.GetComponent<BaseInputModule>() == null)
                es.gameObject.AddComponent<StandaloneInputModule>();
#endif
        }
    }
}
