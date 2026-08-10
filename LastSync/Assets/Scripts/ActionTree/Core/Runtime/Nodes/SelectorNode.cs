namespace ActionTree.Runtime
{
	public class SelectorNode : CompositeNode
	{
		public override NodeState Tick(AIContext context)
		{
			foreach (RuntimeNode child in Children)
			{
				NodeState result = child.Tick(context);

				switch (result)
				{
					case NodeState.Success:
						State = NodeState.Success;
						return State;

					case NodeState.Running:
						State = NodeState.Running;
						return State;
				}
			}

			State = NodeState.Failure;
			return State;
		}
	}
}
