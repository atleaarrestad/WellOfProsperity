using Assets.Scripts.Traps.Conditions;
using Assets.Scripts.Traps.Detectors;
using Assets.Scripts.Traps.Effects;
using Assets.Scripts.Traps.TargetSelectors;
using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Trap))]
public class TrapEditor : Editor
{
    private bool _showDetectors = false;
    private bool _showEffects = false;
    private bool _showConditions = false;
    private bool _showTargetSelector = false;

    private Type[] _detectorTypes;
    private string[] _detectorNames;
    private int _detectorSelected;

    private Type[] _effectTypes;
    private string[] _effectNames;
    private int _effectSelected;

    private Type[] _conditionTypes;
    private string[] _conditionNames;
    private int _conditionSelected;

    private Type[] _selectorTypes;
    private string[] _selectorNames;
    private int _selectorSelected;

    private void OnEnable()
    {
        CacheTypes();
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Trap Builder", EditorStyles.boldLabel);

        var trap = (Trap)target;
        var go = trap.gameObject;

        if (_detectorTypes == null || _effectTypes == null || _conditionTypes == null || _selectorTypes == null)
            CacheTypes();

        EditorGUILayout.Space(6);

        _showDetectors = DrawSection<ITrapDetector>(
            title: "Detectors",
            go: go,
            show: _showDetectors,
            types: _detectorTypes,
            names: _detectorNames,
            selectedIndex: ref _detectorSelected,
            allowDuplicates: false
        );

        _showEffects = DrawSection<ITrapEffect>(
            title: "Effects",
            go: go,
            show: _showEffects,
            types: _effectTypes,
            names: _effectNames,
            selectedIndex: ref _effectSelected,
            allowDuplicates: true
        );

        _showConditions = DrawSection<ITrapCondition>(
            title: "Conditions",
            go: go,
            show: _showConditions,
            types: _conditionTypes,
            names: _conditionNames,
            selectedIndex: ref _conditionSelected,
            allowDuplicates: true
        );

        _showTargetSelector = DrawSingleSection<ITargetSelector>(
            title: "Target Selector",
            go: go,
            show: _showTargetSelector,
            types: _selectorTypes,
            names: _selectorNames,
            selectedIndex: ref _selectorSelected
        );
    }

    private void CacheTypes()
    {
        _detectorTypes = FindMonoBehaviourTypesImplementing(typeof(ITrapDetector));
        _detectorNames = _detectorTypes.Select(t => t.Name).ToArray();
        _detectorSelected = ClampIndex(_detectorSelected, _detectorTypes.Length);

        _effectTypes = FindMonoBehaviourTypesImplementing(typeof(ITrapEffect));
        _effectNames = _effectTypes.Select(t => t.Name).ToArray();
        _effectSelected = ClampIndex(_effectSelected, _effectTypes.Length);

        _conditionTypes = FindMonoBehaviourTypesImplementing(typeof(ITrapCondition));
        _conditionNames = _conditionTypes.Select(t => t.Name).ToArray();
        _conditionSelected = ClampIndex(_conditionSelected, _conditionTypes.Length);

        _selectorTypes = FindMonoBehaviourTypesImplementing(typeof(ITargetSelector));
        _selectorNames = _selectorTypes.Select(t => t.Name).ToArray();
        _selectorSelected = ClampIndex(_selectorSelected, _selectorTypes.Length);
    }

    private static Type[] FindMonoBehaviourTypesImplementing(Type interfaceType)
    {
        return TypeCache.GetTypesDerivedFrom<MonoBehaviour>()
            .Where(t => !t.IsAbstract && interfaceType.IsAssignableFrom(t))
            .OrderBy(t => t.Name)
            .ToArray();
    }

    private static int ClampIndex(int index, int length)
    {
        if (length <= 0) return 0;
        if (index < 0) return 0;
        if (index >= length) return length - 1;
        return index;
    }

    private bool DrawSection<TInterface>(
        string title,
        GameObject go,
        bool show,
        Type[] types,
        string[] names,
        ref int selectedIndex,
        bool allowDuplicates)
    {
        show = EditorGUILayout.BeginFoldoutHeaderGroup(show, title);
        if (!show)
        {
            EditorGUILayout.EndFoldoutHeaderGroup();
            return show;
        }

        EditorGUILayout.Space(4);

        var attached = go.GetComponents<MonoBehaviour>()
            .Where(c => c is TInterface)
            .ToArray();

        EditorGUILayout.LabelField($"Attached {title}: {attached.Length}");
        using (new EditorGUI.IndentLevelScope())
        {
            foreach (var comp in attached)
                EditorGUILayout.ObjectField(comp.GetType().Name, comp, typeof(MonoBehaviour), true);
        }

        EditorGUILayout.Space(6);

        if (types == null || types.Length == 0)
        {
            EditorGUILayout.HelpBox(
                $"No {title.ToLower()} components found. Create a MonoBehaviour that implements {typeof(TInterface).Name}.",
                MessageType.Info
            );
            EditorGUILayout.EndFoldoutHeaderGroup();
            return show;
        }

        selectedIndex = ClampIndex(selectedIndex, types.Length);
        selectedIndex = EditorGUILayout.Popup($"Add {title.TrimEnd('s')}", selectedIndex, names);

        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("Add"))
            {
                var typeToAdd = types[selectedIndex];

                if (!allowDuplicates && go.GetComponent(typeToAdd) != null)
                {
                    Debug.LogWarning($"{typeToAdd.Name} already exists on {go.name}.");
                }
                else
                {
                    Undo.AddComponent(go, typeToAdd);
                    EditorUtility.SetDirty(go);
                }
            }

            if (GUILayout.Button($"Remove All {title}"))
            {
                foreach (var mb in attached)
                    Undo.DestroyObjectImmediate(mb);
            }

            if (GUILayout.Button("Refresh List"))
            {
                CacheTypes();
            }
        }

        EditorGUILayout.EndFoldoutHeaderGroup();
        EditorGUILayout.Space(6);
        return show;
    }

    // Single-slot (0 or 1) version for Target Selector
    private bool DrawSingleSection<TInterface>(
        string title,
        GameObject go,
        bool show,
        Type[] types,
        string[] names,
        ref int selectedIndex)
    {
        show = EditorGUILayout.BeginFoldoutHeaderGroup(show, title);
        if (!show)
        {
            EditorGUILayout.EndFoldoutHeaderGroup();
            return show;
        }

        EditorGUILayout.Space(4);

        // Get current selector (0 or 1)
        var current = go.GetComponents<MonoBehaviour>()
            .FirstOrDefault(c => c is TInterface);

        EditorGUILayout.LabelField("Current:");
        using (new EditorGUI.IndentLevelScope())
        {
            EditorGUILayout.ObjectField(current ? current.GetType().Name : "None",
                current, typeof(MonoBehaviour), true);
        }

        EditorGUILayout.Space(6);

        if (types == null || types.Length == 0)
        {
            EditorGUILayout.HelpBox(
                $"No {title.ToLower()} components found. Create a MonoBehaviour that implements {typeof(TInterface).Name}.",
                MessageType.Info
            );
            EditorGUILayout.EndFoldoutHeaderGroup();
            return show;
        }

        selectedIndex = ClampIndex(selectedIndex, types.Length);
        selectedIndex = EditorGUILayout.Popup("Select", selectedIndex, names);

        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button(current ? "Replace" : "Add"))
            {
                var typeToAdd = types[selectedIndex];

                // If it’s already the same type, do nothing
                if (current != null && current.GetType() == typeToAdd)
                {
                    Debug.LogWarning($"{typeToAdd.Name} is already set as the {title} on {go.name}.");
                }
                else
                {
                    // Remove existing selector (if any)
                    if (current != null)
                        Undo.DestroyObjectImmediate(current);

                    Undo.AddComponent(go, typeToAdd);
                    EditorUtility.SetDirty(go);
                }
            }

            using (new EditorGUI.DisabledScope(current == null))
            {
                if (GUILayout.Button("Remove"))
                {
                    if (current != null)
                        Undo.DestroyObjectImmediate(current);
                }
            }

            if (GUILayout.Button("Refresh List"))
            {
                CacheTypes();
            }
        }

        EditorGUILayout.EndFoldoutHeaderGroup();
        EditorGUILayout.Space(6);
        return show;
    }
}
