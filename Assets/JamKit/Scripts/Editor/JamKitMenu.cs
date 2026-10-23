using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace JamKit.EditorTools
{
    /// <summary>The JamKit menu: New Level, Validate Scene, Prepare WebGL Build, Help.</summary>
    public static class JamKitMenu
    {
        public const string Root = "Assets/JamKit";
        public const string Version = "1.1";
        public const string RepoUrl = "https://github.com/shounak00/JamKit";

        // ------------------------------------------------------------------ New Level

        [MenuItem("JamKit/New Level/2D Platformer", priority = 1)]
        static void NewPlatformer() => NewLevel("Level_Template_Platformer2D", 0);

        [MenuItem("JamKit/New Level/2D Top-Down", priority = 2)]
        static void NewTopDown() => NewLevel("Level_Template_TopDown2D", 0);

        [MenuItem("JamKit/New Level/3D Third-Person", priority = 3)]
        static void NewThirdPerson() => NewLevel("Level_Template_ThirdPerson3D", 1);

        static void NewLevel(string template, int rendererIndex)
        {
            string src = $"{Root}/Scenes/{template}.unity";
            if (!File.Exists(src))
            {
                EditorUtility.DisplayDialog("JamKit", $"Template scene not found:\n{src}", "OK");
                return;
            }
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            if (!AssetDatabase.IsValidFolder("Assets/_Game/Scenes"))
            {
                if (!AssetDatabase.IsValidFolder("Assets/_Game")) AssetDatabase.CreateFolder("Assets", "_Game");
                AssetDatabase.CreateFolder("Assets/_Game", "Scenes");
            }
            int n = 1;
            while (File.Exists($"Assets/_Game/Scenes/Level_{n:00}_MyLevel.unity")) n++;
            string path = EditorUtility.SaveFilePanelInProject("New Level", $"Level_{n:00}_MyLevel", "unity",
                "Name your level (Level_01_Name, Level_02_Name…). It is saved in Assets/_Game/Scenes.", "Assets/_Game/Scenes");
            if (string.IsNullOrEmpty(path)) return;
            if (!AssetDatabase.CopyAsset(src, path))
            {
                EditorUtility.DisplayDialog("JamKit", "Could not create the level (does a file with that name already exist?).", "OK");
                return;
            }
            EditorSceneManager.OpenScene(path);
            foreach (var cam in Object.FindObjectsByType<Camera>(FindObjectsSortMode.None))
            {
                cam.GetUniversalAdditionalCameraData().SetRenderer(rendererIndex);
                EditorUtility.SetDirty(cam);
            }
            EditorSceneManager.SaveOpenScenes();
            JamValidateWindow.AddToBuildSettings(path);
            Debug.Log($"[JamKit] New level created at {path} and added to Build Settings. Drag prefabs from JamKit/Prefabs into the LEVEL section.");
        }

        // ------------------------------------------------------------------ Validate / WebGL / Help

        [MenuItem("JamKit/Validate Scene", priority = 20)]
        public static void ValidateScene() => JamValidateWindow.Open();

        [MenuItem("JamKit/Prepare WebGL Build", priority = 21)]
        public static void PrepareWebGL() => JamWebGLWindow.Open();

        [MenuItem("JamKit/Help (Cheat Sheet)", priority = 40)]
        public static void Help()
        {
            string pdf = $"{Root}/Docs/JamKit_CheatSheet.pdf";
            string md = $"{Root}/Docs/README.md";
            if (File.Exists(pdf)) EditorUtility.OpenWithDefaultApp(Path.GetFullPath(pdf));
            else if (File.Exists(md))
            {
                var asset = AssetDatabase.LoadAssetAtPath<TextAsset>(md);
                if (asset != null)
                {
                    Selection.activeObject = asset;
                    EditorGUIUtility.PingObject(asset);
                }
                EditorUtility.OpenWithDefaultApp(Path.GetFullPath(md));
            }
            else Application.OpenURL(RepoUrl);
        }

        [MenuItem("JamKit/Welcome Window", priority = 41)]
        public static void Welcome() => JamWelcomeWindow.Open();

        // ------------------------------------------------------------------ 3D helpers

        [MenuItem("JamKit/3D/Fit Controller To Model", priority = 11)]
        public static void FitControllerMenu()
        {
            var players = Selection.gameObjects.Select(g => g.GetComponentInParent<JamPlayerThirdPerson3D>()).Where(p => p != null).Distinct().ToList();
            if (players.Count == 0) players = Object.FindObjectsByType<JamPlayerThirdPerson3D>(FindObjectsSortMode.None).ToList();
            if (players.Count == 0)
            {
                EditorUtility.DisplayDialog("JamKit", "Select a 3D player (or open a scene that has one) first.", "OK");
                return;
            }
            foreach (var p in players) Debug.Log("[JamKit] " + FitController(p.gameObject));
        }

        /// <summary>Sizes the CharacterController (and CameraTarget) to the meshes under 'Model'.</summary>
        public static string FitController(GameObject player)
        {
            var cc = player.GetComponent<CharacterController>();
            var model = player.transform.Find("Model");
            if (cc == null || model == null) return $"❌ {player.name}: needs a CharacterController and a 'Model' child.";
            var renderers = model.GetComponentsInChildren<Renderer>().Where(r => r.enabled && r.gameObject.activeInHierarchy && !(r is ParticleSystemRenderer)).ToArray();
            if (renderers.Length == 0) return $"❌ {player.name}: no visible meshes under Model.";
            Bounds b = renderers[0].bounds;
            foreach (var r in renderers) b.Encapsulate(r.bounds);
            Vector3 min = player.transform.InverseTransformPoint(b.min), max = player.transform.InverseTransformPoint(b.max);
            float height = Mathf.Max(0.5f, max.y - Mathf.Max(0f, min.y));
            // Depth (front to back) is the reliable width: a T-pose's arms make X far too wide.
            float radius = Mathf.Clamp(Mathf.Min(max.x - min.x, max.z - min.z) * 0.6f, 0.25f, Mathf.Min(0.5f, height * 0.5f - 0.01f));
            Undo.RecordObject(cc, "Fit Controller To Model");
            cc.height = height;
            cc.radius = radius;
            cc.center = new Vector3(0f, height * 0.5f, 0f);
            var target = player.transform.Find("CameraTarget");
            if (target != null)
            {
                Undo.RecordObject(target, "Fit Controller To Model");
                target.localPosition = new Vector3(0f, height * 0.85f, 0f);
            }
            EditorUtility.SetDirty(cc);
            if (PrefabUtility.IsPartOfPrefabInstance(cc)) PrefabUtility.RecordPrefabInstancePropertyModifications(cc);
            return $"✅ {player.name}: CharacterController height {height:0.00} m, radius {radius:0.00} m.";
        }
    }

    // ====================================================================== Prepare WebGL Build

    public class JamWebGLWindow : EditorWindow
    {
        string company;
        string product;
        string report = "";

        public static void Open()
        {
            var w = GetWindow<JamWebGLWindow>(true, "Prepare WebGL Build");
            w.minSize = new Vector2(440, 330);
            w.company = PlayerSettings.companyName;
            w.product = PlayerSettings.productName;
        }

        void OnGUI()
        {
            JamBehaviourEditor.DrawJamHeader(typeof(JamWebGLWindow));
            EditorGUILayout.HelpBox("Sets everything itch.io needs: WebGL platform, no compression (no blank page), 960×540. Then build to a folder and zip its contents for itch.", MessageType.Info);
            company = EditorGUILayout.TextField(new GUIContent("Team / company name", "Shown in the build and used for save data."), company);
            product = EditorGUILayout.TextField(new GUIContent("Game title", "The name of your game."), product);
            EditorGUILayout.Space();
            if (GUILayout.Button("1. Apply WebGL settings", GUILayout.Height(30))) report = Apply(company, product);
            if (GUILayout.Button("2. Build WebGL now…", GUILayout.Height(30)))
            {
                report = Apply(company, product);
                string folder = EditorUtility.SaveFolderPanel("Build WebGL into…", Path.GetFullPath("Builds"), "WebGL");
                if (!string.IsNullOrEmpty(folder)) report += "\n" + Build(folder);
            }
            EditorGUILayout.Space();
            EditorGUILayout.HelpBox("itch.io: Kind of project = HTML, upload a .zip of the build folder's CONTENTS, tick 'This file will be played in the browser', viewport 960×540, enable the Fullscreen button.", MessageType.None);
            if (!string.IsNullOrEmpty(report)) EditorGUILayout.HelpBox(report, MessageType.None);
        }

        public static string Apply(string company, string product)
        {
            var sb = new System.Text.StringBuilder();
            if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.WebGL)
            {
                bool ok = EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.WebGL, BuildTarget.WebGL);
                sb.AppendLine(ok ? "✅ Platform switched to WebGL" : "❌ Could not switch to WebGL. Install 'Web Build Support' in Unity Hub → Installs → Add modules.");
            }
            else sb.AppendLine("✅ Platform is WebGL");
            if (!string.IsNullOrWhiteSpace(company)) PlayerSettings.companyName = company.Trim();
            if (!string.IsNullOrWhiteSpace(product)) PlayerSettings.productName = product.Trim();
            PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Disabled;
            PlayerSettings.WebGL.decompressionFallback = false;
            PlayerSettings.WebGL.dataCaching = true;
            PlayerSettings.defaultWebScreenWidth = 960;
            PlayerSettings.defaultWebScreenHeight = 540;
            PlayerSettings.runInBackground = true;
            sb.AppendLine("✅ Compression: Disabled (works on every host)");
            sb.AppendLine("✅ Resolution: 960×540");
            sb.AppendLine($"✅ Names: {PlayerSettings.companyName} / {PlayerSettings.productName}");
            var scenes = EditorBuildSettings.scenes.Where(s => s.enabled).ToArray();
            sb.AppendLine(scenes.Length > 0 ? $"✅ {scenes.Length} scene(s) in Build Settings, first: {Path.GetFileNameWithoutExtension(scenes[0].path)}" : "❌ No scenes in Build Settings: run JamKit → Validate Scene → Fix");
            return sb.ToString().TrimEnd();
        }

        public static string Build(string folder)
        {
            var scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray();
            if (scenes.Length == 0) return "❌ No scenes to build.";
            var result = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = folder,
                target = BuildTarget.WebGL,
                options = BuildOptions.None
            });
            return result.summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded
                ? $"✅ Built to {folder} ({result.summary.totalSize / (1024 * 1024)} MB). Zip the folder's contents and upload to itch.io."
                : $"❌ Build failed ({result.summary.totalErrors} errors). Check the Console.";
        }
    }

    // ====================================================================== Welcome window (first open)

    [InitializeOnLoad]
    public class JamWelcomeWindow : EditorWindow
    {
        const string PrefKey = "JamKit.WelcomeShown.";

        static JamWelcomeWindow()
        {
            EditorApplication.delayCall += () =>
            {
                if (Application.isBatchMode || SessionState.GetBool("JamKit.WelcomeChecked", false)) return;
                SessionState.SetBool("JamKit.WelcomeChecked", true);
                string key = PrefKey + Application.dataPath.GetHashCode();
                if (EditorPrefs.GetBool(key, false)) return;
                EditorPrefs.SetBool(key, true);
                Open();
            };
        }

        public static void Open()
        {
            var w = GetWindow<JamWelcomeWindow>(true, "Welcome to JamKit");
            w.minSize = w.maxSize = new Vector2(460, 470);
        }

        void OnGUI()
        {
            var title = new GUIStyle(EditorStyles.boldLabel) { fontSize = 20, alignment = TextAnchor.MiddleCenter };
            GUILayout.Space(10);
            GUILayout.Label("JamKit v" + JamKitMenu.Version, title);
            GUILayout.Label("Make a game without code", new GUIStyle(EditorStyles.centeredGreyMiniLabel) { fontSize = 12 });
            GUILayout.Space(8);
            EditorGUILayout.HelpBox("The golden rule:  Drag a prefab  →  Set fields in the Inspector  →  Press Play.\n\n"
                                  + "Your files go in Assets/_Game. Don't edit Assets/JamKit (updates would overwrite it).", MessageType.Info);
            GUILayout.Space(6);
            if (GUILayout.Button("Open Cheat Sheet", GUILayout.Height(34))) JamKitMenu.Help();
            GUILayout.Space(6);
            GUILayout.Label("Play a demo", EditorStyles.boldLabel);
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("2D Platformer", GUILayout.Height(28))) OpenScene("Demo/Demo_2D_Platformer");
                if (GUILayout.Button("2D Top-Down", GUILayout.Height(28))) OpenScene("Demo/Demo_2D_TopDown");
                if (GUILayout.Button("3D Third-Person", GUILayout.Height(28))) OpenScene("Demo/Demo_3D_ThirdPerson");
            }
            GUILayout.Space(6);
            GUILayout.Label("Start your own", EditorStyles.boldLabel);
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("New 2D Platformer", GUILayout.Height(28))) EditorApplication.ExecuteMenuItem("JamKit/New Level/2D Platformer");
                if (GUILayout.Button("New Top-Down", GUILayout.Height(28))) EditorApplication.ExecuteMenuItem("JamKit/New Level/2D Top-Down");
                if (GUILayout.Button("New 3D", GUILayout.Height(28))) EditorApplication.ExecuteMenuItem("JamKit/New Level/3D Third-Person");
            }
            GUILayout.FlexibleSpace();
            EditorGUILayout.HelpBox("Make a build on the first night of the jam, not 10 minutes before the deadline:  JamKit → Prepare WebGL Build.", MessageType.Warning);
            GUILayout.Label("Reopen this window from JamKit → Welcome Window.", EditorStyles.centeredGreyMiniLabel);
            GUILayout.Space(6);
        }

        static void OpenScene(string name)
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            EditorSceneManager.OpenScene($"{JamKitMenu.Root}/Scenes/{name}.unity");
        }
    }
}
