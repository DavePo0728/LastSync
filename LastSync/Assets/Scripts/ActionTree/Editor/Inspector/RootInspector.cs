using ActionTree.Data;
using UnityEngine;
using UnityEngine.UIElements;

namespace ActionTree.Editor
{
	public class RootInspector : VisualElement
	{
		public RootInspector(RootNodeData root)
		{
			style.paddingLeft = 10;
			style.paddingRight = 10;
			style.paddingTop = 10;

			Label title = new Label("Root");
			title.style.fontSize = 18;
			title.style.unityFontStyleAndWeight = FontStyle.Bold;
			Add(title);

			Add(new Label(""));
			Add(new Label("This is Root Node."));
			Add(new Label(""));
			Add(new Label("Root is the entry point of the behavior tree."));
			Add(new Label("Only one child can be connected to Root."));
		}
	}
}
