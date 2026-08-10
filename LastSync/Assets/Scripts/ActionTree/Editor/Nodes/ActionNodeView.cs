using UnityEngine;
using UnityEngine.UIElements;
using ActionTree.Data;
namespace ActionTree.Editor
{
	public class ActionNodeView : BaseNodeView
	{
		public ActionNodeView(ActionNodeData node)
			: base(node)
		{
			Header.style.backgroundColor = new Color(0.35f, 0.35f, 1.0f);

			TitleLabel.text = "Action";
		}

		public override void Refresh(GraphViewport viewport)
		{
			base.Refresh(viewport);
			TitleLabel.text = GetActionTitle();
		}

		private string GetActionTitle()
		{
			ActionNodeData actionNode = Node as ActionNodeData;
			ActionData action = actionNode?.Action;
			string owner = action?.componentType;
			if (!string.IsNullOrEmpty(owner))
			{
				int separator = owner.LastIndexOf('.');
				if (separator >= 0)
					owner = owner.Substring(separator + 1);
			}

			if (string.IsNullOrEmpty(owner))
				owner = "Action";

			SkillData skill = FindSkill();
			if (skill != null)
				return $"{owner} -> {skill.name} -> {action.methodName}";

			return string.IsNullOrEmpty(action?.methodName)
				? owner
				: $"{owner} -> {action.methodName}";
		}

		private SkillData FindSkill()
		{
			ActionNodeData actionNode = Node as ActionNodeData;
			ActionData action = actionNode?.Action;
			if (action?.parameters == null)
				return null;

			foreach (MethodParameter parameter in action.parameters)
			{
				if (parameter.objectValue is SkillData skill)
					return skill;
			}

			return action.Skill;
		}
	}
}
