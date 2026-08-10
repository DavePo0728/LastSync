using ActionTree.Data;
using ActionTree.Runtime;
using System.Collections.Generic;
using UnityEngine.UIElements;
using static ActionTree.Editor.MethodSelector;


namespace ActionTree.Editor
{
	public class ActionInspector : RuntimeNodeInspector
	{
		private readonly ActionTreeEditorContext context;
		private readonly ActionNodeData node;
		private readonly MethodSelector selector = new();
		private readonly VisualElement parameterContainer = new();
		public ActionInspector(ActionNodeData node, ActionTreeEditorContext context)
	: base("Action")
		{
			this.node = node;
			this.context = context;

			style.flexGrow = 1;

			selector.Refresh(
	context.Components,
	MethodCategory.Action);

			Add(new Label("Action"));

			List<string> names = new(selector.GetDisplayNames());

			if (names.Count == 0)
			{
				Add(new Label("No action methods found."));
				return;
			}

			int index = selector.Find(node.Action);

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

				selector.Apply(newIndex, node.Action);

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
				node.Action.parameters,
				NotifyChanged);
		}
		protected override void SetRuntime(RuntimeNode runtime)
		{
			currentRuntime = runtime;
		}
	}
}
