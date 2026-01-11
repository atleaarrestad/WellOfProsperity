using Assets.Scripts.Combat.Interfaces;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Traps.Conditions
{
    public class FacingCondition : MonoBehaviour, ITrapCondition
    {
        [Tooltip("Half-angle of the allowed cone in degrees. 45 = 90° total.")]
        [SerializeField] private float halfAngleDegrees = 45f;

        [Tooltip("If true, at least one target must be in front. If false, all must be.")]
        [SerializeField] private bool requireAny = true;

        public bool CanActivate(TrapContext context, IReadOnlyList<ITargetable> targets)
        {
            if (targets == null || targets.Count == 0)
            {
                return false;
            }

            var forwardVector = context.trapTransform.right;

            bool anyInCone = false;

            foreach (var target in targets)
            {
                if (target is not Component component)
                {
                    continue;
                }

                var toTarget = ((Vector2)component.transform.position - (Vector2)context.trapTransform.position).normalized;
                var angle = Vector2.Angle(forwardVector, toTarget);

                bool isInsideCone = angle <= halfAngleDegrees;

                if (requireAny && isInsideCone)
                {
                    return true;
                }

                if (!requireAny && !isInsideCone)
                {
                    return false;
                }

                if (isInsideCone)
                {
                    anyInCone = true;
                }
            }

            return requireAny ? anyInCone : true;
        }

        public void OnFired()
        {
            // nothing to do
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            var t = transform;
            Vector2 pos = t.position;
            Vector2 fwd = t.right;

            float a = halfAngleDegrees * Mathf.Deg2Rad;
            Vector2 left = new Vector2(
                Mathf.Cos(a) * fwd.x - Mathf.Sin(a) * fwd.y,
                Mathf.Sin(a) * fwd.x + Mathf.Cos(a) * fwd.y);

            Vector2 right = new Vector2(
                Mathf.Cos(-a) * fwd.x - Mathf.Sin(-a) * fwd.y,
                Mathf.Sin(-a) * fwd.x + Mathf.Cos(-a) * fwd.y);

            Gizmos.DrawLine(pos, pos + left * 3f);
            Gizmos.DrawLine(pos, pos + right * 3f);
        }
#endif
    }
}
