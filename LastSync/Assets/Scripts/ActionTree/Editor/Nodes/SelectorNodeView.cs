using UnityEngine;
using UnityEngine.UIElements;
using ActionTree.Data;
namespace ActionTree.Editor
{
	public class SelectorNodeView : BaseNodeView
	{
		public SelectorNodeView(SelectorNodeData node)
			: base(node)
		{
			Header.style.backgroundColor = new Color(0.35f, 0.35f, 1.0f);

			TitleLabel.text = "Selector";
		}
	}
}