using Assets.Scripts.Combat.Interfaces;
using System;
using System.Collections.Generic;
namespace Assets.Scripts.Traps.Detectors
{
    public interface ITrapDetector
    {
        event Action<IReadOnlyList<ITargetable>> OnDetection;
        void Enable();
        void Disable();
    }
}