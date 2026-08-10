namespace ActionTree.Runtime
{
	public class RootNode : CompositeNode
	{
		public override NodeState Tick(AIContext context)
		{
			if (Children.Count == 0)
			{
				State = NodeState.Failure;
				return State;
			}

			State = Children[0].Tick(context);
			return State;
		}
	}
}
