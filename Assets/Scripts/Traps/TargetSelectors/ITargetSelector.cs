using Assets.Scripts.Combat.Interfaces;
using System.Collections.Generic;
namespace Assets.Scripts.Traps.TargetSelectors
{
    public interface ITargetSelector
    {
        IReadOnlyList<ITargetable> Select(TrapContext trapContext, IReadOnlyList<ITargetable> candidates);
    }
}