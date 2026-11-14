using System.Collections.Generic;
using ActionTree.Editor;
using UnityEngine;
using UnityEngine.UIElements;

public class FixedMapPreviewCanvas : VisualElement
{
	private enum NameLayer { None, Structure, Bridge, Prefab }

	private struct Entry
	{
		public Vector2 Center;
		public Vector2 Size;
		public string Name;
		public Color Color;
		public NameLayer Layer;
	}

	private readonly GraphViewport viewport = new();
	private readonly GraphInteractionManager interaction = new();
	private readonly GridLayer grid;
	private readonly VisualElement objects = new();
	private readonly VisualElement textLayer = new();
	private readonly List<Label> labels = new();
	private bool showStructures = true, showBridges = true, showPrefabs = true;
	private readonly List<Entry> entries = new();
	private readonly List<Entry> pending = new();
	private static readonly Color RoomColor = new(0.2f, 0.5f, 1f);
	private static readonly Color BridgeColor = new(0.2f, 0.85f, 0.35f);
	private static readonly Color PrefabColor = new(1f, 0.25f, 0.25f);
	private static readonly Color WallColor = new(0.8f, 0.8f, 0.8f);
	private static readonly Color PillarColor = new(0.65f, 0.4f, 0.95f);
	private static readonly Color DoorColor = new(1f, 0.85f, 0.2f);
	private static readonly Color TrapColor = new(1f, 0.45f, 0.1f);

	public FixedMapPreviewCanvas()
	{
		style.flexGrow = 1;
		style.flexBasis = 0;
		style.minHeight = 0;
		style.overflow = Overflow.Hidden;
		style.backgroundColor = new Color(0.12f, 0.12f, 0.12f);
		// Integer map coordinates are cell centers. Draw boundaries half a cell
		// away, using the same viewport so the offset scales and pans with objects.
		grid = new GridLayer(viewport, Vector2.one * (GridLayer.MinorGrid * 0.5f));
		Add(grid);
		objects.pickingMode = PickingMode.Ignore;
		objects.StretchToParentSize();
		Add(objects);
		textLayer.pickingMode = PickingMode.Ignore;
		textLayer.StretchToParentSize();
		Add(textLayer);
		RegisterCallback<WheelEvent>(evt =>
		{
			viewport.ZoomAt(evt.localMousePosition, evt.delta.y > 0 ? -0.1f : 0.1f);
			RefreshView();
			evt.StopPropagation();
		});
		RegisterCallback<MouseDownEvent>(evt =>
		{
			if (evt.button != 2) return;
			interaction.BeginPan(evt.localMousePosition);
			this.CaptureMouse();
			evt.StopPropagation();
		});
		RegisterCallback<MouseMoveEvent>(evt =>
		{
			if (!interaction.IsPanning) return;
			viewport.Move(interaction.UpdatePan(evt.localMousePosition));
			RefreshView();
			evt.StopPropagation();
		});
		RegisterCallback<MouseUpEvent>(evt =>
		{
			if (evt.button != 2) return;
			interaction.EndPan();
			this.ReleaseMouse();
			evt.StopPropagation();
		});
		RegisterCallback<MouseCaptureOutEvent>(_ => interaction.EndPan());
	}

	public void RefreshData(FixedMapData map)
	{
		pending.Clear();
		if (map != null)
		{
			if (map.RoomNodes != null)
				foreach (var node in map.RoomNodes)
					if (node != null) AddStructure(node.Data, node.Position, Direction.Up, RoomColor, NameLayer.Structure);
			if (map.BridgeNodes != null)
				foreach (var node in map.BridgeNodes)
					if (node != null && System.Enum.IsDefined(typeof(Direction), node.Direction))
						AddStructure(node.Data, node.Position, node.Direction, BridgeColor, NameLayer.Bridge);
			if (map.PrefabNodes != null)
				foreach (var node in map.PrefabNodes)
				{
					if (node?.Prefab == null) continue;
					Vector3 position = node.UsePrefabTransform ? node.Prefab.transform.localPosition : node.Position;
					// Fixed one-cell marker; retain the exact floating-point X/Z position.
					AddEntry(new Vector2(position.x, position.z), Vector2.one, node.Prefab.name, PrefabColor, NameLayer.Prefab);
				}
		}
		bool changed = pending.Count != entries.Count;
		for (int i = 0; !changed && i < pending.Count; i++)
			changed = !pending[i].Equals(entries[i]);
		if (!changed) return;
		entries.Clear();
		entries.AddRange(pending);
		objects.Clear();
		textLayer.Clear();
		labels.Clear();
		foreach (var entry in entries)
		{
			var box = new VisualElement { pickingMode = PickingMode.Ignore };
			box.style.position = Position.Absolute;
			Color fill = entry.Color;
			fill.a = string.IsNullOrEmpty(entry.Name) ? 0.75f : 0.25f;
			box.style.backgroundColor = fill;
			box.style.borderTopColor = box.style.borderBottomColor = entry.Color;
			box.style.borderLeftColor = box.style.borderRightColor = entry.Color;
			box.style.borderTopWidth = box.style.borderBottomWidth = 1;
			box.style.borderLeftWidth = box.style.borderRightWidth = 1;
			Label label = null;
			if (!string.IsNullOrEmpty(entry.Name))
			{
				label = new Label(entry.Name) { pickingMode = PickingMode.Ignore };
				label.style.position = Position.Absolute;
				label.style.unityTextAlign = TextAnchor.MiddleCenter;
				label.style.color = Color.white;
				label.style.fontSize = 12;
				label.style.marginLeft = label.style.marginRight = 0;
				textLayer.Add(label);
			}
			labels.Add(label);
			objects.Add(box);
		}
		RefreshView();
	}

	private void AddStructure(StructureData data, Vector2 position, Direction direction, Color color, NameLayer layer)
	{
		if (data == null) return;
		bool sideways = direction == Direction.Left || direction == Direction.Right;
		Vector2 size = sideways ? new Vector2(data.Size.y, data.Size.x) : (Vector2)data.Size;
		AddEntry(position, size, string.IsNullOrEmpty(data.StructureID) ? data.name : data.StructureID, color, layer);
		if (data.Structures == null) return;
		// Same cell-center pivot as StructureSpawner. Doors are already present
		// in Structures; reading Doors again would draw each doorway twice.
		Vector2 pivot = ((Vector2)data.Size - Vector2.one) * 0.5f;
		foreach (var part in data.Structures)
		{
			if (part == null) continue;
			Vector2 center = (part.Position + part.End) * 0.5f;
			Vector2 partSize = new Vector2(
				Mathf.Abs(part.End.x - part.Position.x) + 1f,
				Mathf.Abs(part.End.y - part.Position.y) + 1f);
			Color partColor;
			switch (part.Type)
			{
				case CellType.Wall:
					partColor = WallColor;
					partSize = part.Orientation == Orientation.Horizontal
						? new Vector2(partSize.x, 1f) : new Vector2(1f, partSize.y);
					break;
				case CellType.Pillar:
					partColor = PillarColor;
					center = part.Position;
					partSize = Vector2.one;
					break;
				case CellType.Door:
					partColor = DoorColor;
					break;
				case CellType.TrapFloor:
					partColor = TrapColor;
					break;
				default:
					continue;
			}
			Vector2 local = center - pivot;
			// Positive Unity Y rotation maps (X, Z) to (Z, -X).
			Vector2 rotated = direction switch
			{
				Direction.Right => new Vector2(local.y, -local.x),
				Direction.Down => -local,
				Direction.Left => new Vector2(-local.y, local.x),
				_ => local
			};
			if (sideways) partSize = new Vector2(partSize.y, partSize.x);
			AddEntry(position + rotated, partSize, null, partColor);
		}
	}

	private void AddEntry(Vector2 center, Vector2 size, string name, Color color, NameLayer layer = NameLayer.None)
	{
		if (!IsFinite(center.x) || !IsFinite(center.y) ||
			!IsFinite(size.x) || !IsFinite(size.y) || size.x <= 0 || size.y <= 0) return;
		pending.Add(new Entry { Center = center, Size = size, Name = name, Color = color, Layer = layer });
	}

	public void SetNameVisibility(bool structures, bool bridges, bool prefabs)
	{
		showStructures = structures;
		showBridges = bridges;
		showPrefabs = prefabs;
		RefreshView();
	}

	private static bool IsFinite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);

	// StructureSpawner centers its Size cells around Position; positive Z points up.
	private static Vector2 ToCanvas(Vector2 position) => new Vector2(position.x, -position.y) * GridLayer.MinorGrid;

	public void CenterMap()
	{
		Vector2 min = Vector2.zero, max = Vector2.zero;
		for (int i = 0; i < entries.Count; i++)
		{
			var entry = entries[i];
			Vector2 lower = entry.Center - entry.Size * 0.5f;
			Vector2 upper = entry.Center + entry.Size * 0.5f;
			min = i == 0 ? lower : Vector2.Min(min, lower);
			max = i == 0 ? upper : Vector2.Max(max, upper);
		}
		viewport.SetOffset(contentRect.size * 0.5f - ToCanvas((min + max) * 0.5f) * viewport.Zoom);
		RefreshView();
	}

	private void RefreshView()
	{
		grid.Refresh();
		for (int i = 0; i < entries.Count; i++)
		{
			var entry = entries[i];
			Vector2 size = entry.Size * GridLayer.MinorGrid * viewport.Zoom;
			Vector2 corner = viewport.WorldToScreen(ToCanvas(entry.Center)) - size * 0.5f;
			VisualElement box = objects[i];
			box.style.left = corner.x;
			box.style.top = corner.y;
			box.style.width = size.x;
			box.style.height = size.y;
			// Allow names to remain readable even on a one-cell prefab marker.
			Label label = labels[i];
			if (label == null) continue;
			bool visible = entry.Layer switch
			{
				NameLayer.Structure => showStructures,
				NameLayer.Bridge => showBridges,
				NameLayer.Prefab => showPrefabs,
				_ => false
			};
			label.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
			float labelWidth = Mathf.Max(size.x, entry.Name.Length * 12f + 16f);
			label.style.left = corner.x + (size.x - labelWidth) * 0.5f;
			label.style.top = corner.y;
			label.style.width = labelWidth;
			label.style.height = size.y;
		}
	}
}
