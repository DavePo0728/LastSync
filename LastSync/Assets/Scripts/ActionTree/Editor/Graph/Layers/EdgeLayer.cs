using UnityEngine.UIElements;
using UnityEngine;
using ActionTree.Data;
namespace ActionTree.Editor
{
	public class EdgeLayer : BaseLayer
	{
		private readonly GraphNodeManager nodeManager;
		private const float PortRadius = 4f;
		private const float ParentStem = 12f;
		private const float ChildStem = 24f;
		public EdgeLayer( GraphViewport viewport, GraphNodeManager nodeManager) : base(viewport)
		{
			this.nodeManager = nodeManager;

			generateVisualContent += OnGenerateVisualContent;
		}

		private void OnGenerateVisualContent(
			MeshGenerationContext ctx)
		{
			Painter2D painter = ctx.painter2D;

			painter.lineWidth = 2;
			painter.strokeColor = Color.white;

			foreach (BaseNodeView view in nodeManager.NodeViews)
			{
				if (view.Node.Parent != null)
					continue;

				DrawTree(
					painter,
					view.Node);
			}
		}

		private void DrawTree( Painter2D painter, BaseNodeData node)
		{
			if (node.Children.Count == 0)
				return;

			DrawPort(
				painter,
				node.OutputPort);

			foreach (BaseNodeData child in node.Children)
			{
				DrawPort(
					painter,
					child.InputPort);
			}

			DrawBranch(
				painter,
				node);

			foreach (BaseNodeData child in node.Children)
			{
				DrawTree(
					painter,
					child);
			}
		}

		private void DrawBranch( Painter2D painter, BaseNodeData parent)
		{
			if (parent.Children.Count == 0)
				return;

			BaseNodeData first = parent.Children[0];
			BaseNodeData last = parent.Children[parent.Children.Count - 1];

			Vector2 parentBottom = parent.OutputPort;
			Vector2 firstTop = first.InputPort;
			Vector2 lastTop = last.InputPort;

			// 水平線放在 Parent 與 Child 的中間
			float horizontalY = (parentBottom.y + firstTop.y) * 0.5f;

			// Parent ↓
			DrawLine(
				painter,
				parentBottom,
				new Vector2(parentBottom.x, horizontalY));

			// 水平線
			DrawLine(
				painter,
				new Vector2(firstTop.x, horizontalY),
				new Vector2(lastTop.x, horizontalY));

			// Child ↓
			foreach (BaseNodeData child in parent.Children)
			{
				Vector2 childTop = child.InputPort;

				DrawLine(
					painter,
					new Vector2(childTop.x, horizontalY),
					childTop);
			}
		}

		private void DrawLine( Painter2D painter, Vector2 worldStart, Vector2 worldEnd)
		{
			Vector2 start =
				Viewport.WorldToScreen(worldStart);

			Vector2 end =
				Viewport.WorldToScreen(worldEnd);

			painter.BeginPath();

			painter.MoveTo(start);

			painter.LineTo(end);

			painter.Stroke();
		}
		private void DrawPort( Painter2D painter, Vector2 world)
		{
			Vector2 p = Viewport.WorldToScreen(world);

			painter.fillColor = Color.white;

			painter.BeginPath();

			painter.Arc(
				p,
				PortRadius,
				0,
				360);

			painter.Fill();
		}
	}
}