using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace JamKit.EditorTools
{
    /// <summary>Every JamKit component gets a header and a one-line plain-English help box above its fields.</summary>
    [CustomEditor(typeof(JamBehaviour), true)]
    [CanEditMultipleObjects]
    public class JamBehaviourEditor : Editor
    {
        static GUIStyle headerStyle;

        public override void OnInspectorGUI()
        {
            DrawJamHeader(target.GetType());
            serializedObject.Update();
            DrawPropertiesExcluding(serializedObject, "m_Script");
            serializedObject.ApplyModifiedProperties();
        }

        public static void DrawJamHeader(System.Type type)
        {
            headerStyle ??= new GUIStyle(EditorStyles.boldLabel) { fontSize = 13 };
            var rect = EditorGUILayout.GetControlRect(false, 22f);
            EditorGUI.DrawRect(new Rect(rect.x - 2, rect.y, rect.width + 4, rect.height), new Color(0.95f, 0.55f, 0.15f, 0.25f));
            EditorGUI.LabelField(new Rect(rect.x + 4, rect.y + 2, rect.width, rect.height), "JamKit · " + ObjectNames.NicifyVariableName(type.Name.Replace("Jam", "")), headerStyle);
            var help = type.GetCustomAttribute<JamHelpAttribute>(false);
            if (help != null) EditorGUILayout.HelpBox(help.Text, MessageType.None);
            EditorGUILayout.Space(2);
        }
    }

    [CustomEditor(typeof(PlayerConfig))]
    [CanEditMultipleObjects]
    public class PlayerConfigEditor : Editor
    {
        static readonly string[] Dash = { "canDash", "dashSpeed", "dashDuration", "dashCooldown", "dashInvincible" };
        static readonly (string title, string[] fields)[] Platformer =
        {
            ("Movement", new[] { "moveSpeed", "acceleration", "deceleration", "airControl" }),
            ("Jump", new[] { "jumpHeight", "maxJumps", "coyoteTime", "jumpBuffer", "jumpCutMultiplier", "jumpWithUp" }),
            ("Gravity", new[] { "gravityScale", "fallGravityMultiplier", "maxFallSpeed" }),
            ("Walls", new[] { "wallSlide", "wallSlideSpeed", "wallJump", "wallJumpVelocity" }),
            ("Dash", Dash),
            ("Hurt", new[] { "hurtControlLock" }),
        };
        static readonly (string title, string[] fields)[] TopDown =
        {
            ("Movement", new[] { "moveSpeed", "acceleration", "deceleration", "facing" }),
            ("Dash", Dash),
            ("Hurt", new[] { "hurtControlLock" }),
        };
        static readonly (string title, string[] fields)[] ThirdPerson =
        {
            ("Movement", new[] { "moveSpeed", "runSpeed", "acceleration", "deceleration", "turnSmoothTime", "airControl" }),
            ("Jump", new[] { "jumpHeight", "maxJumps", "coyoteTime", "jumpBuffer", "gravity", "terminalVelocity" }),
            ("Camera", new[] { "lookSensitivity", "invertLookY" }),
            ("Hurt", new[] { "hurtControlLock" }),
        };

        public override void OnInspectorGUI()
        {
            JamBehaviourEditor.DrawJamHeader(typeof(PlayerConfig));
            EditorGUILayout.HelpBox("The feel of a player. Tweak these while playing: changes to this asset are kept when you stop. "
                                  + "Drag a different config into the player's 'Config' field to swap presets.", MessageType.None);
            serializedObject.Update();
            var kindProp = serializedObject.FindProperty("kind");
            EditorGUILayout.PropertyField(kindProp);
            var kind = (PlayerConfigKind)kindProp.enumValueIndex;

            var sections = kind == PlayerConfigKind.Platformer2D ? Platformer : kind == PlayerConfigKind.TopDown2D ? TopDown : ThirdPerson;
            foreach (var (title, fields) in sections)
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField(title, EditorStyles.boldLabel);
                foreach (var p in fields) Draw(p, kind);
            }
            serializedObject.ApplyModifiedProperties();

            EditorGUILayout.Space();
            if (GUILayout.Button("Reset to defaults for " + kind))
            {
                foreach (var t in targets)
                {
                    Undo.RecordObject(t, "Reset PlayerConfig");
                    ((PlayerConfig)t).ApplyDefaults(kind);
                    EditorUtility.SetDirty(t);
                }
            }
        }

        void Draw(string name, PlayerConfigKind kind)
        {
            var p = serializedObject.FindProperty(name);
            if (p == null) return;
            if (kind == PlayerConfigKind.ThirdPerson3D && name == "moveSpeed")
                EditorGUILayout.PropertyField(p, new GUIContent("Walk Speed", p.tooltip));
            else
                EditorGUILayout.PropertyField(p, true);
        }
    }
}
