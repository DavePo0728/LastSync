using ActionTree.Data;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ActionTreeAsset))]
public class ActionTreeAssetInspector : UnityEditor.Editor
{
	public override void OnInspectorGUI()
	{
		DrawDefaultInspector();

		GUILayout.Space(10);

		ActionTreeAsset asset = (ActionTreeAsset)target;

		if (GUILayout.Button("Repair Asset"))
		{
			asset.SaveStableBackup();
			EditorUtility.SetDirty(asset);
			AssetDatabase.SaveAssets();
		}

		if (GUILayout.Button("Open Action Tree"))
		{
			asset.Create();
			EditorUtility.SetDirty(asset);
			AssetDatabase.SaveAssets();

			ActionTree.Editor.ActionTreeWindow.Open(asset, null);
		}
	}

	[MenuItem("Tools/Action Tree/Repair All ActionTree Assets")]
	private static void RepairAllActionTreeAssets()
	{
		string[] guids = AssetDatabase.FindAssets("t:ActionTreeAsset");
		int repairedCount = 0;

		foreach (string guid in guids)
		{
			string path = AssetDatabase.GUIDToAssetPath(guid);
			ActionTreeAsset asset = AssetDatabase.LoadAssetAtPath<ActionTreeAsset>(path);

			if (asset == null)
				continue;

			asset.SaveStableBackup();
			EditorUtility.SetDirty(asset);
			repairedCount++;
		}

		AssetDatabase.SaveAssets();
		AssetDatabase.Refresh();

		Debug.Log($"Repaired {repairedCount} ActionTreeAsset files.");
	}
}
