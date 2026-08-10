using UnityEngine;

namespace ActionTree.Runtime
{
	public class AIContext
	{
		public GameObject Owner { get; }

		public AIContext(GameObject owner)
		{
			Owner = owner;
		}

		public T Get<T>() where T : Component
		{
			return Owner.GetComponent<T>();
		}
	}
}