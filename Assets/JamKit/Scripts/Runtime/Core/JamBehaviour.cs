using System;
using UnityEngine;

namespace JamKit
{
    /// <summary>One-line, plain-English help shown at the top of a JamKit component's Inspector.</summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public sealed class JamHelpAttribute : Attribute
    {
        public readonly string Text;
        public JamHelpAttribute(string text) { Text = text; }
    }

    /// <summary>Base class of every JamKit component. Gives them the JamKit Inspector header and help box.</summary>
    public abstract class JamBehaviour : MonoBehaviour
    {
    }

    /// <summary>Who a damage source or trigger reacts to.</summary>
    public enum JamTargets
    {
        Player,
        Enemies,
        Both
    }
}
