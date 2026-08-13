using ActionTree.Runtime;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace ActionTree
{
	public static class ReflectionUtility
	{
		private const BindingFlags MethodFlags =
			BindingFlags.Instance |
			BindingFlags.Public |
			BindingFlags.DeclaredOnly;

		public static MonoBehaviour[] GetComponents(GameObject gameObject)
		{
			if (gameObject == null)
				return Array.Empty<MonoBehaviour>();

			return gameObject.GetComponents<MonoBehaviour>();
		}

		public static MonoBehaviour GetComponent(GameObject gameObject, string componentType)
		{
			if (gameObject == null || string.IsNullOrEmpty(componentType))
				return null;

			MonoBehaviour result = null;
			int count = 0;

			foreach (MonoBehaviour component in GetComponents(gameObject))
			{
				if (component == null)
					continue;

				if (component.GetType().FullName != componentType)
					continue;

				count++;

				if (result == null)
					result = component;
			}

			if (count > 1)
			{
				Debug.LogWarning(
					$"Found {count} '{componentType}' components on '{gameObject.name}'. " +
					"The first component will be used.");
			}

			return result;
		}

		public static MethodInfo GetMethod(MonoBehaviour component, string methodName)
		{
			if (component == null || string.IsNullOrEmpty(methodName))
				return null;

			return component.GetType().GetMethod(methodName, MethodFlags);
		}

		public static List<MethodInfo> GetSupportedMethods(MonoBehaviour component)
		{
			List<MethodInfo> methods = new();

			if (component == null)
				return methods;

			foreach (MethodInfo method in component.GetType().GetMethods(MethodFlags))
			{
				if (IsMethodValid(method))
					methods.Add(method);
			}

			return methods;
		}

		public static bool IsMethodValid(MethodInfo method)
		{
			if (method == null || !method.IsPublic || method.IsGenericMethod)
				return false;

			foreach (ParameterInfo parameter in method.GetParameters())
			{
				if (!IsSupportedType(parameter.ParameterType))
					return false;
			}

			return true;
		}

		public static bool IsSupportedType(Type type)
		{
			if (type == null)
				return false;

			return type == typeof(int) ||
				type == typeof(float) ||
				type == typeof(bool) ||
				type == typeof(string) ||
				type == typeof(Vector2) ||
				type == typeof(Vector3) ||
				type == typeof(Vector4) ||
				type == typeof(Color) ||
				type.IsEnum ||
				typeof(UnityEngine.Object).IsAssignableFrom(type);
		}

		public static NodeState Invoke(ActionData action, AIContext context)
		{
			if (action == null || !action.enabled || context == null)
				return NodeState.Failure;

			MonoBehaviour component = action.cachedComponent;

			if (component == null ||
				component.gameObject != context.Owner)
			{
				component = GetComponent(context.Owner, action.componentType);
				action.cachedComponent = component;
			}

			if (component == null)
				return NodeState.Failure;

			MethodInfo method = action.cachedMethod;

			if (method == null)
			{
				method = GetMethod(component, action.methodName);
				action.cachedMethod = method;
			}

			if (method == null)
				return NodeState.Failure;

			object[] arguments = BuildArguments(action.parameters, context);

			try
			{
				object result = method.Invoke(component, arguments);

				if (result is ActNode actNode)
					return actNode ? NodeState.Success : NodeState.Failure;

				if (result is CondNode condNode)
					return condNode ? NodeState.Success : NodeState.Failure;

				Debug.LogError($"{component.GetType().Name}.{method.Name} must return ActNode or CondNode.");
				return NodeState.Failure;
			}
			catch (Exception e)
			{
				Debug.LogException(e);
				return NodeState.Failure;
			}
		}

		public static object[] BuildArguments(List<MethodParameter> parameters)
		{
			return BuildArguments(parameters, null);
		}

		public static object[] BuildArguments(List<MethodParameter> parameters, AIContext context)
		{
			if (parameters == null || parameters.Count == 0)
				return Array.Empty<object>();

			object[] arguments = new object[parameters.Count];

			for (int i = 0; i < parameters.Count; i++)
			{
				MethodParameter parameter = parameters[i];
				Type type = Type.GetType(parameter.parameterType);

				if (type == typeof(int))
					arguments[i] = parameter.intValue;
				else if (type == typeof(float))
					arguments[i] = parameter.floatValue;
				else if (type == typeof(bool))
					arguments[i] = parameter.boolValue;
				else if (type == typeof(string))
					arguments[i] = parameter.stringValue;
				else if (type == typeof(Vector2))
					arguments[i] = parameter.vector2Value;
				else if (type == typeof(Vector3))
					arguments[i] = parameter.vector3Value;
				else if (type == typeof(Vector4))
					arguments[i] = parameter.vector4Value;
				else if (type == typeof(Color))
					arguments[i] = parameter.colorValue;
				else if (type != null && type.IsEnum)
					arguments[i] = Enum.ToObject(type, parameter.intValue);
				else if (type != null && typeof(UnityEngine.Object).IsAssignableFrom(type))
				{
					UnityEngine.Object value = parameter.objectValue;

					if (value == null && context != null && typeof(Component).IsAssignableFrom(type))
						value = context.Owner.GetComponent(type);

					arguments[i] = value;
				}
				else
				{
					Debug.LogWarning($"Unsupported parameter type: {parameter.parameterType}");
					arguments[i] = null;
				}
			}

			return arguments;
		}
	}
}
