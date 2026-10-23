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
            var clipSpeeds = new Dictionary<string, float>();
            // A hidden copy of the character to play clips on and watch where its feet are
            var rig = Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(character));
            rig.hideFlags = HideFlags.HideAndDontSave;
            var rigAnimator = rig.GetComponent<Animator>();
            if (rigAnimator == null) rigAnimator = rig.AddComponent<Animator>();
            rigAnimator.avatar = avatar;
            try
            {
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
                if (clipsByState.ContainsKey(state))
                {
                    r.AppendLine($"⚠️ {Path.GetFileName(path)}: a second {state} clip. Skipped.");
                    continue;
                }
                // In Place check: a clip that travels will make the character slide or snap back.
                if (clip.isHumanMotion && new Vector3(clip.averageSpeed.x, 0f, clip.averageSpeed.z).magnitude > 0.2f)
                {
                    r.AppendLine($"❌ {state} clip '{clip.name}' is not In Place: the character will slide. Re-download with In Place ticked.");
                    clipsByState[state] = clip;
                    continue;
                }
                // 3b. Fit the clip to the game's movement: the kit's jump leaves the ground at once,
                // landing starts on touchdown, footsteps fire when a foot plants.
                var feet = AnalyzeFeet(rig, rigAnimator, clip);
                var fitted = imp.clipAnimations;
                string fit = FitClip(state, feet, fitted[0], clip.frameRate);
                if (fit != null)
                {
                    imp.clipAnimations = fitted;
                    imp.SaveAndReimport();
                    clip = AssetDatabase.LoadAllAssetsAtPath(path).OfType<AnimationClip>().First(a => !a.name.StartsWith("__preview"));
                }
                if ((state == "Walk" || state == "Run") && feet.stanceSpeed > 0.2f) clipSpeeds[state] = feet.stanceSpeed;
                clipsByState[state] = clip;
                r.AppendLine($"✅ {state}: {clip.name}" + (LoopingStates.Contains(state) ? " (looping)" : "") + (fit != null ? ", " + fit : ""));
            }
            }
            finally
            {
                Object.DestroyImmediate(rig);
            }

            // States without a Mixamo clip would play the kit's empty placeholder and snap to T-pose: borrow a close match.
            foreach (var fb in new[] { ("Victory", "Idle"), ("Hurt", "Land"), ("Death", "Idle") })
            {
                if (clipsByState.ContainsKey(fb.Item1) || !clipsByState.TryGetValue(fb.Item2, out var stand)) continue;
                clipsByState[fb.Item1] = stand;
                r.AppendLine($"⚠️ {fb.Item1}: no clip, using {stand.name} for now. Download a '{fb.Item1}' animation from Mixamo (In Place, Without Skin) for a real one.");
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
                animator.enabled = true; // a switched-off Animator leaves the character frozen in T-pose
                animator.avatar = avatar;
                animator.runtimeAnimatorController = ov;
                animator.applyRootMotion = false;
                animator.Rebind();
                var humanoid = inst.GetComponent<JamHumanoidAnimator>();
                humanoid.walkClipSpeed = clipSpeeds.TryGetValue("Walk", out var ws) ? ws : 0f;
                humanoid.runClipSpeed = clipSpeeds.TryGetValue("Run", out var rs) ? rs : 0f;
                if (humanoid.walkClipSpeed > 0f)
                    r.AppendLine($"✅ Feet synced to movement: walk clip {humanoid.walkClipSpeed:0.0} m/s" + (humanoid.runClipSpeed > 0f ? $", run clip {humanoid.runClipSpeed:0.0} m/s" : ""));
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

        /// <summary>Where the feet are over a clip: lift-off, touchdown, each foot plant, and how fast a planted foot slides back (= the speed the clip walks at).</summary>
        class FootInfo
        {
            public float takeoff = -1f, touchdown = -1f, firstContact = -1f, stanceSpeed;
            public readonly List<float> plants = new List<float>();
        }

        static FootInfo AnalyzeFeet(GameObject rig, Animator rigAnimator, AnimationClip clip)
        {
            var info = new FootInfo();
            var feet = new[] { FootBone(rigAnimator, HumanBodyBones.LeftToes, HumanBodyBones.LeftFoot), FootBone(rigAnimator, HumanBodyBones.RightToes, HumanBodyBones.RightFoot) };
            if (feet[0] == null || feet[1] == null || clip.length <= 0f) return info;
            int n = Mathf.Max(2, Mathf.CeilToInt(clip.length * 60f));
            float dt = clip.length / n, minY = float.MaxValue;
            var pos = new Vector3[2, n + 1];
            for (int i = 0; i <= n; i++)
            {
                clip.SampleAnimation(rig, i * dt);
                for (int f = 0; f < 2; f++) { pos[f, i] = feet[f].position; minY = Mathf.Min(minY, pos[f, i].y); }
            }
            bool Down(int f, int i) => pos[f, i].y < minY + 0.03f;

            float slide = 0f, stance = 0f;
            bool wasAir = false;
            for (int i = 0; i <= n; i++)
            {
                for (int f = 0; f < 2; f++)
                {
                    if (i > 0 && Down(f, i) && !Down(f, i - 1)) info.plants.Add(i * dt);
                    if (i < n && Down(f, i) && Down(f, i + 1))
                    {
                        var d = pos[f, i] - pos[f, i + 1];
                        slide += new Vector2(d.x, d.z).magnitude;
                        stance += dt;
                    }
                }
                bool air = !Down(0, i) && !Down(1, i);
                if (air && !wasAir && i > 0 && info.takeoff < 0f) info.takeoff = i * dt;
                if (!air && wasAir && info.touchdown < 0f) info.touchdown = i * dt;
                if (!air && info.firstContact < 0f) info.firstContact = i * dt;
                wasAir = air;
            }
            info.stanceSpeed = stance > 0f ? slide / stance : 0f;
            return info;
        }

        static Transform FootBone(Animator a, HumanBodyBones toes, HumanBodyBones foot)
        {
            var t = a.GetBoneTransform(toes);
            return t != null ? t : a.GetBoneTransform(foot);
        }

        /// <summary>Trims jump / land to the moments the feet leave and touch the ground, adds OnFootstep events to walk / run. Returns what changed, or null.</summary>
        static string FitClip(string state, FootInfo feet, ModelImporterClipAnimation c, float fps)
        {
            float start = c.firstFrame / fps;
            if (state == "Jump" && feet.takeoff > 0.15f)
            {
                // Skip the crouch wind-up: the kit's jump is airborne the frame you press the button.
                float from = feet.takeoff - 0.1f, to = feet.touchdown > feet.takeoff ? feet.touchdown : c.lastFrame / fps - start;
                c.firstFrame = (start + from) * fps;
                c.lastFrame = (start + to) * fps;
                return $"starts at take-off ({to - from:0.00} s)";
            }
            if (state == "Land" && feet.firstContact > 0.1f)
            {
                c.firstFrame = (start + feet.firstContact - 0.05f) * fps;
                return "starts at touchdown";
            }
            if ((state == "Walk" || state == "Run") && feet.plants.Count > 0)
            {
                float length = c.lastFrame / fps - start;
                c.events = feet.plants.Select(t => new AnimationEvent { functionName = "OnFootstep", time = Mathf.Clamp01(t / length) }).ToArray();
                return $"{feet.plants.Count} footstep events";
            }
            return null;
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
