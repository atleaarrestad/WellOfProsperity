using Assets.Scripts.Combat.Interfaces;
using System.Collections.Generic;
namespace Assets.Scripts.Traps.Conditions
{
    public interface ITrapCondition
    {
        bool CanActivate(TrapContext context, IReadOnlyList<ITargetable> targets);
        void OnFired();
    }
}