using System.Collections.Generic;
using UnityEngine;
using ActionTree.Data;
namespace ActionTree.Editor
{
	public class NodeLayer : BaseLayer
	{
		public IReadOnlyList<BaseNodeView> NodeViews => nodeManager.NodeViews;
		private readonly GraphNodeManager nodeManager;
		
		private System.Action<BaseNodeView> onNodeClicked;
		public NodeLayer(
			GraphViewport viewport,
			GraphNodeManager nodeManager)
			: base(viewport)
		{

			this.nodeManager = nodeManager;
		}

		public void AddNode(BaseNodeView nodeView)
		{
			nodeManager.Add(nodeView);

			nodeView.Clicked += OnNodeClicked;

			Add(nodeView);

			nodeView.Refresh(Viewport);
		}

		public override void Refresh()
		{
			base.Refresh();

			foreach (BaseNodeView node in nodeManager.NodeViews)
			{
				node.Refresh(Viewport);
			}
		}
		public void SetNodeClickedCallback(System.Action<BaseNodeView> callback)
		{
			onNodeClicked = callback;
		}

		private void OnNodeClicked(BaseNodeView node)
		{
			onNodeClicked?.Invoke(node);
		}
public BaseNodeView HitTest(Vector2 worldPosition)
{
	foreach (BaseNodeView node in nodeManager.NodeViews)
	{
		if (node.Node.Rect.Contains(worldPosition))
		{
			return node;
		}
	}
	return null;
}
		public void ClearNodes()
		{
			foreach (BaseNodeView node in nodeManager.NodeViews)
			{
				node.Clicked -= OnNodeClicked;

				Remove(node);
			}

			nodeManager.Clear();
		}
		public BaseNodeView Find(BaseNodeData node)
		{
			foreach (BaseNodeView view in nodeManager.NodeViews)
			{
				if (view.Node == node)
					return view;
			}

			return null;
		}
		public void RemoveNode(BaseNodeView node)
		{
			node.Clicked -= OnNodeClicked;

			Remove(node);

			nodeManager.Remove(node);
		}
	}
}