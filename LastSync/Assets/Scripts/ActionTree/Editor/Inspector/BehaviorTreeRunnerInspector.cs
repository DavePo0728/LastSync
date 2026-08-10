using UnityEditor;
using UnityEngine;
using ActionTree.Editor;

[CustomEditor(typeof(BehaviorTreeRunner))]
public class BehaviorTreeRunnerInspector : Editor
{
	public override void OnInspectorGUI()
	{
		DrawDefaultInspector();

		EditorGUILayout.Space();

		BehaviorTreeRunner runner = (BehaviorTreeRunner)target;

		GUI.enabled = runner.Tree != null;

		if (GUILayout.Button("Repair Action Tree Asset"))
		{
			runner.Tree.SaveStableBackup();

			EditorUtility.SetDirty(runner.Tree);
			AssetDatabase.SaveAssets();
		}

		if (GUILayout.Button("Open Action Tree"))
		{
			runner.Tree.Create();

			EditorUtility.SetDirty(runner.Tree);
			AssetDatabase.SaveAssets();

			ActionTreeWindow.Open(runner.Tree, runner.gameObject);
		}

		GUI.enabled = true;
	}
}
