using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace ActionTree.Editor
{
	/// <summary>
	/// Reflection 快取
	/// </summary>
	public static class ReflectionCache
	{
		/// <summary>
		/// Component -> Methods
		/// </summary>
		private static readonly Dictionary<Type, List<MethodInfo>> methodCache = new();

		/// <summary>
		/// 清除快取
		/// </summary>
		public static void Clear()
		{
			methodCache.Clear();
		}

		/// <summary>
		/// 取得 Component 可用 Method
		/// </summary>
		public static List<MethodInfo> GetMethods(MonoBehaviour component)
		{
			if (component == null)
				return new List<MethodInfo>();

			return GetMethods(component.GetType());
		}

		/// <summary>
		/// 取得 Type 可用 Method
		/// </summary>
		public static List<MethodInfo> GetMethods(Type type)
		{
			if (type == null)
				return new List<MethodInfo>();

			if (methodCache.TryGetValue(type, out List<MethodInfo> methods))
				return methods;

			methods = new List<MethodInfo>();

			foreach (MethodInfo method in type.GetMethods(
	BindingFlags.Instance |
	BindingFlags.Static |
	BindingFlags.Public |
	BindingFlags.DeclaredOnly))
			{
				if (ReflectionUtility.IsMethodValid(method))
				{
					methods.Add(method);
				}
			}

			methodCache[type] = methods;

			return methods;
		}

		/// <summary>
		/// 是否已有快取
		/// </summary>
		public static bool HasCache(Type type)
		{
			return type != null && methodCache.ContainsKey(type);
		}

		/// <summary>
		/// 移除指定 Type 快取
		/// </summary>
		public static void Remove(Type type)
		{
			if (type == null)
				return;

			methodCache.Remove(type);
		}
	}
}