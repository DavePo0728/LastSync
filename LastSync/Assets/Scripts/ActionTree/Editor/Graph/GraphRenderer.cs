using UnityEngine;
using UnityEngine.UIElements;

namespace ActionTree.Editor
{
	public abstract class GraphRenderer
	{
		public abstract void Draw(MeshGenerationContext context, GraphViewport viewport);
	}
}