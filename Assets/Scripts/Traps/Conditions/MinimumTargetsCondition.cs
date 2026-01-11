using Assets.Scripts.Combat.Interfaces;
using System.Collections.Generic;
using UnityEngine;
namespace Assets.Scripts.Traps.Conditions
{
    public class MinimumTargetsCondition : MonoBehaviour, ITrapCondition
    {
        [SerializeField] private int minimumTargets = 1;
        public bool CanActivate(TrapContext context, IReadOnlyList<ITargetable> targets)
            => targets != null && targets.Count >= minimumTargets;
        

        public void OnFired(){}
    }
}
