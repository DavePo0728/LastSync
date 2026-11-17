using UnityEngine;
using UnityEngine.UIElements;

namespace ActionTree.Editor
{
	public class GridLayer : BaseLayer
	{
		public const float MinorGrid = 20f;
		private const int MajorEvery = 5;
		private readonly Vector2 gridOrigin;

		public GridLayer(GraphViewport viewport, Vector2 gridOrigin = default)
			: base(viewport)
		{
			this.gridOrigin = gridOrigin;
			generateVisualContent += OnGenerateVisualContent;
		}

		private void OnGenerateVisualContent(MeshGenerationContext context)
		{
			Painter2D painter = context.painter2D;

			Rect rect = contentRect;

			Vector2 worldMin = Viewport.ScreenToWorld(Vector2.zero);
			Vector2 worldMax = Viewport.ScreenToWorld(new Vector2(rect.width, rect.height));

			float firstX = Mathf.Floor((worldMin.x - gridOrigin.x) / MinorGrid) * MinorGrid + gridOrigin.x;
			float firstY = Mathf.Floor((worldMin.y - gridOrigin.y) / MinorGrid) * MinorGrid + gridOrigin.y;

			//==========================
			// Vertical
			//==========================

			for (float worldX = firstX; worldX <= worldMax.x; worldX += MinorGrid)
			{
				float screenX = Viewport.WorldToScreen(new Vector2(worldX, 0)).x;

				int index = Mathf.RoundToInt((worldX - gridOrigin.x) / MinorGrid);

				bool major = index % MajorEvery == 0;

				painter.strokeColor = major
					? new Color(0.32f, 0.32f, 0.32f)
					: new Color(0.24f, 0.24f, 0.24f);

				painter.lineWidth = major ? 1.4f : 1f;

				painter.BeginPath();
				painter.MoveTo(new Vector2(screenX, 0));
				painter.LineTo(new Vector2(screenX, rect.height));
				painter.Stroke();
			}

			//==========================
			// Horizontal
			//==========================

			for (float worldY = firstY; worldY <= worldMax.y; worldY += MinorGrid)
			{
				float screenY = Viewport.WorldToScreen(new Vector2(0, worldY)).y;

				int index = Mathf.RoundToInt((worldY - gridOrigin.y) / MinorGrid);

				bool major = index % MajorEvery == 0;

				painter.strokeColor = major
					? new Color(0.32f, 0.32f, 0.32f)
					: new Color(0.24f, 0.24f, 0.24f);

				painter.lineWidth = major ? 1.4f : 1f;

				painter.BeginPath();
				painter.MoveTo(new Vector2(0, screenY));
				painter.LineTo(new Vector2(rect.width, screenY));
				painter.Stroke();
			}
		}
	}
}
