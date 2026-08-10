using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace ActionTree.Data
{
	[System.Serializable]
	[MovedFrom(false, null, null, "ActionNodeData")]
	public class ActionNodeData : BaseNodeData
	{
		[SerializeField]
		private ActionData action = new();

		public ActionData Action => action;

		public ActionNodeData(Vector2 position)
			: base(position, "Action")
		{
		}

		public override bool CanDelete => true;
		public override bool CanDuplicate => true;
		public override bool CanRename => true;
		public override bool CanMove => true;
		public override string TypeId => ActionTreeNodeTypeIds.Action;

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
