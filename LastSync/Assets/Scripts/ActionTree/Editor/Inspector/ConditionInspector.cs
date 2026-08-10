using UnityEngine.UIElements;
using ActionTree.Data;
using ActionTree.Runtime;
using System.Collections.Generic;
using static ActionTree.Editor.MethodSelector;


namespace ActionTree.Editor
{
	public class ConditionInspector : RuntimeNodeInspector
	{

		private readonly ActionTreeEditorContext context;
		private readonly ConditionNodeData node;
		private readonly MethodSelector selector = new();
		private readonly VisualElement parameterContainer = new();

		public ConditionInspector(
			ConditionNodeData node,
			ActionTreeEditorContext context)
			: base("Condition")
		{
			this.node = node;
			this.context = context;

			style.flexGrow = 1;

			selector.Refresh(
	context.Components,
	MethodCategory.Condition);

			Add(new Label("Condition"));

			List<string> names = new(selector.GetDisplayNames());

			if (names.Count == 0)
			{
				Add(new Label("No condition methods found."));
				return;
			}

			int index = selector.Find(node.Condition);

			if (index < 0 && names.Count > 0)
				index = 0;

			PopupField<string> popup = new PopupField<string>(
				"Method",
				names,
				index);

			popup.RegisterValueChangedCallback(evt =>
			{
				int newIndex = names.IndexOf(evt.newValue);

				if (newIndex < 0)
					return;

				selector.Apply(newIndex, node.Condition);

				RefreshParameters();

				NotifyChanged();
			});

			Add(popup);
			Add(parameterContainer);

			RefreshParameters();
		}

		private void RefreshParameters()
		{
			ParameterDrawer.Draw(
				parameterContainer,
				node.Condition.parameters,
				NotifyChanged);
		}

		protected override void SetRuntime(RuntimeNode runtime)
		{
			currentRuntime = runtime;
		}
	}
}
