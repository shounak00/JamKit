using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace JamKit.EditorTools
{
    /// <summary>
    /// Editor helpers shared by the shipped tools (Validate, Mixamo setup) and the internal builder.
    /// Lives outside Scripts/Editor/Internal because that folder is not exported with the kit.
    /// </summary>
    public static class JamEditorUtil
    {
        public const string ControllerPath = JamKitMenu.Root + "/Animation/3D/JamHumanoid.controller";

        public static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            string parent = Path.GetDirectoryName(path).Replace('\\', '/');
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, Path.GetFileName(path));
        }

        // ------------------------------------------------------------------ Render pipeline: index 0 = 2D Renderer, index 1 = Universal Renderer

        public const int Renderer2DIndex = 0;
        public const int Renderer3DIndex = 1;

        public static string SetupRenderers()
        {
            var assets = new System.Collections.Generic.HashSet<UniversalRenderPipelineAsset>();
            if (GraphicsSettings.defaultRenderPipeline is UniversalRenderPipelineAsset d) assets.Add(d);
            for (int i = 0; i < UnityEngine.QualitySettings.names.Length; i++)
                if (UnityEngine.QualitySettings.GetRenderPipelineAssetAt(i) is UniversalRenderPipelineAsset q) assets.Add(q);
            if (assets.Count == 0) return "❌ No URP asset found. Use the Universal 2D template or assign a URP asset in Project Settings → Graphics.";

            var report = new System.Text.StringBuilder();
            foreach (var urp in assets)
            {
                var so = new SerializedObject(urp);
                var list = so.FindProperty("m_RendererDataList");
                string folder = Path.GetDirectoryName(AssetDatabase.GetAssetPath(urp)).Replace('\\', '/');

                // Index 0 must be a 2D renderer
                var first = list.arraySize > 0 ? list.GetArrayElementAtIndex(0).objectReferenceValue : null;
                if (!(first is Renderer2DData))
                {
                    var r2d = FindOrCreateRenderer(folder + "/Renderer2D.asset", "_2DRenderer");
                    list.InsertArrayElementAtIndex(0);
                    list.GetArrayElementAtIndex(0).objectReferenceValue = r2d;
                }

                // Index 1 must be a Universal (3D) renderer
                var second = list.arraySize > 1 ? list.GetArrayElementAtIndex(1).objectReferenceValue : null;
                if (!(second is UniversalRendererData))
                {
                    var r3d = AssetDatabase.LoadAssetAtPath<UniversalRendererData>(folder + "/UniversalRenderer3D.asset")
                              ?? FindOrCreateRenderer(folder + "/UniversalRenderer3D.asset", "UniversalRenderer") as UniversalRendererData;
                    if (list.arraySize < 2) list.arraySize = 2;
                    if (second != null)
                    {
                        list.InsertArrayElementAtIndex(1);
                    }
                    list.GetArrayElementAtIndex(1).objectReferenceValue = r3d;
                }
                so.FindProperty("m_DefaultRendererIndex").intValue = Renderer2DIndex;
                so.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(urp);
                report.AppendLine($"URP '{urp.name}': [0] {Name(list, 0)} · [1] {Name(list, 1)}");
            }
            AssetDatabase.SaveAssets();
            return report.ToString().TrimEnd();
        }

        static string Name(SerializedProperty list, int i) =>
            list.arraySize > i && list.GetArrayElementAtIndex(i).objectReferenceValue != null ? list.GetArrayElementAtIndex(i).objectReferenceValue.name : "missing";

        static ScriptableRendererData FindOrCreateRenderer(string path, string rendererTypeName)
        {
            var existing = AssetDatabase.LoadAssetAtPath<ScriptableRendererData>(path);
            if (existing != null) return existing;
            // URP's own (internal) factory sets up shaders and post-processing data correctly.
            var method = typeof(UniversalRenderPipelineAsset).GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static)
                .FirstOrDefault(m => m.Name == "CreateRendererAsset" && m.GetParameters().Length == 4);
            if (method == null) throw new Exception("URP CreateRendererAsset not found; add a Universal Renderer to the URP asset by hand (index 1).");
            var enumType = method.GetParameters()[1].ParameterType;
            object rendererType = Enum.Parse(enumType, rendererTypeName);
            var created = (ScriptableRendererData)method.Invoke(null, new object[] { path, rendererType, false, "" });
            string createdPath = AssetDatabase.GetAssetPath(created);
            if (createdPath != path && !string.IsNullOrEmpty(createdPath)) AssetDatabase.MoveAsset(createdPath, path);
            return AssetDatabase.LoadAssetAtPath<ScriptableRendererData>(path) ?? created;
        }
    }
}
