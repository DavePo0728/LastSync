using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace ActionTree.Data
{
	[System.Serializable]
	[MovedFrom(false, null, null, "ConditionNodeData")]
	public class ConditionNodeData : BaseNodeData
	{
		[SerializeField]
		private ActionData condition = new();

		public ActionData Condition => condition;

		public ConditionNodeData(Vector2 position)
			: base(position, "Condition")
		{
		}

		public override bool CanDelete => true;

		public override bool CanDuplicate => true;

		public override bool CanRename => true;

		public override bool CanMove => true;
		public override string TypeId => ActionTreeNodeTypeIds.Condition;

		public override bool CanAcceptParent(BaseNodeData parent)
		{
			return true;
		}

		public override bool CanAddChild(BaseNodeData child)
		{
			return false;
		}
		
	}
}
