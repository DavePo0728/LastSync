using UnityEngine;
using UnityEngine.UIElements;
using ActionTree.Data;
namespace ActionTree.Editor
{
	public class SequenceNodeView : BaseNodeView
	{
		public SequenceNodeView(SequenceNodeData node)
			: base(node)
		{
			Header.style.backgroundColor = new Color(0.35f, 0.35f, 1.0f);

			TitleLabel.text = "Sequence";
		}
	}
}