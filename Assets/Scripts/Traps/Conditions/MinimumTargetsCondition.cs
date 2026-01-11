using Assets.Scripts.Combat.Interfaces;
using System.Collections.Generic;
using UnityEngine;
namespace Assets.Scripts.Traps.Conditions
{
    public class MinimumTargetsCondition : MonoBehaviour, ITrapCondition
    {
        public bool CanActivate(TrapContext context, IReadOnlyList<ITargetable> targets)
        {
            throw new System.NotImplementedException();
        }

        public void OnFired()
        {
            throw new System.NotImplementedException();
        }

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}
