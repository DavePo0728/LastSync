#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(FixedPrefabNode))]
public class FixedPrefabNodeDrawer : PropertyDrawer
{
	private const float Spacing = 2f;

	public override float GetPropertyHeight(
		SerializedProperty property,
		GUIContent label)
	{
		float line = EditorGUIUtility.singleLineHeight;

		if (!property.isExpanded)
		{
			return line;
		}

		SerializedProperty usePrefabTransform =
			property.FindPropertyRelative("UsePrefabTransform");

		int visibleFields = usePrefabTransform.boolValue ? 2 : 5;
		return line + Spacing + visibleFields * (line + Spacing);
	}

	public override void OnGUI(
		Rect position,
		SerializedProperty property,
		GUIContent label)
	{
		EditorGUI.BeginProperty(position, label, property);

		float line = EditorGUIUtility.singleLineHeight;
		Rect row = new Rect(position.x, position.y, position.width, line);

		property.isExpanded = EditorGUI.Foldout(
			row,
			property.isExpanded,
			label,
			true);

		if (property.isExpanded)
		{
			EditorGUI.indentLevel++;

			SerializedProperty prefab =
				property.FindPropertyRelative("Prefab");
			SerializedProperty usePrefabTransform =
				property.FindPropertyRelative("UsePrefabTransform");

			row.y += line + Spacing;
			EditorGUI.PropertyField(row, prefab);

			row.y += line + Spacing;
			EditorGUI.PropertyField(row, usePrefabTransform);

			if (!usePrefabTransform.boolValue)
			{
				DrawNextProperty(
					ref row,
					property.FindPropertyRelative("Position"));
				DrawNextProperty(
					ref row,
					property.FindPropertyRelative("Rotation"));
				DrawNextProperty(
					ref row,
					property.FindPropertyRelative("Scale"));
			}

			EditorGUI.indentLevel--;
		}

		EditorGUI.EndProperty();
	}

	private static void DrawNextProperty(
		ref Rect row,
		SerializedProperty property)
	{
		row.y += EditorGUIUtility.singleLineHeight + Spacing;
		EditorGUI.PropertyField(row, property);
	}
}
#endif
