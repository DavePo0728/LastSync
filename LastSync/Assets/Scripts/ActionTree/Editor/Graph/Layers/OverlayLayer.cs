using UnityEngine;
using UnityEngine.UIElements;

namespace ActionTree.Editor
{
	public class OverlayLayer : BaseLayer
	{
		private readonly GraphInteractionManager interaction;

		public OverlayLayer(
			GraphViewport viewport,
			GraphInteractionManager interaction)
			: base(viewport)
		{
			this.interaction = interaction;

			generateVisualContent += OnGenerateVisualContent;
		}
		private void OnGenerateVisualContent(MeshGenerationContext context)
		{
			if (!interaction.IsBoxSelecting)
				return;

			Painter2D painter = context.painter2D;

			Rect rect = GetSelectionRect();

			painter.strokeColor = new Color(0.3f, 0.6f, 1f);
			painter.lineWidth = 1.5f;

			painter.BeginPath();
			painter.MoveTo(new Vector2(rect.xMin, rect.yMin));
			painter.LineTo(new Vector2(rect.xMax, rect.yMin));
			painter.LineTo(new Vector2(rect.xMax, rect.yMax));
			painter.LineTo(new Vector2(rect.xMin, rect.yMax));
			painter.ClosePath();
			painter.Stroke();
		}
		private Rect GetSelectionRect()
		{
			Vector2 start = interaction.BoxStart;
			Vector2 end = interaction.BoxEnd;

			float xMin = Mathf.Min(start.x, end.x);
			float yMin = Mathf.Min(start.y, end.y);
			float xMax = Mathf.Max(start.x, end.x);
			float yMax = Mathf.Max(start.y, end.y);

			return Rect.MinMaxRect(
				xMin,
				yMin,
				xMax,
				yMax);
		}
	}
}