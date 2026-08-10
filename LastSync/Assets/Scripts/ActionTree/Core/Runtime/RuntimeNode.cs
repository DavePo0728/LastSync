namespace ActionTree.Runtime
{
	public abstract class RuntimeNode
	{
		public NodeState State { get; protected set; } = NodeState.Failure;

		public abstract NodeState Tick(AIContext context);

		public virtual void Reset()
		{
			State = NodeState.Failure;
		}
	}
}
