using System.Collections.Generic;
using UnityEngine;
using ActionTree.Runtime;

namespace ActionTree.Editor
{
	public class ActionTreeEditorContext
	{
		public IReadOnlyList<MonoBehaviour> Components => components;

		private readonly List<MonoBehaviour> components = new();

		public BehaviorTreeRunner Runner { get; set; }

		public GameObject Owner => Runner != null ? Runner.gameObject : null;

		public void Refresh()
		{
			components.Clear();

			if (Owner == null)
				return;

			components.AddRange(Owner.GetComponents<MonoBehaviour>());
		}
	}
}