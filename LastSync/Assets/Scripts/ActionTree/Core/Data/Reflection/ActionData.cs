using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using ActionTree.Data;
using UnityEngine.Scripting.APIUpdating;

namespace ActionTree
{
	[Serializable]
	[MovedFrom(false, null, null, "ActionData")]
	public class ActionData
	{
		public string componentType;

		public string methodName;

		public List<MethodParameter> parameters = new();

		[SerializeField]
		private SkillData skill;

		public SkillData Skill
		{
			get => skill;
			set => skill = value;
		}

		public bool enabled = true;

		[NonSerialized]
		internal MonoBehaviour cachedComponent;

		[NonSerialized]
		internal MethodInfo cachedMethod;

		[NonSerialized]
		internal ParameterInfo[] cachedParameters;

		public string DisplayName
		{
			get
			{
				if (string.IsNullOrEmpty(methodName))
					return "Missing";

				if (parameters == null || parameters.Count == 0)
					return $"{methodName}()";

				string[] names = new string[parameters.Count];

				for (int i = 0; i < parameters.Count; i++)
				{
					Type type = Type.GetType(parameters[i].parameterType);

					names[i] = type != null
						? type.Name
						: "Unknown";
				}

				return $"{methodName}({string.Join(", ", names)})";
			}
		}

		public void Clear()
		{
			componentType = string.Empty;
			methodName = string.Empty;
			parameters.Clear();

			ClearCache();
		}

		public void ClearCache()
		{
			cachedComponent = null;
			cachedMethod = null;
			cachedParameters = null;
		}

		public void CopyFrom(ActionData source)
		{
			ClearCache();

			if (source == null)
			{
				Clear();
				return;
			}

			componentType = source.componentType;
			methodName = source.methodName;
			skill = source.skill;
			enabled = source.enabled;

			parameters ??= new List<MethodParameter>();
			parameters.Clear();

			if (source.parameters == null)
				return;

			foreach (MethodParameter parameter in source.parameters)
			{
				parameters.Add(new MethodParameter(parameter));
			}
		}

		public ActionData Clone()
		{
			ActionData clone = new();
			clone.CopyFrom(this);
			return clone;
		}

		public void RebuildParameters(MethodInfo method)
		{
			ClearCache();

			parameters.Clear();

			if (method == null)
				return;

			cachedMethod = method;
			cachedParameters = method.GetParameters();

			foreach (ParameterInfo info in cachedParameters)
			{
				MethodParameter parameter = new MethodParameter
				{
					parameterName = info.Name,
					parameterType = info.ParameterType.AssemblyQualifiedName
				};

				parameters.Add(parameter);
			}
		}
	}
}
