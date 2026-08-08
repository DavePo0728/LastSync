using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

[InitializeOnLoad]
public static class UiGraphicSelectionReloadGuard
{
    static UiGraphicSelectionReloadGuard()
    {
        AssemblyReloadEvents.beforeAssemblyReload -= OnBeforeAssemblyReload;
        AssemblyReloadEvents.beforeAssemblyReload += OnBeforeAssemblyReload;

        EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }

    private static void OnBeforeAssemblyReload()
    {
        DeselectGraphicObject();
    }

    private static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.ExitingEditMode)
        {
            DeselectGraphicObject();
        }
    }

    private static void DeselectGraphicObject()
    {
        GameObject selectedObject = Selection.activeGameObject;

        if (selectedObject == null)
        {
            return;
        }

        // Image、RawImage、Text、MaskableGraphic 等都繼承自 Graphic。
        if (selectedObject.GetComponent<Graphic>() == null)
        {
            return;
        }

        Selection.objects = Array.Empty<UnityEngine.Object>();
    }
}