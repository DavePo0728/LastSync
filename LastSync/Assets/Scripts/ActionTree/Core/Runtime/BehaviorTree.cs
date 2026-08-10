namespace ActionTree.Runtime
{
	public class BehaviorTree
	{
		public RuntimeNode Root { get; set; }

		public NodeState Tick(AIContext context)
		{
			if (Root == null)
				return NodeState.Failure;

			return Root.Tick(context);
		}

		public void Reset()
		{
			Root?.Reset();
		}
	}
}
