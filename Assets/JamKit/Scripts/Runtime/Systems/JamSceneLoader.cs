using UnityEngine;
using UnityEngine.SceneManagement;

namespace JamKit
{
    [JamHelp("Scene changes for buttons and events: LoadScene(name), LoadNext(), Reload(), Quit(). Drag this object into a Button's OnClick and pick a function.")]
    public class JamSceneLoader : JamBehaviour
    {
        /// <summary>Loads a scene by name. The scene must be in Build Settings (JamKit → Validate Scene can add it).</summary>
        public void LoadScene(string sceneName) => Load(sceneName);

        /// <summary>Loads the next scene in Build Settings (wraps to the first one after the last).</summary>
        public void LoadNext() => LoadNextInBuild();

        /// <summary>Restarts the current scene.</summary>
        public void Reload() => ReloadCurrent();

        /// <summary>Quits the game (stops Play mode in the Editor).</summary>
        public void Quit() => QuitGame();

        // ------------------------------------------------------------------ Static versions

        public static void Load(string sceneName)
        {
            if (string.IsNullOrWhiteSpace(sceneName))
            {
                Debug.LogWarning("[JamKit] LoadScene was called with an empty scene name.");
                return;
            }
            Time.timeScale = 1f;
            if (Application.CanStreamedLevelBeLoaded(sceneName))
            {
                SceneManager.LoadScene(sceneName);
                return;
            }
#if UNITY_EDITOR
            // In the Editor we can still load it, but the build would fail: warn loudly.
            foreach (string guid in UnityEditor.AssetDatabase.FindAssets("t:Scene " + sceneName))
            {
                string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
                if (System.IO.Path.GetFileNameWithoutExtension(path) != sceneName) continue;
                Debug.LogWarning($"[JamKit] Scene '{sceneName}' is not in Build Settings. It loads in the Editor but NOT in a build. Run JamKit → Validate Scene → Fix.");
                UnityEditor.SceneManagement.EditorSceneManager.LoadSceneInPlayMode(path, new LoadSceneParameters(LoadSceneMode.Single));
                return;
            }
#endif
            Debug.LogError($"[JamKit] Scene '{sceneName}' was not found in Build Settings. Check the spelling, then run JamKit → Validate Scene → Fix.");
        }

        public static void LoadNextInBuild()
        {
            int count = SceneManager.sceneCountInBuildSettings;
            if (count == 0)
            {
                Debug.LogError("[JamKit] No scenes in Build Settings.");
                return;
            }
            Time.timeScale = 1f;
            int next = (SceneManager.GetActiveScene().buildIndex + 1) % count;
            SceneManager.LoadScene(next);
        }

        public static void ReloadCurrent()
        {
            Time.timeScale = 1f;
            Scene s = SceneManager.GetActiveScene();
            if (s.buildIndex >= 0)
            {
                SceneManager.LoadScene(s.buildIndex);
                return;
            }
#if UNITY_EDITOR
            UnityEditor.SceneManagement.EditorSceneManager.LoadSceneInPlayMode(s.path, new LoadSceneParameters(LoadSceneMode.Single));
#else
            SceneManager.LoadScene(s.name);
#endif
        }

        public static void QuitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
