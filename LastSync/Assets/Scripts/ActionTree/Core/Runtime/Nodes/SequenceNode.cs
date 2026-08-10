namespace ActionTree.Runtime
{
	public class SequenceNode : CompositeNode
	{
		public override NodeState Tick(AIContext context)
		{
			while (currentChildIndex < Children.Count)
			{
				NodeState result = Children[currentChildIndex].Tick(context);

				switch (result)
				{
					case NodeState.Success:
						currentChildIndex++;
						break;

					case NodeState.Failure:
						Reset();
						State = NodeState.Failure;
						return State;

					case NodeState.Running:
						State = NodeState.Running;
						return State;
				}
			}

			Reset();
			State = NodeState.Success;
			return State;
		}
	}
}
