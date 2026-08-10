using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.ShaderKeywordFilter;
using UnityEngine;
using UnityEngine.UIElements;
using ActionTree.Data;

namespace ActionTree.Editor
{
	public class ActionTreeCanvas : VisualElement
	{
		public GraphViewport Viewport { get; }

		public GridLayer GridLayer { get; }

		//public ConnectionLayer ConnectionLayer { get; }

		public EdgeLayer EdgeLayer { get; }

		public NodeLayer NodeLayer { get; }


		public OverlayLayer OverlayLayer { get; }
		public GraphController Controller { get; }

		public GraphNodeManager NodeManager { get; }

		public GraphInteractionManager InteractionManager { get; }

		public GraphInspector Inspector { get; }

		public ActionTreeAsset Asset { get; private set; }

		public VisualElement ContentLayer { get; }
		public GraphSelectionManager SelectionManager { get; } = new();

		public ActionTreeCanvas(GraphInspector inspector)
		{
			style.flexGrow = 1;
			Inspector = inspector;
			Inspector.InspectorChanged += () =>
			{
				RefreshGraph();

				EditorUtility.SetDirty(Asset);

				AssetDatabase.SaveAssets();
			};
			// ========= Core =========

			Viewport = new GraphViewport();
			NodeManager = new GraphNodeManager();
			InteractionManager = new GraphInteractionManager();

			// ========= Controller =========

			Controller = new GraphController(
			this,
			Viewport,
			InteractionManager,
			Inspector);

			// ========= Layers =========

			GridLayer = new GridLayer(Viewport);

			ContentLayer = new VisualElement();

			ContentLayer.style.position = Position.Absolute;
			ContentLayer.style.left = 0;
			ContentLayer.style.top = 0;
			ContentLayer.style.right = 0;
			ContentLayer.style.bottom = 0;

			EdgeLayer = new EdgeLayer(Viewport, NodeManager);
			//ConnectionLayer = new ConnectionLayer(this);
			NodeLayer = new NodeLayer(Viewport, NodeManager);
			OverlayLayer = new OverlayLayer( Viewport, InteractionManager);

			// Canvas
			Add(GridLayer);
			Add(ContentLayer);

			// Content
			ContentLayer.Add(EdgeLayer);
			//ContentLayer.Add(ConnectionLayer);
			ContentLayer.Add(NodeLayer);
			ContentLayer.Add(OverlayLayer);

			RegisterCallback<WheelEvent>(OnWheel);
			RegisterCallback<MouseDownEvent>(OnMouseDown);
			RegisterCallback<MouseUpEvent>(OnMouseUp);
			RegisterCallback<MouseMoveEvent>(OnMouseMove);
		}

		public void RefreshGraph()
		{
			GridLayer.Refresh();
			EdgeLayer.Refresh();
			NodeLayer.Refresh();
			//ConnectionLayer.MarkDirtyRepaint();
			OverlayLayer.Refresh();

			foreach (BaseNodeView node in NodeLayer.NodeViews)
			{
				node.IsHoverParent =
					node == InteractionManager.HoverParent;
			}
		}
		public void Tick()
		{
			RefreshGraph();
		}
		private void OnMouseDown(MouseDownEvent evt)
		{
			if (evt.button == 1)
			{
				Vector2 world = Viewport.ScreenToWorld(evt.localMousePosition);

				BaseNodeView node = NodeLayer.HitTest(world);

				if (node != null)
					ShowNodeContextMenu(node);
				else
					ShowCanvasContextMenu(evt.localMousePosition);

				evt.StopPropagation();
				return;
			}

			Controller.OnMouseDown(evt);
		}

		private void OnMouseUp(MouseUpEvent evt)
		{
			Controller.OnMouseUp(evt);
		}

		private void OnMouseMove(MouseMoveEvent evt)
		{
			Controller.OnMouseMove(evt);
		}

		private void OnWheel(WheelEvent evt)
		{
			Controller.OnWheel(evt);
		}
		public void Load(ActionTreeAsset asset)
		{
			Asset = asset;

			NodeLayer.ClearNodes();

			foreach (BaseNodeData node in asset.Nodes)
			{
				BaseNodeView view = CreateView(node);

				if (view == null)
					continue;

				NodeLayer.AddNode(view);
			}

			RefreshGraph();
		}

		private BaseNodeView CreateView(BaseNodeData node)
		{
			if (node is RootNodeData root)
				return new RootNodeView(root);

			if (node is ActionNodeData action)
				return new ActionNodeView(action);

			if (node is ConditionNodeData condition)
				return new ConditionNodeView(condition);

			if (node is SequenceNodeData sequence)
				return new SequenceNodeView(sequence);

			if (node is SelectorNodeData selector)
				return new SelectorNodeView(selector);
			return null;
		}
		public void ShowCanvasContextMenu(Vector2 position)
		{
			GenericMenu menu = new GenericMenu();

			menu.AddItem(
				new GUIContent("Create/Action"),
				false,
				() => CreateAction(position));

			menu.AddItem(
				new GUIContent("Create/Condition"),
				false,
				() => CreateCondition(position));

			menu.AddItem(
				new GUIContent("Create/Sequence"),
				false,
				() => CreateSequence(position));

			menu.AddItem(
				new GUIContent("Create/Selector"),
				false,
				() => CreateSelector(position));

			menu.ShowAsContext();
		}

		public void ShowNodeContextMenu(BaseNodeView node)
		{
			GenericMenu menu = new GenericMenu();

			//menu.AddItem(
			//	new GUIContent("Add Child"),
			//	false,
			//	() => InteractionManager.BeginConnection(node));
			menu.AddSeparator("");

			menu.AddItem(
				new GUIContent("Delete"),
				false,
				() => DeleteSelection());
			menu.ShowAsContext();
		}
		private void CreateAction(Vector2 mousePosition)
		{
			Vector2 worldPos = Viewport.ScreenToWorld(mousePosition);

			ActionNodeData node = new ActionNodeData(worldPos);

			Asset.Nodes.Add(node);

			BaseNodeView view = CreateView(node);

			NodeLayer.AddNode(view);

			RefreshGraph();

			EditorUtility.SetDirty(Asset);
		}
		private void CreateCondition(Vector2 mousePosition)
		{
			Vector2 worldPos = Viewport.ScreenToWorld(mousePosition);

			ConditionNodeData node = new ConditionNodeData(worldPos);

			Asset.Nodes.Add(node);

			BaseNodeView view = CreateView(node);

			NodeLayer.AddNode(view);

			RefreshGraph();

			EditorUtility.SetDirty(Asset);
		}
		private void CreateSequence(Vector2 mousePosition)
		{
			Vector2 worldPos = Viewport.ScreenToWorld(mousePosition);

			SequenceNodeData node = new SequenceNodeData(worldPos);

			Asset.Nodes.Add(node);

			BaseNodeView view = CreateView(node);

			NodeLayer.AddNode(view);

			RefreshGraph();

			EditorUtility.SetDirty(Asset);
		}
		private void CreateSelector(Vector2 mousePosition)
		{
			Vector2 worldPos = Viewport.ScreenToWorld(mousePosition);

			SelectorNodeData node = new SelectorNodeData(worldPos);

			Asset.Nodes.Add(node);

			BaseNodeView view = CreateView(node);

			NodeLayer.AddNode(view);

			RefreshGraph();

			EditorUtility.SetDirty(Asset);
		}

		public void DeleteSelection()
		{
			List<BaseNodeView> nodes = new(SelectionManager.SelectedNodes);

			foreach (BaseNodeView view in nodes)
			{
				BaseNodeData node = view.Node;

				foreach (BaseNodeData parent in Asset.Nodes)
				{
					parent.RemoveChild(node);
				}

				Asset.Nodes.Remove(node);

				NodeLayer.RemoveNode(view);
			}

			SelectionManager.Clear();

			Inspector.Clear();

			RefreshGraph();

			EditorUtility.SetDirty(Asset);
		}
	}
}