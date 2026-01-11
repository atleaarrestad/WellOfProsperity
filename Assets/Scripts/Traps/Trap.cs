using Assets.Scripts.Combat.Interfaces;
using Assets.Scripts.Traps.Conditions;
using Assets.Scripts.Traps.Detectors;
using Assets.Scripts.Traps.Effects;
using Assets.Scripts.Traps.TargetSelectors;
using System;
using System.Collections.Generic;
using UnityEngine;

public class Trap : MonoBehaviour
{
    [Header("Context")]
    [SerializeField] private Transform firePointTransform;
    [SerializeField] private LayerMask enemyMask;

    private ITrapDetector[] detectors;
    private ITrapCondition[] contitions;
    private ITrapEffect[] effects;
    private ITargetSelector selector;

    private void Awake()
    {
        detectors = GetComponents<ITrapDetector>();
        contitions = GetComponents<ITrapCondition>();
        effects = GetComponents<ITrapEffect>();
        selector = GetComponent<ITargetSelector>();

        firePointTransform ??= transform;
    }

    private void OnEnable()
    {
        detectors ??= GetComponents<ITrapDetector>();

        foreach (var detector in detectors)
        {
            detector.OnDetection += HandleDetection;
            detector.Enable();
        }
    }

    private void OnDisable()
    {
        if (detectors == null)
        {
            return;
        }

        foreach (var detector in detectors)
        {
            detector.OnDetection -= HandleDetection;
            detector.Disable();
        }
    }

    private void HandleDetection(IReadOnlyList<ITargetable> candidates)
    {
        var context = BuildContext();
        var targets = SelectTargets(context, candidates);

        foreach (var condition in contitions)
        {
            if (!condition.CanActivate(context, targets))
            {
                return;
            }
        }

        foreach (var effect in effects)
        {
            effect.Execute(context, targets);
        }

        foreach (var condition in contitions)
        { 
            condition.OnFired();
        }
    }

    private TrapContext BuildContext()
    {
        return new TrapContext
        {
            trapTransform = transform,
            firePoint = firePointTransform.position,
            enemyMask = enemyMask
        };
    }

    private IReadOnlyList<ITargetable> SelectTargets(TrapContext context, IReadOnlyList<ITargetable> candidates)
    {
        if (selector == null)
        {
            return candidates;
        }

        var selected = selector.Select(context, candidates);

        return selected ?? Array.Empty<ITargetable>();
    }

#if UNITY_EDITOR
    private void OnValidate()
    {

    }
#endif
}
