using System.Collections.Generic;
using UnityEngine;

namespace ActionTree.Data
{
	[CreateAssetMenu(
		fileName = "New SkillData",
		menuName = "Action Tree/Skill Data")]
	public class SkillData : ScriptableObject
	{
		[Header("Info")]
		[SerializeField]
		private string skillName;

		[SerializeField]
		private Sprite icon;

		[Header("Setting")]
		[SerializeField]
		private float damage = 10f;

		[SerializeField]
		private float range = 5f;

		[SerializeField]
		private float cooldown = 1f;

		[SerializeField]
		private float castTime;

		[SerializeField]
		private float priority;

		[Header("Runtime")]

		public string SkillName => skillName;

		public Sprite Icon => icon;

		public float Damage => damage;

		public float Range => range;

		public float Cooldown => cooldown;

		public float CastTime => castTime;

		public float Priority => priority;

		[Header("Before")]
		[SerializeField]
		private List<SkillActionData> beforeActions = new();

		[Header("Attack")]
		[SerializeField]
		private List<SkillActionData> attackActions = new();

		[Header("After")]
		[SerializeField]
		private List<SkillActionData> afterActions = new();

		public List<SkillActionData> BeforeActions => beforeActions;

		public List<SkillActionData> AttackActions => attackActions;

		public List<SkillActionData> AfterActions => afterActions;
	}
}