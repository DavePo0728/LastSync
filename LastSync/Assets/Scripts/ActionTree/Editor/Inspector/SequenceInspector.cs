using ActionTree.Data;
using UnityEngine;
using UnityEngine.UIElements;

namespace ActionTree.Editor
{
	public class SequenceInspector : VisualElement
	{
		public SequenceInspector(SequenceNodeData node)
		{
			style.paddingLeft = 10;
			style.paddingRight = 10;
			style.paddingTop = 10;

			Label title = new Label("Sequence");
			title.style.fontSize = 20;
			title.style.unityFontStyleAndWeight = FontStyle.Bold;
			title.style.marginBottom = 8;
			Add(title);

			Add(new Label($"Children : {node.Children.Count}"));
		}
	}
}
