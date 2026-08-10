using UnityEngine;
using UnityEngine.UIElements;

namespace ActionTree.Editor
{
	public class GridLayer : BaseLayer
	{
		private const float MinorGrid = 20f;
		private const int MajorEvery = 5;

		public GridLayer(GraphViewport viewport)
			: base(viewport)
		{
			generateVisualContent += OnGenerateVisualContent;
		}

		private void OnGenerateVisualContent(MeshGenerationContext context)
		{
			Painter2D painter = context.painter2D;

			Rect rect = contentRect;

			Vector2 worldMin = Viewport.ScreenToWorld(Vector2.zero);
			Vector2 worldMax = Viewport.ScreenToWorld(new Vector2(rect.width, rect.height));

			float firstX = Mathf.Floor(worldMin.x / MinorGrid) * MinorGrid;
			float firstY = Mathf.Floor(worldMin.y / MinorGrid) * MinorGrid;

			//==========================
			// Vertical
			//==========================

			for (float worldX = firstX; worldX <= worldMax.x; worldX += MinorGrid)
			{
				float screenX = Viewport.WorldToScreen(new Vector2(worldX, 0)).x;

				int index = Mathf.RoundToInt(worldX / MinorGrid);

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

				int index = Mathf.RoundToInt(worldY / MinorGrid);

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