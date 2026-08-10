using ActionTree.Data;
using ActionTree.Editor;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using System.Reflection;
[CustomEditor(typeof(SkillData))]
public class SkillDataEditor : Editor
{
	private ReorderableList beforeList;
	private ReorderableList attackList;
	private ReorderableList afterList;

	private void OnEnable()
	{
		beforeList = CreateList("beforeActions", "Before");
		attackList = CreateList("attackActions", "Attack");
		afterList = CreateList("afterActions", "After");
	}

	public override void OnInspectorGUI()
	{
		serializedObject.Update();

		DrawPropertiesExcluding(
			serializedObject,
			"beforeActions",
			"attackActions",
			"afterActions");

		EditorGUILayout.Space();

		beforeList.DoLayoutList();

		EditorGUILayout.Space();

		attackList.DoLayoutList();

		EditorGUILayout.Space();

		afterList.DoLayoutList();

		serializedObject.ApplyModifiedProperties();
	}
	private void DrawSkillAction(Rect rect, SerializedProperty action)
	{
		float y = rect.y;

		SerializedProperty methodName =
			action.FindPropertyRelative("methodName");

		MethodInfo method = typeof(SkillProcess).GetMethod(
	methodName.stringValue,
	BindingFlags.Public | BindingFlags.Static);

		string displayName = methodName.stringValue;

		if (method != null)
		{
			SkillActionAttribute attribute =
				method.GetCustomAttribute<SkillActionAttribute>();

			if (attribute != null)
			{
				displayName = $"{method.Name} ({attribute.Name})";
			}
		}

		EditorGUI.LabelField(
			new Rect(rect.x, y, rect.width, EditorGUIUtility.singleLineHeight),
			displayName);

		y += EditorGUIUtility.singleLineHeight + 2;

		SerializedProperty parameters =
			action.FindPropertyRelative("parameters");

		for (int i = 0; i < parameters.arraySize; i++)
		{
			SerializedProperty parameter =
				parameters.GetArrayElementAtIndex(i);

			string name =
				parameter.FindPropertyRelative("parameterName").stringValue;

			string type =
				parameter.FindPropertyRelative("parameterType").stringValue;

			if (type == typeof(float).AssemblyQualifiedName)
			{
				EditorGUI.PropertyField(
					new Rect(rect.x, y, rect.width, EditorGUIUtility.singleLineHeight),
					parameter.FindPropertyRelative("floatValue"),
					new GUIContent(name));
			}
			else if (type == typeof(int).AssemblyQualifiedName)
			{
				EditorGUI.PropertyField(
					new Rect(rect.x, y, rect.width, EditorGUIUtility.singleLineHeight),
					parameter.FindPropertyRelative("intValue"),
					new GUIContent(name));
			}
			else if (type == typeof(bool).AssemblyQualifiedName)
			{
				EditorGUI.PropertyField(
					new Rect(rect.x, y, rect.width, EditorGUIUtility.singleLineHeight),
					parameter.FindPropertyRelative("boolValue"),
					new GUIContent(name));
			}
			else if (type == typeof(string).AssemblyQualifiedName)
			{
				EditorGUI.PropertyField(
					new Rect(rect.x, y, rect.width, EditorGUIUtility.singleLineHeight),
					parameter.FindPropertyRelative("stringValue"),
					new GUIContent(name));
			}

			y += EditorGUIUtility.singleLineHeight + 2;
		}
	}
	private ReorderableList CreateList(string propertyName, string title)
	{
		SerializedProperty property =
			serializedObject.FindProperty(propertyName);

		ReorderableList list = new ReorderableList(
			serializedObject,
			property,
			true,
			true,
			true,
			true);

		list.drawHeaderCallback = rect =>
		{
			EditorGUI.LabelField(rect, title);
		};

		list.drawElementCallback = (rect, index, active, focused) =>
		{
			SerializedProperty element =
				property.GetArrayElementAtIndex(index);

			DrawSkillAction(rect, element);
		};

		list.elementHeightCallback = index =>
		{
			SerializedProperty element =
				property.GetArrayElementAtIndex(index);

			SerializedProperty parameters =
				element.FindPropertyRelative("parameters");

			return (parameters.arraySize + 1) * (EditorGUIUtility.singleLineHeight + 2) + 4;
		};
		list.onAddDropdownCallback = (Rect buttonRect, ReorderableList l) =>
		{
			MethodSelector selector = new();
			selector.Refresh(typeof(SkillProcess));

			GenericMenu menu = new GenericMenu();

			for (int i = 0; i < selector.Count; i++)
			{
				MethodInfo method = selector.GetMethod(i);

				SkillActionAttribute attribute =
					method.GetCustomAttribute<SkillActionAttribute>();

				string name = attribute != null
					? attribute.Name
					: method.Name;

				menu.AddItem(new GUIContent(name), false, () =>
				{
					serializedObject.Update();

					int index = property.arraySize;

					property.InsertArrayElementAtIndex(index);

					SerializedProperty element =
						property.GetArrayElementAtIndex(index);

					element.FindPropertyRelative("methodName").stringValue = method.Name;

					serializedObject.ApplyModifiedProperties();

					SkillData skillData = (SkillData)target;

					List<SkillActionData> listData = null;

					if (property.name == "beforeActions")
						listData = skillData.BeforeActions;
					else if (property.name == "attackActions")
						listData = skillData.AttackActions;
					else if (property.name == "afterActions")
						listData = skillData.AfterActions;

					if (listData != null)
					{
						listData[index].RebuildParameters(method);

						EditorUtility.SetDirty(skillData);
					}
				});
			}

			menu.DropDown(buttonRect);
		};
		return list;
	}
}
