using Assets.Scripts.Combat.Interfaces;
using System.Collections.Generic;
using UnityEngine;
namespace Assets.Scripts.Traps.Effects
{
    public class DamageOverTimeEffect : MonoBehaviour, ITrapEffect
    {
        public void Execute(TrapContext context, IReadOnlyList<ITargetable> targets)
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
