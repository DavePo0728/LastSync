using System.Collections.Generic;

namespace ActionTree.Runtime
{
	public abstract class CompositeNode : RuntimeNode
	{
		public List<RuntimeNode> Children { get; } = new();

		protected int currentChildIndex;

		public override void Reset()
		{
			base.Reset();
			currentChildIndex = 0;

			foreach (RuntimeNode child in Children)
			{
				child?.Reset();
			}
		}
	}
}
