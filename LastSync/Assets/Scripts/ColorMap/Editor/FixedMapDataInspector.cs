using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(FixedMapData))]
public class FixedMapDataInspector : Editor
{
	public override void OnInspectorGUI()
	{
		if (GUILayout.Button("Open 2DEdit"))
			FixedMap2DEditWindow.Open((FixedMapData)target);

		if (DrawDefaultInspector())
			FixedMap2DEditWindow.RefreshOpenWindows();
	}
}
