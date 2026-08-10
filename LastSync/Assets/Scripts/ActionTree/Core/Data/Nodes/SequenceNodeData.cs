using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace ActionTree.Data
{
	[System.Serializable]
	[MovedFrom(false, null, null, "SequenceNodeData")]
	public class SequenceNodeData : BaseNodeData
	{
		public SequenceNodeData(Vector2 position)
			: base(position,
				  "Sequence")
		{
		}

		public override bool CanDelete => true;

		public override bool CanDuplicate => true;

		public override bool CanRename => true;

		public override bool CanMove => true;
		public override string TypeId => ActionTreeNodeTypeIds.Sequence;

		public override bool CanAcceptParent(BaseNodeData parent)
		{
			return true;
		}

		public override bool CanAddChild(BaseNodeData child)
		{
			return child != null;
		}
	}
}
