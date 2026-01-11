using Assets.Scripts.Combat.Interfaces;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Traps.Conditions
{
    public class CooldownCondition : MonoBehaviour, ITrapCondition
    {
        [SerializeField] private float cooldownSeconds = 1f;

        private float nextReadyTime;

        public bool CanActivate(TrapContext context, IReadOnlyList<ITargetable> targets)
        {
            return Time.time >= nextReadyTime;
        }

        public void OnFired()
        {
            nextReadyTime = Time.time + Mathf.Max(0f, cooldownSeconds);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (cooldownSeconds < 0f) cooldownSeconds = 0f;
        }
#endif
    }
}
