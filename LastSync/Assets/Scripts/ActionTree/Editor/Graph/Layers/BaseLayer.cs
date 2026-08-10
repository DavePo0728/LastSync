using UnityEngine;
using UnityEngine.UIElements;

namespace ActionTree.Editor
{
	public abstract class BaseLayer : VisualElement
	{
		/// <summary>
		/// Graph 共用 Viewport
		/// </summary>
		protected GraphViewport Viewport { get; }

		protected BaseLayer(GraphViewport viewport)
		{
			Viewport = viewport;

			style.position = Position.Absolute;

			style.left = 0;
			style.top = 0;
			style.right = 0;
			style.bottom = 0;

			// 預設不攔截滑鼠事件
			pickingMode = PickingMode.Ignore;
		}
		public virtual void Refresh()
		{
			MarkDirtyRepaint();
		}
		
	}
}