using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class FixedMap2DEditWindow : EditorWindow
{
	[SerializeField] private FixedMapData map;
	private FixedMapPreviewCanvas canvas;
	private Toggle structureNames;
	private Toggle bridgeNames;
	private Toggle prefabNames;
	private static string PreferencePrefix => $"LastSync.FixedMap2DEdit.{Application.dataPath}.Names.";

	public static void Open(FixedMapData asset)
	{
		foreach (var existing in Resources.FindObjectsOfTypeAll<FixedMap2DEditWindow>())
		{
			if (existing.map != asset) continue;
			existing.Show();
			existing.EnsureControls();
			existing.Focus();
			return;
		}

		var window = CreateInstance<FixedMap2DEditWindow>();
		window.map = asset;
		window.titleContent = new GUIContent($"2DEdit - {asset.name}");
		window.minSize = new Vector2(500, 300);
		window.Show();
	}

	public static void RefreshOpenWindows()
	{
		foreach (var window in Resources.FindObjectsOfTypeAll<FixedMap2DEditWindow>())
			window.RefreshPreview();
	}

	private void OnEnable() => Undo.undoRedoPerformed += RefreshPreview;
	private void OnDisable() => Undo.undoRedoPerformed -= RefreshPreview;
	private void OnFocus() => EnsureControls();

	private void EnsureControls()
	{
		if (rootVisualElement.Q<VisualElement>("name-layer-controls") == null)
			CreateGUI();
	}

	private void CreateGUI()
	{
		rootVisualElement.Clear();
		rootVisualElement.style.flexDirection = FlexDirection.Column;
		var toolbar = new UnityEditor.UIElements.Toolbar();
		toolbar.style.flexShrink = 0;
		toolbar.Add(new UnityEditor.UIElements.ToolbarButton(() => canvas.CenterMap()) { text = "Center Map" });
		toolbar.Add(new Label("Read only | Middle mouse: pan | Wheel: zoom | 1 cell = 1 unit | X / Z"));
		rootVisualElement.Add(toolbar);
		var controls = new VisualElement { name = "name-layer-controls" };
		controls.style.flexDirection = FlexDirection.Column;
		controls.style.flexShrink = 0;
		controls.style.minHeight = 90;
		controls.style.paddingLeft = 8;
		controls.style.paddingTop = controls.style.paddingBottom = 4;
		controls.Add(new Label("文字圖層"));
		rootVisualElement.Add(controls);
		structureNames = AddNameToggle(controls, "顯示結構名稱", "Structures");
		bridgeNames = AddNameToggle(controls, "顯示橋名稱", "Bridges");
		prefabNames = AddNameToggle(controls, "顯示預製物件名稱", "Prefabs");
		rootVisualElement.Add(new Label("Walls: light gray | Pillars: purple | Doors: yellow | Traps: orange"));
		canvas = new FixedMapPreviewCanvas();
		rootVisualElement.Add(canvas);
		ApplyNameVisibility();
		RefreshPreview();
		canvas.RegisterCallback<GeometryChangedEvent>(OnInitialLayout);
	}

	private Toggle AddNameToggle(VisualElement parent, string label, string preference)
	{
		var toggle = new Toggle(label);
		toggle.style.height = 22;
		toggle.style.flexShrink = 0;
		toggle.SetValueWithoutNotify(EditorPrefs.GetBool(PreferencePrefix + preference, true));
		toggle.RegisterValueChangedCallback(evt =>
		{
			EditorPrefs.SetBool(PreferencePrefix + preference, evt.newValue);
			// Keep all open map windows consistent with the saved project preference.
			foreach (var window in Resources.FindObjectsOfTypeAll<FixedMap2DEditWindow>())
				window.RestoreNameVisibility();
		});
		parent.Add(toggle);
		return toggle;
	}

	private void RestoreNameVisibility()
	{
		if (structureNames == null || bridgeNames == null || prefabNames == null) return;
		structureNames.SetValueWithoutNotify(EditorPrefs.GetBool(PreferencePrefix + "Structures", true));
		bridgeNames.SetValueWithoutNotify(EditorPrefs.GetBool(PreferencePrefix + "Bridges", true));
		prefabNames.SetValueWithoutNotify(EditorPrefs.GetBool(PreferencePrefix + "Prefabs", true));
		ApplyNameVisibility();
	}

	private void ApplyNameVisibility()
	{
		canvas?.SetNameVisibility(structureNames.value, bridgeNames.value, prefabNames.value);
	}

	private void OnInitialLayout(GeometryChangedEvent evt)
	{
		if (evt.newRect.width <= 0 || evt.newRect.height <= 0) return;
		canvas.CenterMap();
		canvas.UnregisterCallback<GeometryChangedEvent>(OnInitialLayout);
	}

	// Also observes referenced StructureData/Prefab edits and external changes.
	// The canvas only rebuilds when visible data changes, preserving pan and zoom.
	private void OnInspectorUpdate() => RefreshPreview();

	private void RefreshPreview()
	{
		if (map != null) titleContent = new GUIContent($"2DEdit - {map.name}");
		canvas?.RefreshData(map);
	}
}
