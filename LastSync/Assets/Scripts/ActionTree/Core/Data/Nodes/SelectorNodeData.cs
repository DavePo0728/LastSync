using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace ActionTree.Data
{
	[System.Serializable]
	[MovedFrom(false, null, null, "SelectorNodeData")]
	public class SelectorNodeData : BaseNodeData
	{
		public SelectorNodeData(Vector2 position)
			: base(position,
				  "Selector")
		{
		}

		public override bool CanDelete => true;

		public override bool CanDuplicate => true;

		public override bool CanRename => true;

		public override bool CanMove => true;
		public override string TypeId => ActionTreeNodeTypeIds.Selector;

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
