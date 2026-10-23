using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering.Universal;

namespace JamKit.EditorTools
{
    /// <summary>JamKit → Validate Scene: a friendly ✅/❌ checklist for the open level, with Fix buttons.</summary>
    public class JamValidateWindow : EditorWindow
    {
        public class Check
        {
            public bool ok;
            public bool warningOnly;
            public string label;
            public string detail;
            public Action fix;
        }

        List<Check> checks = new List<Check>();
        Vector2 scroll;

        public static void Open()
        {
            var w = GetWindow<JamValidateWindow>(false, "Validate Scene");
            w.minSize = new Vector2(480, 420);
            w.Refresh();
        }

        void OnEnable() => Refresh();
        void OnFocus() => Refresh();

        void Refresh()
        {
            checks = Run();
            Repaint();
        }

        void OnGUI()
        {
            JamBehaviourEditor.DrawJamHeader(typeof(JamValidateWindow));
            EditorGUILayout.LabelField("Scene: " + EditorSceneManager.GetActiveScene().name, EditorStyles.boldLabel);
            int bad = checks.Count(c => !c.ok && !c.warningOnly);
            EditorGUILayout.HelpBox(bad == 0 ? "All good! Press Play, then make a build (JamKit → Prepare WebGL Build)." : $"{bad} thing(s) to fix. Click Fix where you see it.",
                bad == 0 ? MessageType.Info : MessageType.Warning);
            scroll = EditorGUILayout.BeginScrollView(scroll);
            foreach (var c in checks)
            {
                using (new EditorGUILayout.HorizontalScope(EditorStyles.helpBox))
                {
                    string icon = c.ok ? "✅" : c.warningOnly ? "⚠️" : "❌";
                    using (new EditorGUILayout.VerticalScope())
                    {
                        EditorGUILayout.LabelField(icon + "  " + c.label, EditorStyles.boldLabel);
                        if (!string.IsNullOrEmpty(c.detail)) EditorGUILayout.LabelField(c.detail, EditorStyles.wordWrappedMiniLabel);
                    }
                    if (!c.ok && c.fix != null && GUILayout.Button("Fix", GUILayout.Width(60), GUILayout.Height(30)))
                    {
                        c.fix();
                        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
                        Refresh();
                        GUIUtility.ExitGUI();
                    }
                }
            }
            EditorGUILayout.EndScrollView();
            if (GUILayout.Button("Check again", GUILayout.Height(26))) Refresh();
        }

        // ------------------------------------------------------------------ The checks

        public static List<Check> Run()
        {
            var list = new List<Check>();
            var scene = EditorSceneManager.GetActiveScene();
            var players = FindAll<JamPlayer>();
            bool is3D = players.Any(p => p is JamPlayerThirdPerson3D) || FindAll<JamThirdPersonCamera>().Length > 0;
            bool isTopDown = players.Any(p => p is JamPlayerTopDown2D);

            list.Add(new Check
            {
                ok = players.Length == 1,
                label = players.Length == 1 ? "One Player" : players.Length == 0 ? "No Player in the scene" : $"{players.Length} Players in the scene",
                detail = players.Length > 1 ? "Delete the extra ones: " + string.Join(", ", players.Select(p => p.name)) : players.Length == 0 ? "Drag a player from JamKit/Prefabs/Player." : null,
                fix = players.Length == 0 ? () => AddPrefab("Player", is3D ? "Player_ThirdPerson3D" : isTopDown ? "Player_TopDown2D" : "Player_Platformer2D", "---- PLAYER ----", "Player") : (Action)null
            });

            if (players.Length >= 1)
            {
                var noConfig = players.Where(p => p.config == null).ToArray();
                list.Add(new Check
                {
                    ok = noConfig.Length == 0,
                    label = "Player has a PlayerConfig",
                    detail = noConfig.Length == 0 ? null : "The Config field is empty. Fix assigns the default preset.",
                    fix = () =>
                    {
                        foreach (var p in noConfig)
                        {
                            Undo.RecordObject(p, "Assign PlayerConfig");
                            string name = p is JamPlayerThirdPerson3D ? "PlayerConfig_ThirdPerson_Default" : p is JamPlayerTopDown2D ? "PlayerConfig_TopDown" : "PlayerConfig_Snappy";
                            p.config = AssetDatabase.LoadAssetAtPath<PlayerConfig>($"{JamKitMenu.Root}/Configs/{name}.asset");
                            EditorUtility.SetDirty(p);
                        }
                    }
                });
            }

            var gms = FindAll<JamGameManager>();
            list.Add(new Check
            {
                ok = gms.Length == 1,
                label = gms.Length == 1 ? "One GameManager" : gms.Length == 0 ? "No GameManager (no Win/Lose screens)" : $"{gms.Length} GameManagers",
                detail = gms.Length > 1 ? "Keep only one." : null,
                fix = gms.Length == 0 ? () => AddPrefab("Systems", "GameManager", "---- SYSTEMS ----") : (Action)null
            });

            var goals = FindAll<JamGoal>();
            bool timerWin = FindAll<JamTimer>().Any(t => t.mode == JamTimerMode.CountDown && t.onZero == JamTimerEnd.Win);
            list.Add(new Check
            {
                ok = goals.Length > 0 || timerWin,
                label = "A way to win (a Goal, or a countdown Timer set to Win)",
                detail = goals.Length > 0 || timerWin ? null : "A game isn't a game until it can end. Fix adds a Goal next to the player.",
                fix = () =>
                {
                    var p = FindAll<JamPlayer>().FirstOrDefault();
                    Vector3 pos = p != null ? p.transform.position + (is3D ? new Vector3(0, 0, 6) : new Vector3(6, 0, 0)) : Vector3.zero;
                    var g = AddPrefab("World", is3D ? "Goal_3D" : "Goal_2D", "Goal");
                    if (g != null) g.transform.position = pos;
                }
            });

            var cam = Camera.main;
            list.Add(new Check
            {
                ok = cam != null,
                label = "A Main Camera",
                detail = cam != null ? null : "No camera tagged MainCamera.",
                fix = () => AddPrefab("Systems", is3D ? "ThirdPersonCamera" : "Camera2D", "---- CAMERA ----", "Main Camera")
            });

            if (cam != null)
            {
                var data = cam.GetUniversalAdditionalCameraData();
                int want = is3D ? 1 : 0;
                bool rendererExists = RendererCount() > want;
                list.Add(new Check
                {
                    ok = rendererExists,
                    label = is3D ? "3D renderer available (URP renderer index 1)" : "2D renderer available (URP renderer index 0)",
                    detail = rendererExists ? null : "The URP asset is missing a renderer. Fix adds a 2D renderer at 0 and a Universal (3D) renderer at 1.",
                    fix = () => JamEditorUtil.SetupRenderers()
                });
                var so = new SerializedObject(data);
                int current = so.FindProperty("m_RendererIndex").intValue;
                list.Add(new Check
                {
                    ok = current == want,
                    label = $"Camera uses the {(is3D ? "3D" : "2D")} renderer",
                    detail = current == want ? null : $"Camera renderer index is {current}, should be {want}.",
                    fix = () =>
                    {
                        Undo.RecordObject(data, "Set renderer");
                        data.SetRenderer(want);
                        EditorUtility.SetDirty(data);
                    }
                });
            }

            var huds = FindAll<JamHUD>();
            list.Add(new Check
            {
                ok = huds.Length > 0,
                label = "HUD and Win/Lose screens (UI_Canvas)",
                detail = huds.Length > 0 ? null : "Fix adds the UI_Canvas prefab.",
                fix = () => AddPrefab("UI", "UI_Canvas", "---- UI ----", "UI_Canvas")
            });

            list.Add(new Check
            {
                ok = FindAll<EventSystem>().Length > 0,
                label = "EventSystem (makes buttons clickable)",
                fix = () =>
                {
                    var go = new GameObject("EventSystem", typeof(EventSystem));
                    Undo.RegisterCreatedObjectUndo(go, "Add EventSystem");
                    var parent = GameObject.Find("---- SYSTEMS ----");
                    if (parent != null) go.transform.SetParent(parent.transform);
                }
            });

            if (!is3D)
            {
                var badTilemaps = FindAll<UnityEngine.Tilemaps.TilemapCollider2D>().Length == 0 && FindAll<UnityEngine.Tilemaps.Tilemap>().Length > 0;
                list.Add(new Check
                {
                    ok = !badTilemaps,
                    warningOnly = true,
                    label = "Ground tilemap has a collider",
                    detail = badTilemaps ? "Your tilemaps have no Tilemap Collider 2D, so the player will fall through. Add one (+ Composite Collider 2D) to Tilemap_Ground." : null
                });
            }

            bool inBuild = !string.IsNullOrEmpty(scene.path) && EditorBuildSettings.scenes.Any(s => s.path == scene.path && s.enabled);
            list.Add(new Check
            {
                ok = inBuild,
                label = "Scene is in Build Settings",
                detail = inBuild ? null : string.IsNullOrEmpty(scene.path) ? "Save the scene first (Ctrl+S)." : "Without this, Next Level and builds can't find it.",
                fix = string.IsNullOrEmpty(scene.path) ? (Action)null : () => AddToBuildSettings(scene.path)
            });

            var first = EditorBuildSettings.scenes.FirstOrDefault(s => s.enabled);
            bool menuFirst = first != null && Path.GetFileNameWithoutExtension(first.path) == "MainMenu";
            list.Add(new Check
            {
                ok = menuFirst,
                warningOnly = true,
                label = "MainMenu is scene 0 in Build Settings",
                detail = menuFirst ? null : "The game starts with whatever scene is first. Fix puts the kit's MainMenu first (edit its title text!).",
                fix = () => PutMainMenuFirst()
            });

            var gm = gms.FirstOrDefault();
            if (gm != null && gm.HasNextLevel)
            {
                bool nextOk = EditorBuildSettings.scenes.Any(s => s.enabled && Path.GetFileNameWithoutExtension(s.path) == gm.nextLevelName);
                list.Add(new Check
                {
                    ok = nextOk,
                    label = $"Next level '{gm.nextLevelName}' is in Build Settings",
                    detail = nextOk ? null : "Check the spelling in the GameManager's Next Level Name, and add that scene to Build Settings.",
                    fix = () =>
                    {
                        string guid = AssetDatabase.FindAssets("t:Scene " + gm.nextLevelName).FirstOrDefault(g => Path.GetFileNameWithoutExtension(AssetDatabase.GUIDToAssetPath(g)) == gm.nextLevelName);
                        if (guid != null) AddToBuildSettings(AssetDatabase.GUIDToAssetPath(guid));
                        else EditorUtility.DisplayDialog("JamKit", $"No scene called '{gm.nextLevelName}' found.", "OK");
                    }
                });
            }
            return list;
        }

        // ------------------------------------------------------------------ Helpers

        static T[] FindAll<T>() where T : UnityEngine.Object => UnityEngine.Object.FindObjectsByType<T>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        static int RendererCount()
        {
            var urp = UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;
            if (urp == null) return 0;
            return new SerializedObject(urp).FindProperty("m_RendererDataList").arraySize;
        }

        static GameObject AddPrefab(string folder, string name, string parentName, string rename = null)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{JamKitMenu.Root}/Prefabs/{folder}/{name}.prefab");
            if (prefab == null)
            {
                EditorUtility.DisplayDialog("JamKit", $"Prefab not found: {folder}/{name}", "OK");
                return null;
            }
            var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            if (rename != null) go.name = rename;
            var parent = GameObject.Find(parentName);
            if (parent != null) go.transform.SetParent(parent.transform, false);
            Undo.RegisterCreatedObjectUndo(go, "Add " + name);
            Selection.activeGameObject = go;
            return go;
        }

        public static void AddToBuildSettings(string path)
        {
            var scenes = EditorBuildSettings.scenes.ToList();
            var existing = scenes.FirstOrDefault(s => s.path == path);
            if (existing != null) existing.enabled = true;
            else scenes.Add(new EditorBuildSettingsScene(path, true));
            EditorBuildSettings.scenes = scenes.ToArray();
        }

        static void PutMainMenuFirst()
        {
            string menu = $"{JamKitMenu.Root}/Scenes/MainMenu.unity";
            var custom = AssetDatabase.FindAssets("t:Scene MainMenu", new[] { "Assets/_Game" }).Select(AssetDatabase.GUIDToAssetPath).FirstOrDefault();
            if (custom != null) menu = custom;
            var scenes = EditorBuildSettings.scenes.Where(s => s.path != menu).ToList();
            scenes.Insert(0, new EditorBuildSettingsScene(menu, true));
            EditorBuildSettings.scenes = scenes.ToArray();
        }
    }
}
