using System.Collections.Generic;

namespace ActionTree.Editor
{
	public class GraphNodeManager
	{
		private readonly List<BaseNodeView> nodeViews = new();

		public IReadOnlyList<BaseNodeView> NodeViews => nodeViews;

		public int Count => nodeViews.Count;

		public void Add(BaseNodeView node)
		{
			if (node == null)
				return;

			if (nodeViews.Contains(node))
				return;

			nodeViews.Add(node);
		}

		public void Remove(BaseNodeView nodeView)
		{
			nodeViews.Remove(nodeView);
		}

		public void Clear()
		{
			nodeViews.Clear();
		}

		public bool Contains(BaseNodeView node)
		{
			return nodeViews.Contains(node);
		}

		public BaseNodeView Find(string guid)
		{
			foreach (BaseNodeView node in nodeViews)
			{
				if (node.Node.Guid == guid)
					return node;
			}

			return null;
		}
	}
}