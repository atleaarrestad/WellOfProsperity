using Assets.Scripts.Combat.Interfaces;
using System.Collections.Generic;
using UnityEngine;
namespace Assets.Scripts.Traps.TargetSelectors
{
    public class ClosestTargetSelector : MonoBehaviour, ITargetSelector
    {
        private readonly List<ITargetable> selectedCandidates = new(1);
        public IReadOnlyList<ITargetable> Select(TrapContext trapContext, IReadOnlyList<ITargetable> candidates)
        {
            if (candidates == null || candidates.Count == 0)
            {
                return System.Array.Empty<ITargetable>();
            }

            var bestCandidate = candidates[0];
            var bestDistance = float.PositiveInfinity;
            var origin = transform.position;

            for (int i = 0; i < candidates.Count; i++)
            {
                var candidate = candidates[i];
                if (candidate is not Component component)
                {
                    continue;
                }

                var distance = (component.transform.position - origin).sqrMagnitude;
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    bestCandidate = candidate;
                }
            }

            selectedCandidates.Clear();
            selectedCandidates.Add(bestCandidate);
            return selectedCandidates;
        }

    }
}