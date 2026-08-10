using UnityEngine;
using UnityEngine.UIElements;
using ActionTree.Data;
namespace ActionTree.Editor
{
	public class RootNodeView : BaseNodeView
	{
		public RootNodeView(RootNodeData node)
			: base(node)
		{
			Header.style.backgroundColor = new Color(0.75f, 0.15f, 0.15f);

			TitleLabel.text = "Root";
		}
	}
}