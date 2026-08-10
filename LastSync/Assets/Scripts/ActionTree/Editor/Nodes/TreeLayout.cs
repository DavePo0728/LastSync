using UnityEngine;
using UnityEngine.UIElements;
using ActionTree.Data;
namespace ActionTree.Editor
{
	public static class TreeLayout
	{
		public static void Layout(BaseNodeData root)
		{
			if (root == null)
				return;

			RefreshSize(root);

			CalculateWidth(root);

			Place(root, root.Position);
		}

		private static void RefreshSize(BaseNodeData node)
		{
			node.ResetSize();

			foreach (BaseNodeData child in node.Children)
			{
				RefreshSize(child);
			}
		}

		private static float CalculateWidth(BaseNodeData node)
		{
			if (node.Children.Count == 0)
			{
				node.SetSubtreeWidth(node.Width);

				//Debug.Log($"{node.Title} : {node.SubtreeWidth}");

				return node.SubtreeWidth;
			}

			float totalWidth = 0;

			foreach (BaseNodeData child in node.Children)
			{
				totalWidth += CalculateWidth(child);
			}

			totalWidth +=
				node.ChildSpacingX *
				(node.Children.Count - 1);

			node.SetSubtreeWidth(
				Mathf.Max(node.NodeWidth, totalWidth));

			//Debug.Log($"{node.Title} : {node.SubtreeWidth}");

			return node.SubtreeWidth;
		}

		private static void Place(BaseNodeData node, Vector2 position)
		{
			node.Position = position;

			if (node.Children.Count == 0)
				return;

			float totalWidth = 0f;

			foreach (BaseNodeData child in node.Children)
			{
				totalWidth += child.SubtreeWidth;
			}

			totalWidth += node.ChildSpacingX * (node.Children.Count - 1);

			// 父節點中心
			float parentCenter =
				position.x + node.Width * 0.5f;

			// 第一棵子樹左界
			float currentLeft =
				parentCenter - totalWidth * 0.5f;

			foreach (BaseNodeData child in node.Children)
			{
				// 子樹中心
				float subtreeCenter =
					currentLeft + child.SubtreeWidth * 0.5f;

				// Node 左上角
				float childX =
					subtreeCenter - child.Width * 0.5f;

				float childY =
					position.y + node.Height + node.ChildSpacingY;

				Place(
					child,
					new Vector2(childX, childY));

				currentLeft +=
					child.SubtreeWidth +
					node.ChildSpacingX;
			}
		}

		public static BaseNodeData FindRoot(BaseNodeData node)
		{
			while (node.Parent != null)
				node = node.Parent;

			return node;
		}
		

	}
}