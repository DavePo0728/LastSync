using UnityEngine;

namespace ActionTree.Editor
{
	public class GraphViewport
	{
		public Vector2 Offset { get; private set; } = Vector2.zero;

		public float Zoom { get; private set; } = 1f;

		public void SetOffset(Vector2 offset)
		{
			Offset = offset;
		}

		public void Move(Vector2 delta)
		{
			Offset += delta;
		}

		public void ZoomAt(Vector2 mouseScreen, float delta)
		{
			float oldZoom = Zoom;

			Zoom = Mathf.Clamp(Zoom + delta, 0.4f, 2.5f);

			if (Mathf.Approximately(oldZoom, Zoom))
				return;
			//Debug.Log($"Zoom = {Zoom}");
			Vector2 world = (mouseScreen - Offset) / oldZoom;

			Offset = mouseScreen - world * Zoom;
		}

		public Vector2 WorldToScreen(Vector2 world)
		{
			return world * Zoom + Offset;
		}

		public Vector2 ScreenToWorld(Vector2 screen)
		{
			return (screen - Offset) / Zoom;
		}

		public void Reset()
		{
			Offset = Vector2.zero;
			Zoom = 1f;
		}

	}
}
