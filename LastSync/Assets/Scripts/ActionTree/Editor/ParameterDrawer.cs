using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

namespace ActionTree.Editor
{
	public static class ParameterDrawer
	{
		public static void Draw(
			VisualElement root,
			List<MethodParameter> parameters,
			Action changed = null)
		{
			root.Clear();

			foreach (MethodParameter parameter in parameters)
			{
				Type type = Type.GetType(parameter.parameterType);

				if (type == typeof(int))
				{
					IntegerField field = new(parameter.parameterName)
					{
						value = parameter.intValue
					};

					field.RegisterValueChangedCallback(evt =>
					{
						parameter.intValue = evt.newValue;
						changed?.Invoke();
					});

					root.Add(field);
				}
				else if (type == typeof(float))
				{
					FloatField field = new(parameter.parameterName)
					{
						value = parameter.floatValue
					};

					field.RegisterValueChangedCallback(evt =>
					{
						parameter.floatValue = evt.newValue;
						changed?.Invoke();
					});

					root.Add(field);
				}
				else if (type == typeof(bool))
				{
					Toggle field = new(parameter.parameterName)
					{
						value = parameter.boolValue
					};

					field.RegisterValueChangedCallback(evt =>
					{
						parameter.boolValue = evt.newValue;
						changed?.Invoke();
					});

					root.Add(field);
				}
				else if (type == typeof(string))
				{
					TextField field = new(parameter.parameterName)
					{
						value = parameter.stringValue
					};

					field.RegisterValueChangedCallback(evt =>
					{
						parameter.stringValue = evt.newValue;
						changed?.Invoke();
					});

					root.Add(field);
				}
				else if (typeof(UnityEngine.Object).IsAssignableFrom(type))
				{
					ObjectField field = new(parameter.parameterName)
					{
						objectType = type,
						value = parameter.objectValue,
						allowSceneObjects = false
					};

					field.RegisterValueChangedCallback(evt =>
					{
						parameter.objectValue = evt.newValue;
						changed?.Invoke();
					});

					root.Add(field);
				}
				else
				{
					root.Add(new Label(parameter.parameterName));
				}
			}
		}
	}
}
