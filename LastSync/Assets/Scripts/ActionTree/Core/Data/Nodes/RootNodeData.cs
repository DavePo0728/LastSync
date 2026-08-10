using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace ActionTree.Data
{
	[System.Serializable]
	[MovedFrom(false, null, null, "RootNodeData")]
	public class RootNodeData : BaseNodeData
	{
		public RootNodeData(Vector2 position)
			: base(position,
				  "Root")
		{
		}

		public override bool CanDelete => false;

		public override bool CanDuplicate => false;

		public override bool CanRename => false;

		public override bool CanMove => true;
		public override string TypeId => ActionTreeNodeTypeIds.Root;

		public override bool CanAcceptParent(BaseNodeData parent)
		{
			return false;
		}

		public override bool CanAddChild(BaseNodeData child)
		{
			if (child == null)
				return false;

			return Children.Count == 0;
		}
	}
}
