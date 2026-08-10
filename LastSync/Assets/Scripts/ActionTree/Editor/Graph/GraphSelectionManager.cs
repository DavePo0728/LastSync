using System.Collections.Generic;

namespace ActionTree.Editor
{
	public class GraphSelectionManager
	{
		public IReadOnlyList<BaseNodeView> SelectedNodes => selectedNodes;

		private readonly List<BaseNodeView> selectedNodes = new();

		public void Clear()
		{
			foreach (BaseNodeView node in selectedNodes)
			{
				node.SetSelected(false);
			}

			selectedNodes.Clear();
		}

		public void Select(BaseNodeView node)
		{
			Clear();

			if (node == null)
				return;

			selectedNodes.Add(node);

			node.SetSelected(true);
		}

		public void Add(BaseNodeView node)
		{
			if (node == null)
				return;

			if (selectedNodes.Contains(node))
				return;

			selectedNodes.Add(node);

			node.SetSelected(true);
		}

		public void Remove(BaseNodeView node)
		{
			if (!selectedNodes.Remove(node))
				return;

			node.SetSelected(false);
		}

		public bool IsSelected(BaseNodeView node)
		{
			return selectedNodes.Contains(node);
		}
		public void Set(IEnumerable<BaseNodeView> nodes)
		{
			Clear();

			foreach (BaseNodeView node in nodes)
			{
				Add(node);
			}
		}
	}
}