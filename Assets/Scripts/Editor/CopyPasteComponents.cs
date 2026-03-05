using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

public static class CopyPasteComponents
{
    private static List<Component> copiedComponents = new();

    // ---------------- COPY ----------------
    [MenuItem("GameObject/Copy Components", false, 0)]
    private static void CopyComponents()
    {
        var go = Selection.activeGameObject;
        if (!go)
        {
            Debug.LogWarning("No GameObject selected.");
            return;
        }

        copiedComponents.Clear();

        foreach (var comp in go.GetComponents<Component>())
        {
            if (comp is Transform) continue;
            copiedComponents.Add(comp);
        }

        Debug.Log($"Copied {copiedComponents.Count} components from {go.name}");
    }

    // ---------------- PASTE ----------------
    [MenuItem("GameObject/Paste Components", false, 1)]
    private static void PasteComponents()
    {
        var go = Selection.activeGameObject;
        if (!go)
        {
            Debug.LogWarning("No GameObject selected.");
            return;
        }

        if (copiedComponents.Count == 0)
        {
            Debug.LogWarning("No components copied.");
            return;
        }

        Undo.RegisterCompleteObjectUndo(go, "Paste Components");

        foreach (var source in copiedComponents)
        {
            if (source == null)
                continue;

            var type = source.GetType();

            // Unity sometimes refuses to add certain component types
            var target = go.AddComponent(type);
            if (target == null)
            {
                Debug.LogWarning($"Could not add component of type {type.Name}");
                continue;
            }

            EditorUtility.CopySerialized(source, target);
        }

        Debug.Log($"Pasted {copiedComponents.Count} components to {go.name}");
    }

    // ---------------- VALIDATION ----------------
    [MenuItem("GameObject/Paste Components", true)]
    private static bool PasteComponents_Validate()
    {
        return Selection.activeGameObject != null && copiedComponents.Count > 0;
    }
}
