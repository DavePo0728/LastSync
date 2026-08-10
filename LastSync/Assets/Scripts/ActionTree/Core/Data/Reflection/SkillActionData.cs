using ActionTree;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

[Serializable]
public class SkillActionData
{
	[SerializeField]
	private string methodName;

	[SerializeField]
	private List<MethodParameter> parameters = new();

	public string MethodName
	{
		get => methodName;
		set => methodName = value;
	}

	public List<MethodParameter> Parameters => parameters;

	public void RebuildParameters(MethodInfo method)
	{
		parameters.Clear();

		if (method == null)
			return;

		foreach (ParameterInfo parameter in method.GetParameters())
		{
			// AIStatus 由 AISkill 自動傳入，不需要顯示
			if (parameter.ParameterType == typeof(AIStatus))
				continue;

			MethodParameter data = new()
			{
				parameterName = parameter.Name,
				parameterType = parameter.ParameterType.AssemblyQualifiedName
			};

			parameters.Add(data);
		}
	}
}