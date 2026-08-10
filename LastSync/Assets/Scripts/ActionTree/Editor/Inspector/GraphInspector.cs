using ActionTree.Data;
using UnityEngine;
using UnityEngine.UIElements;

namespace ActionTree.Editor
{
	public class GraphInspector : VisualElement
	{
		public event System.Action InspectorChanged;

		private readonly ActionTreeEditorContext context;

		public GraphInspector(ActionTreeEditorContext context)
			: this()
		{
			this.context = context;
		}
		public GraphInspector()
		{
			style.width = 300;

			style.flexShrink = 0;

			style.backgroundColor = new Color(0.22f, 0.22f, 0.22f);

			style.borderLeftWidth = 2;

			style.borderLeftColor = new Color(0.12f, 0.12f, 0.12f);
		}

		public void Show(BaseNodeData node)
		{
			Clear();

			if (node is RootNodeData root)
			{
				Add(new RootInspector(root));
			}
			if (node is SelectorNodeData selector)
			{
				Add(new SelectorInspector(selector));
				return;
			}
			if (node is SequenceNodeData sequence)
			{
				Add(new SequenceInspector(sequence));
				return;
			}
			if (node is ConditionNodeData condition)
			{
				ConditionInspector inspector =
	new ConditionInspector(condition, context);

				inspector.Changed += () =>
				{
					InspectorChanged?.Invoke();
				};

				Add(inspector);
				return;
			}
			if (node is ActionNodeData action)
			{
				ActionInspector inspector = new ActionInspector(action, context);

				inspector.Changed += () =>
				{
					InspectorChanged?.Invoke();
				};

				Add(inspector);
			}
		}
	}
}
