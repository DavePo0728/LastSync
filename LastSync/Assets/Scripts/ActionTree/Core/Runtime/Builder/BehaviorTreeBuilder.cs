using ActionTree.Data;
using UnityEngine;

namespace ActionTree.Runtime
{
	public static class BehaviorTreeBuilder
	{
		public static BehaviorTree Build(ActionTreeAsset asset)
		{
			BehaviorTree tree = new BehaviorTree();

			if (asset == null)
			{
				Debug.LogError("ActionTreeAsset 為 Null。");
				return tree;
			}

			if (asset.Root == null)
			{
				Debug.LogError("ActionTreeAsset 沒有 Root。");
				return tree;
			}

			tree.Root = BuildNode(asset.Root);

			return tree;
		}

		private static RuntimeNode BuildNode(BaseNodeData node)
		{
			if (node == null)
				return null;

			RuntimeNode runtimeNode = CreateRuntimeNode(node);

			if (runtimeNode == null)
				return null;

			if (runtimeNode is CompositeNode composite)
			{
				foreach (BaseNodeData child in node.Children)
				{
					RuntimeNode runtimeChild = BuildNode(child);

					if (runtimeChild != null)
						composite.Children.Add(runtimeChild);
				}
			}

			return runtimeNode;
		}

		private static RuntimeNode CreateRuntimeNode(BaseNodeData node)
		{
			switch (node)
			{
				case RootNodeData:
					return new RootNode();

				case SelectorNodeData:
					return new SelectorNode();

				case SequenceNodeData:
					return new SequenceNode();

				case ActionNodeData action:
					{
						return new ReflectionMethodNode(action.Action);
					}

				case ConditionNodeData condition:
					{
						return new ReflectionMethodNode(condition.Condition);
					}

				case MethodNodeData method:
					{
						return new ReflectionMethodNode(method.Action);
					}
				default:
					Debug.LogWarning($"Unsupported node type: {node.GetType().Name}");
					return null;
			}
		}
	}
}
