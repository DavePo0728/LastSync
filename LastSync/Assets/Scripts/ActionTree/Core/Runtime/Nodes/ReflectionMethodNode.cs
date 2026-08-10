using ActionTree;

namespace ActionTree.Runtime
{
	public sealed class ReflectionMethodNode : RuntimeNode
	{
		private readonly ActionData actionData;

		public ReflectionMethodNode(ActionData actionData)
		{
			this.actionData = actionData;
		}

		public ActionData Action => actionData;

		public override NodeState Tick(AIContext context)
		{
			State = ReflectionUtility.Invoke(actionData, context);
			return State;
		}
	}
}
