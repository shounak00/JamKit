using System.Collections.Generic;
using UnityEngine;

namespace JamKit
{
    /// <summary>Sets Animator parameters only if they exist, so any controller (or none) works without errors.</summary>
    public class JamAnimatorParams
    {
        readonly Animator animator;
        HashSet<string> names;
        RuntimeAnimatorController cachedFor;

        public JamAnimatorParams(Animator animator) { this.animator = animator; }

        public bool Valid => animator != null && animator.isActiveAndEnabled && animator.runtimeAnimatorController != null;

        bool Has(string p)
        {
            if (!Valid) return false;
            if (names == null || cachedFor != animator.runtimeAnimatorController)
            {
                names = new HashSet<string>();
                foreach (var param in animator.parameters) names.Add(param.name);
                cachedFor = animator.runtimeAnimatorController;
            }
            return names.Contains(p);
        }

        public void Float(string p, float v) { if (Has(p)) animator.SetFloat(p, v); }
        public void Float(string p, float v, float damp) { if (Has(p)) animator.SetFloat(p, v, damp, Time.deltaTime); }
        public void Bool(string p, bool v) { if (Has(p)) animator.SetBool(p, v); }
        public void Trigger(string p) { if (Has(p)) animator.SetTrigger(p); }
    }
}
