using ActionTree.Data;
using System;
using System.Reflection;

namespace ActionTree.Editor
{
	public sealed class MethodEntry
	{
		public Type OwnerType;

		public MethodInfo Method;

		public string Group;

		public string DisplayName;

		public SkillData Skill;
	}
}