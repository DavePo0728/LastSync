using UnityEngine;
using UnityEngine.UIElements;
using ActionTree.Data;
namespace ActionTree.Editor
{
	public class ConditionNodeView : BaseNodeView
	{
		public ConditionNodeView(ConditionNodeData node)
			: base(node)
		{
			Header.style.backgroundColor = new Color(0.35f, 0.35f, 1.0f);

			TitleLabel.text = "Condition";
		}

		public override void Refresh(GraphViewport viewport)
		{
			base.Refresh(viewport);
			TitleLabel.text = GetConditionTitle();
		}

		private string GetConditionTitle()
		{
			ConditionNodeData conditionNode = Node as ConditionNodeData;
			ActionData condition = conditionNode?.Condition;
			string owner = condition?.componentType;

			if (!string.IsNullOrEmpty(owner))
			{
				int separator = owner.LastIndexOf('.');
				if (separator >= 0)
					owner = owner.Substring(separator + 1);
			}

			if (string.IsNullOrEmpty(owner))
				owner = "Condition";

			SkillData skill = FindSkill();
			if (skill != null)
				return $"{owner} -> {skill.name} -> {condition.methodName}";

			return string.IsNullOrEmpty(condition?.methodName)
				? owner
				: $"{owner} -> {condition.methodName}";
		}

		private SkillData FindSkill()
		{
			ConditionNodeData conditionNode = Node as ConditionNodeData;
			ActionData condition = conditionNode?.Condition;

			if (condition?.parameters != null)
			{
				foreach (MethodParameter parameter in condition.parameters)
				{
					if (parameter.objectValue is SkillData skill)
						return skill;
				}
			}

			return condition?.Skill;
		}

	}
}
