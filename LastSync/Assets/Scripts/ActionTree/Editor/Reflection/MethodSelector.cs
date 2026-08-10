using ActionTree.Data;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEditor;

namespace ActionTree.Editor
{
	public sealed class MethodSelector
	{
		public enum MethodCategory
		{
			Action,
			Condition,
			Skill
		}

		private readonly List<MethodEntry> entries = new();

		public IReadOnlyList<MethodEntry> Entries => entries;

		public int Count => entries.Count;

		public void Refresh(IReadOnlyList<MonoBehaviour> sourceComponents, MethodCategory category)
		{
			entries.Clear();

			foreach (MonoBehaviour component in sourceComponents)
			{
				if (component == null)
					continue;

				foreach (MethodInfo method in ReflectionCache.GetMethods(component))
				{
					switch (category)
					{
						case MethodCategory.Action:

							if (method.ReturnType != typeof(ActNode))
								continue;

							break;

						case MethodCategory.Condition:

							if (method.ReturnType != typeof(CondNode))
								continue;

							break;

						case MethodCategory.Skill:

							if (method.ReturnType != typeof(void))
								continue;

							if (!method.IsDefined(typeof(SkillActionAttribute), false))
								continue;

							break;
					}

					if (HasSkillParameter(method))
					{
						foreach (SkillData skill in LoadSkills())
						{
							entries.Add(new MethodEntry
							{
								OwnerType = component.GetType(),
								Method = method,
								Group = component.GetType().Name,
								DisplayName = $"{skill.name}/{method.Name}",
								Skill = skill
							});
						}

						continue;
					}

					entries.Add(new MethodEntry
					{
						OwnerType = component.GetType(),
						Method = method,
						Group = component.GetType().Name,
						DisplayName = method.Name
					});
				}
			}
		}

		public void Refresh(Type type)
		{
			entries.Clear();

			if (type == null)
				return;

			foreach (MethodInfo method in ReflectionCache.GetMethods(type))
			{
				if (method.ReturnType != typeof(void))
					continue;

				if (!method.IsDefined(typeof(SkillActionAttribute), false))
					continue;

				entries.Add(new MethodEntry
				{
					OwnerType = type,
					Method = method,
					Group = type.Name,
					DisplayName = method.Name
				});
			}
		}

		public MethodInfo GetMethod(int index)
		{
			if (index < 0 || index >= entries.Count)
				return null;

			return entries[index].Method;
		}

		public Type GetOwnerType(int index)
		{
			if (index < 0 || index >= entries.Count)
				return null;

			return entries[index].OwnerType;
		}

		public SkillData GetSkill(int index)
		{
			if (index < 0 || index >= entries.Count)
				return null;

			return entries[index].Skill;
		}

		public string[] GetDisplayNames()
		{
			string[] names = new string[entries.Count];

			for (int i = 0; i < entries.Count; i++)
			{
				names[i] = $"{entries[i].Group}/{entries[i].DisplayName}";
			}

			return names;
		}

		public int Find(ActionData action)
		{
			if (action == null)
				return -1;

			for (int i = 0; i < entries.Count; i++)
			{
				MethodEntry entry = entries[i];

				if (entry.OwnerType.FullName != action.componentType)
					continue;

				if (entry.Method.Name != action.methodName)
					continue;

				if (entry.Skill != null && entry.Skill != action.Skill)
					continue;

				return i;
			}

			return -1;
		}

		public void Apply(int index, ActionData action)
		{
			if (action == null)
				return;

			if (index < 0 || index >= entries.Count)
				return;

			MethodEntry entry = entries[index];

			action.componentType = entry.OwnerType.FullName;
			action.methodName = entry.Method.Name;
			action.RebuildParameters(entry.Method);
			action.Skill = entry.Skill;

			if (entry.Skill != null)
			{
				foreach (MethodParameter parameter in action.parameters)
				{
					if (Type.GetType(parameter.parameterType) == typeof(SkillData))
					{
						parameter.objectValue = entry.Skill;
						break;
					}
				}
			}
		}

		private static bool HasSkillParameter(MethodInfo method)
		{
			foreach (ParameterInfo parameter in method.GetParameters())
			{
				if (parameter.ParameterType == typeof(SkillData))
					return true;
			}

			return false;
		}

		private static IEnumerable<SkillData> LoadSkills()
		{
			foreach (string guid in AssetDatabase.FindAssets("t:SkillData"))
			{
				SkillData skill = AssetDatabase.LoadAssetAtPath<SkillData>(
					AssetDatabase.GUIDToAssetPath(guid));

				if (skill != null)
					yield return skill;
			}
		}
	}
}
