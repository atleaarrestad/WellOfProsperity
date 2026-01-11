using Assets.Scripts.Combat.Interfaces;
using System.Collections.Generic;
using UnityEngine;
namespace Assets.Scripts.Traps.Conditions
{
    public class RequiresTargetsCondition : MonoBehaviour, ITrapCondition
    {
        public bool CanActivate(TrapContext context, IReadOnlyList<ITargetable> targets)
            => targets != null && targets.Count > 0;

        public void OnFired() { }
    }
}