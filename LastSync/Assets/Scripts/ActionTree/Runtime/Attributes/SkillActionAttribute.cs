using System;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public sealed class SkillActionAttribute : Attribute
{
	public string Name { get; }

	public SkillActionAttribute(string name)
	{
		Name = name;
	}
}