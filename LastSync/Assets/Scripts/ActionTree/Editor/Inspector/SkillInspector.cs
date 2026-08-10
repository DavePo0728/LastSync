using System;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace ActionTree.Editor
{
	public class SkillInspector : VisualElement
	{
		public event Action Changed;

		private readonly SkillActionData action;
		private readonly MethodSelector selector = new();
		private readonly VisualElement parameterContainer = new();

		public SkillInspector(string title, SkillActionData action)
		{
			this.action = action;

			style.flexGrow = 1;

			selector.Refresh(typeof(SkillProcess));

			Add(new Label(title));

			List<string> names = new(selector.GetDisplayNames());

			int index = -1;

			for (int i = 0; i < selector.Count; i++)
			{
				if (selector.GetMethod(i).Name == action.MethodName)
				{
					index = i;
					break;
				}
			}

			if (index < 0 && names.Count > 0)
			{
				index = 0;

				action.MethodName = selector.GetMethod(0).Name;
				action.RebuildParameters(selector.GetMethod(0));
			}

			PopupField<string> popup = new(
				"Method",
				names,
				index);

			popup.RegisterValueChangedCallback(evt =>
			{
				int newIndex = names.IndexOf(evt.newValue);

				if (newIndex < 0)
					return;

				action.MethodName = selector.GetMethod(newIndex).Name;
				action.RebuildParameters(selector.GetMethod(newIndex));

				RefreshParameters();

				Changed?.Invoke();
			});

			Add(popup);

			Add(parameterContainer);

			RefreshParameters();
		}

		private void RefreshParameters()
		{
			ParameterDrawer.Draw(
				parameterContainer,
				action.Parameters,
				() => Changed?.Invoke());
		}
	}
}
