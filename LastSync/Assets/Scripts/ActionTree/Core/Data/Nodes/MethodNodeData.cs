
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace ActionTree.Data
{
	[System.Serializable]
	[MovedFrom(false, null, null, "MethodNodeData")]
	public sealed class MethodNodeData : BaseNodeData
	{
		[SerializeField]
		private ActionData action = new();

		public ActionData Action => action;

		public MethodNodeData(Vector2 position)
			: base(position, "Method")
		{
		}

		public override string TypeId => ActionTreeNodeTypeIds.Method;
	}
}
