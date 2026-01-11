using Assets.Scripts.Combat.Interfaces;
using System.Collections.Generic;

namespace Assets.Scripts.Traps.Effects
{
    public interface ITrapEffect
    {
        void Execute(TrapContext context, IReadOnlyList<ITargetable> targets);
    }
}