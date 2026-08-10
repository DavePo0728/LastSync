using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using ActionTree.Data;
namespace ActionTree.Editor
{
	public class GraphController
	{
		private readonly ActionTreeCanvas canvas;

		private readonly GraphViewport viewport;

		private readonly GraphInteractionManager interaction;
		private readonly GraphInspector inspector;
		public GraphController(
			ActionTreeCanvas canvas,
			GraphViewport viewport,
			GraphInteractionManager interaction,
			GraphInspector inspector)
		{this.inspector = inspector;
			this.canvas = canvas;
			this.viewport = viewport;
			this.interaction = interaction;
			this.inspector = inspector;
		}

		#region Mouse

		public void OnMouseDown(MouseDownEvent evt)
		{

			Vector2 worldMouse = viewport.ScreenToWorld(evt.localMousePosition);


			BaseNodeView node = canvas.NodeLayer.HitTest(worldMouse);
			// 建立父子模式
			//if (interaction.IsCreatingConnection && evt.button == 0)
			//{
			//	if (node != null)
			//	{
			//		BaseNode parent = interaction.ConnectionStart.Node;
			//		BaseNode child = node.Node;

			//		parent.AddChild(child);

			//	}

			//	interaction.EndConnection();

			//	canvas.RefreshGraph();

			//	return;
			//}
			// 右鍵
			if (evt.button == 1)
			{
				if (node != null)
				{
					canvas.ShowNodeContextMenu(node);
				}
				else
				{
					canvas.ShowCanvasContextMenu(evt.localMousePosition);
				}

				return;
			}
			if (evt.button == 2)
			{
				interaction.BeginPan(evt.localMousePosition);

				return;
			}
			// 左鍵
			if (evt.button != 0)
				return;

			if (node != null)
			{
				canvas.SelectionManager.Select(node);

				inspector.Show(node.Node);

				interaction.BeginDrag(node, worldMouse);
			}
			else
			{
				canvas.SelectionManager.Clear();

				inspector.Clear();

				interaction.BeginBoxSelection(evt.localMousePosition);
			}

			canvas.RefreshGraph();
		}

		public void OnMouseMove(MouseMoveEvent evt)
		{
			interaction.UpdateMouse(evt.localMousePosition);
			//=========================
			// 拖動畫布
			//=========================
			if (interaction.IsBoxSelecting)
			{
				interaction.UpdateBoxSelection(evt.localMousePosition);

				SelectNodesInBox();

				canvas.RefreshGraph();

				return;
			}
			if (interaction.IsPanning)
			{
				Vector2 delta = interaction.UpdatePan(evt.localMousePosition);

				canvas.Viewport.Move(delta);

				canvas.RefreshGraph();

				return;
			}
			//=========================
			// 拖曳 Node
			//=========================

			if (!interaction.IsDraggingNode)
				return;

			if (interaction.DragNode == null)
				return;
			
			Vector2 world =
	viewport.ScreenToWorld(evt.localMousePosition);

			Vector2 deltaWorld =
				world - interaction.DragStartMouse;

			interaction.DragNode.Node.Position =
				interaction.DragStartNode + deltaWorld;

			BaseNodeView hover = FindHoverParent();
			interaction.SetHoverParent(hover);

			//if (hover != null)
			//{
			//	Debug.Log($"Hover : {hover.Node.Title}");
			//}


			canvas.RefreshGraph();
		}

		public void OnMouseUp(MouseUpEvent evt)
		{
			if (evt.button == 2)
			{
				interaction.EndPan();
				return;
			}

			if (interaction.IsBoxSelecting)
			{
				interaction.EndBoxSelection();

				canvas.RefreshGraph();

				return;
			}

			if (interaction.HoverParent != null)
			{
				BaseNodeData parent = interaction.HoverParent.Node;
				BaseNodeData child = interaction.DragNode.Node;

				parent.AddChild(child);

				// 暫時先放到 Parent 正下方
				BaseNodeData root = TreeLayout.FindRoot(parent);

				TreeLayout.Layout(root);

				EditorUtility.SetDirty(canvas.Asset);
			}

			interaction.SetHoverParent(null);
			interaction.EndDrag();

			canvas.RefreshGraph();
		}
		private void SelectNodesInBox()
		{
			//Debug.Log("SelectNodesInBox");
			Vector2 start = interaction.BoxStart;
			Vector2 end = interaction.BoxEnd;

			Rect selection = Rect.MinMaxRect(
				Mathf.Min(start.x, end.x),
				Mathf.Min(start.y, end.y),
				Mathf.Max(start.x, end.x),
				Mathf.Max(start.y, end.y));

			canvas.SelectionManager.Clear();

			foreach (BaseNodeView node in canvas.NodeLayer.NodeViews)
			{
				Vector2 screenPos = viewport.WorldToScreen(node.Node.Position);

				Rect nodeRect = new Rect(
					screenPos.x,
					screenPos.y,
					node.Node.Width * viewport.Zoom,
					node.Node.Height * viewport.Zoom);

				if (selection.Overlaps(nodeRect))
				{
					canvas.SelectionManager.Add(node);
				}
			}

			if (canvas.SelectionManager.SelectedNodes.Count == 1)
			{
				BaseNodeView node = canvas.SelectionManager.SelectedNodes[0];

				inspector.Show(node.Node);
			}
			else
			{
				inspector.Clear();
			}
		}
		public void OnWheel(WheelEvent evt)
		{
			float delta = evt.delta.y > 0 ? -0.1f : 0.1f;

			canvas.Viewport.ZoomAt(
				evt.localMousePosition,
				delta);

			canvas.RefreshGraph();

			evt.StopPropagation();
		}

		private Rect GetDragRect()
		{
			BaseNodeView drag = interaction.DragNode;

			Vector2 screen =
				viewport.WorldToScreen(drag.Node.Position);

			return new Rect(
				screen.x,
				screen.y,
				drag.Node.Width * viewport.Zoom,
				drag.Node.Height * viewport.Zoom);
		}
		private BaseNodeView FindHoverParent()
		{
			Vector2 mouse = interaction.CurrentMousePosition;


			foreach (BaseNodeView node in canvas.NodeLayer.NodeViews)
			{
				if (node == interaction.DragNode)
					continue;


				Rect dropZone = node.GetDropZone(viewport);

				if (!node.Node.CanAddChild(interaction.DragNode.Node))
					continue;

				if (dropZone.Contains(mouse))
					return node;
			}

			return null;
		}
		#endregion
	}
}