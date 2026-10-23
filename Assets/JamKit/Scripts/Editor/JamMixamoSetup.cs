using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace JamKit.EditorTools
{
    /// <summary>
    /// JamKit → 3D → Setup Mixamo Character. One click turns the FBX files in JamKit/_MixamoDropZone
    /// into a playable third-person character:
    /// 1 humanoid rig + avatar · 2 URP materials and textures · 3 loop / bake-into-pose clip settings ·
    /// 4 Animator Override Controller · 5 player prefab variant · 6 replace player in scene · 7 ✅/❌ report.
    /// Nothing here is ever exported: the drop zone is git-ignored and excluded from the package.
    /// </summary>
    public class JamMixamoSetup : EditorWindow
    {
        public const string DropZone = JamKitMenu.Root + "/_MixamoDropZone";
        const string Generated = DropZone + "/Generated";
        const string BasePlayer = JamKitMenu.Root + "/Prefabs/Player/Player_ThirdPerson3D.prefab";

        static readonly string[] LoopingStates = { "Idle", "Walk", "Run", "Fall" };
        static readonly Dictionary<string, string[]> Keywords = new Dictionary<string, string[]>
        {
            { "Idle", new[] { "idle" } },
            { "Walk", new[] { "walk" } },
            { "Run", new[] { "run", "jog", "sprint" } },
            { "Jump", new[] { "jump" } },
            { "Fall", new[] { "fall" } },
            { "Land", new[] { "land" } },
            { "Hurt", new[] { "hurt", "hit", "impact", "react" } },
            { "Death", new[] { "death", "dying", "die" } },
            { "Victory", new[] { "victory", "cheer", "celebrat", "dance", "win" } },
        };

        string report = "";
        string lastVariant;
        Vector2 scroll;

        [MenuItem("JamKit/3D/Setup Mixamo Character", priority = 10)]
        public static void Open()
        {
            var w = GetWindow<JamMixamoSetup>(false, "Setup Mixamo Character");
            w.minSize = new Vector2(480, 460);
        }

        void OnGUI()
        {
            JamBehaviourEditor.DrawJamHeader(typeof(JamMixamoSetup));
            EditorGUILayout.HelpBox(
                "1. mixamo.com → Characters → pick one (e.g. Y Bot) → Download: FBX for Unity, T-pose, With Skin.\n"
              + "2. Optional animations (Idle, Walking, Running, Jump, Falling Idle, Landing): tick IN PLACE, FBX for Unity, Without Skin.\n"
              + "3. Drop every FBX into JamKit/_MixamoDropZone, then click the button below.", MessageType.Info);
            var files = FindFbx();
            EditorGUILayout.LabelField($"Files in the drop zone: {files.Count}", EditorStyles.boldLabel);
            foreach (var f in files.Take(12)) EditorGUILayout.LabelField("   • " + Path.GetFileName(f), EditorStyles.miniLabel);
            if (files.Count > 12) EditorGUILayout.LabelField($"   … and {files.Count - 12} more", EditorStyles.miniLabel);
            EditorGUILayout.Space();
            using (new EditorGUI.DisabledScope(files.Count == 0))
            {
                if (GUILayout.Button("Set up character", GUILayout.Height(34)))
                {
                    report = Run(out lastVariant);
                    GUIUtility.ExitGUI();
                }
            }
            using (new EditorGUI.DisabledScope(string.IsNullOrEmpty(lastVariant)))
            {
                if (GUILayout.Button("Replace player in current scene", GUILayout.Height(28)))
                    report += "\n" + ReplacePlayerInScene(lastVariant);
            }
            if (GUILayout.Button("Open drop zone folder")) EditorUtility.RevealInFinder(DropZone + "/_README.txt");
            EditorGUILayout.HelpBox("Mixamo characters are free to use inside your game. Never re-upload the raw FBX files (not in a public repo, not in a package).", MessageType.Warning);
            if (!string.IsNullOrEmpty(report))
            {
                scroll = EditorGUILayout.BeginScrollView(scroll);
                EditorGUILayout.HelpBox(report, MessageType.None);
                EditorGUILayout.EndScrollView();
            }
        }

        static List<string> FindFbx()
        {
            if (!AssetDatabase.IsValidFolder(DropZone)) return new List<string>();
            return AssetDatabase.FindAssets("t:Model", new[] { DropZone })
                .Select(AssetDatabase.GUIDToAssetPath)
                .Where(p => p.EndsWith(".fbx", System.StringComparison.OrdinalIgnoreCase) && !p.StartsWith(Generated))
                .Distinct().ToList();
        }

        // ------------------------------------------------------------------ The 7 steps

        public static string Run(out string variantPath)
        {
            variantPath = null;
            var r = new StringBuilder();
            var files = FindFbx();
            if (files.Count == 0) return "❌ No FBX files in JamKit/_MixamoDropZone.";

            // The character = the FBX with the most skinned vertices; the others are animations.
            string character = files.OrderByDescending(SkinnedVertexCount).First();
            if (SkinnedVertexCount(character) == 0) return "❌ None of the FBX files has a skinned mesh. Download the character 'With Skin'.";
            var anims = files.Where(f => f != character).ToList();
            string charName = Sanitize(Path.GetFileNameWithoutExtension(character));
            r.AppendLine($"Character: {Path.GetFileName(character)}   Animations: {anims.Count}");

            // 1. Humanoid rig, avatar from the character
            var charImp = (ModelImporter)AssetImporter.GetAtPath(character);
            charImp.animationType = ModelImporterAnimationType.Human;
            charImp.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
            // 2. Materials: extract textures so URP Lit materials get them (no grey / pink character)
            charImp.materialImportMode = ModelImporterMaterialImportMode.ImportViaMaterialDescription;
            charImp.SaveAndReimport();
            string texFolder = Generated + "/Textures";
            JamEditorUtil.EnsureFolder(texFolder);
            try { charImp.ExtractTextures(texFolder); } catch { /* no embedded textures */ }
            AssetDatabase.Refresh();
            charImp.SaveAndReimport();

            var avatar = AssetDatabase.LoadAllAssetsAtPath(character).OfType<Avatar>().FirstOrDefault();
            bool humanOk = avatar != null && avatar.isValid && avatar.isHuman;
            r.AppendLine(humanOk ? "✅ Rig set to Humanoid, avatar created" : "❌ Humanoid avatar failed: the model may not be a standard humanoid (check the Rig tab → Configure).");
            if (!humanOk) return r.ToString();
            int pinkCount = AssetDatabase.LoadAllAssetsAtPath(character).OfType<Material>().Count(m => m.shader == null || m.shader.name.Contains("Error"));
            r.AppendLine(pinkCount == 0 ? "✅ Materials imported with URP shaders" : $"⚠️ {pinkCount} material(s) look broken: select the FBX → Materials tab → Extract Materials, then set them to URP Lit.");

            // 1+3. Animation FBX: copy the avatar, loop + bake-into-pose settings
            var clipsByState = new Dictionary<string, AnimationClip>();
            foreach (var path in anims)
            {
                var imp = (ModelImporter)AssetImporter.GetAtPath(path);
                imp.animationType = ModelImporterAnimationType.Human;
                imp.avatarSetup = ModelImporterAvatarSetup.CopyFromOther;
                imp.sourceAvatar = avatar;
                imp.importAnimation = true;
                string state = StateFor(Path.GetFileNameWithoutExtension(path));
                var clipSettings = imp.defaultClipAnimations;
                foreach (var c in clipSettings)
                {
                    c.loopTime = state != null && LoopingStates.Contains(state);
                    c.lockRootRotation = true;      // Root Transform Rotation: Bake Into Pose
                    c.lockRootHeightY = true;       // Root Transform Position (Y): Bake Into Pose
                    c.keepOriginalOrientation = true;
                    c.keepOriginalPositionY = true;
                    c.name = Path.GetFileNameWithoutExtension(path);
                }
                imp.clipAnimations = clipSettings;
                imp.SaveAndReimport();
                var clip = AssetDatabase.LoadAllAssetsAtPath(path).OfType<AnimationClip>().FirstOrDefault(a => !a.name.StartsWith("__preview"));
                if (clip == null)
                {
                    r.AppendLine($"⚠️ {Path.GetFileName(path)}: no animation found.");
                    continue;
                }
                if (state == null)
                {
                    r.AppendLine($"⚠️ {Path.GetFileName(path)}: name doesn't say which move it is (idle, walk, run, jump, fall, land…). Skipped.");
                    continue;
                }
                if (!clipsByState.ContainsKey(state)) clipsByState[state] = clip;
                // In Place check: a clip that travels will make the character slide or snap back.
                if (clip.isHumanMotion && new Vector3(clip.averageSpeed.x, 0f, clip.averageSpeed.z).magnitude > 0.2f)
                    r.AppendLine($"❌ {state} clip '{clip.name}' is not In Place: the character will slide. Re-download with In Place ticked.");
                else
                    r.AppendLine($"✅ {state}: {clip.name}" + (LoopingStates.Contains(state) ? " (looping)" : ""));
            }

            // 4. Animator Override Controller from JamHumanoid.controller; missing clips keep the kit's clips
            var baseCtrl = AssetDatabase.LoadAssetAtPath<AnimatorController>(JamEditorUtil.ControllerPath);
            if (baseCtrl == null) return r.AppendLine("❌ JamHumanoid.controller missing. Re-import the JamKit package.").ToString();
            JamEditorUtil.EnsureFolder(Generated);
            string overridePath = $"{Generated}/JamHumanoid_{charName}.overrideController";
            var ov = AssetDatabase.LoadAssetAtPath<AnimatorOverrideController>(overridePath);
            if (ov == null)
            {
                ov = new AnimatorOverrideController(baseCtrl);
                AssetDatabase.CreateAsset(ov, overridePath);
            }
            ov.runtimeAnimatorController = baseCtrl;
            var stateClips = BaseClipsByState(baseCtrl);
            var pairs = new List<KeyValuePair<AnimationClip, AnimationClip>>();
            ov.GetOverrides(pairs);
            for (int i = 0; i < pairs.Count; i++)
            {
                string state = stateClips.FirstOrDefault(kv => kv.Value == pairs[i].Key).Key;
                if (state != null && clipsByState.TryGetValue(state, out var replacement))
                    pairs[i] = new KeyValuePair<AnimationClip, AnimationClip>(pairs[i].Key, replacement);
            }
            ov.ApplyOverrides(pairs);
            EditorUtility.SetDirty(ov);
            var missing = Keywords.Keys.Where(s => !clipsByState.ContainsKey(s)).ToList();
            r.AppendLine("✅ Override controller: " + Path.GetFileName(overridePath));
            if (missing.Count > 0) r.AppendLine("   Using the kit's own clips for: " + string.Join(", ", missing));

            // 5. Player prefab variant with the new model, controller fitted to its size
            var basePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(BasePlayer);
            if (basePrefab == null) return r.AppendLine("❌ Player_ThirdPerson3D prefab missing.").ToString();
            var inst = (GameObject)PrefabUtility.InstantiatePrefab(basePrefab);
            try
            {
                inst.name = "Player_ThirdPerson3D_" + charName;
                var model = inst.transform.Find("Model");
                foreach (Transform child in model) child.gameObject.SetActive(false); // hide the kit's character
                var charAsset = AssetDatabase.LoadAssetAtPath<GameObject>(character);
                var charInst = (GameObject)PrefabUtility.InstantiatePrefab(charAsset, model);
                charInst.transform.localPosition = Vector3.zero;
                charInst.transform.localRotation = Quaternion.identity;
                var innerAnimator = charInst.GetComponent<Animator>();
                if (innerAnimator != null) Object.DestroyImmediate(innerAnimator, true);
                var animator = model.GetComponent<Animator>();
                animator.avatar = avatar;
                animator.runtimeAnimatorController = ov;
                animator.applyRootMotion = false;
                animator.Rebind();
                r.AppendLine(JamKitMenu.FitController(inst));
                variantPath = $"{Generated}/{inst.name}.prefab";
                PrefabUtility.SaveAsPrefabAsset(inst, variantPath);
                r.AppendLine($"✅ Prefab variant: {variantPath}");
            }
            finally
            {
                Object.DestroyImmediate(inst);
            }
            AssetDatabase.SaveAssets();
            r.AppendLine("Next: click 'Replace player in current scene', or drag the new prefab into your level.");
            Debug.Log("[JamKit] Setup Mixamo Character\n" + r);
            return r.ToString();
        }

        /// <summary>Step 6: swap the 3D player in the open scene for the new variant (keeps position and parent).</summary>
        public static string ReplacePlayerInScene(string variantPath)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(variantPath);
            if (prefab == null) return "❌ Variant not found. Run the setup first.";
            var old = Object.FindAnyObjectByType<JamPlayerThirdPerson3D>();
            if (old == null) return "❌ No 3D player in the open scene. Open Demo_3D_ThirdPerson or a 3D level.";
            var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab, old.transform.parent);
            go.transform.SetPositionAndRotation(old.transform.position, old.transform.rotation);
            go.transform.SetSiblingIndex(old.transform.GetSiblingIndex());
            go.name = "Player";
            Undo.RegisterCreatedObjectUndo(go, "Replace player");
            Undo.DestroyObjectImmediate(old.gameObject);
            Selection.activeGameObject = go;
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(go.scene);
            return "✅ Player replaced in the scene. Press Play!";
        }

        // ------------------------------------------------------------------ Helpers

        static int SkinnedVertexCount(string path)
        {
            var go = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (go == null) return 0;
            return go.GetComponentsInChildren<SkinnedMeshRenderer>(true).Sum(s => s.sharedMesh != null ? s.sharedMesh.vertexCount : 0);
        }

        static string StateFor(string fileName)
        {
            string n = fileName.ToLowerInvariant();
            // "Falling Idle" is a fall, not an idle: check the more specific words first.
            foreach (var state in new[] { "Fall", "Land", "Jump", "Run", "Walk", "Hurt", "Death", "Victory", "Idle" })
                if (Keywords[state].Any(k => n.Contains(k))) return state;
            return null;
        }

        /// <summary>State name → the clip the base controller uses for it (kit CC0 clip or placeholder).</summary>
        static Dictionary<string, AnimationClip> BaseClipsByState(AnimatorController ctrl)
        {
            var map = new Dictionary<string, AnimationClip>();
            foreach (var cs in ctrl.layers[0].stateMachine.states)
            {
                var st = cs.state;
                if (st.motion is AnimationClip clip) map[st.name] = clip;
                else if (st.motion is BlendTree tree)
                {
                    string[] names = { "Idle", "Walk", "Run" };
                    var children = tree.children;
                    for (int i = 0; i < children.Length && i < names.Length; i++)
                        if (children[i].motion is AnimationClip c) map[names[i]] = c;
                }
            }
            return map;
        }

        static string Sanitize(string s) => new string(s.Where(ch => char.IsLetterOrDigit(ch) || ch == '_').ToArray());
    }
}
