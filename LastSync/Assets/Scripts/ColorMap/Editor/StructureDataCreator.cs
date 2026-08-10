#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public class StructureDataCreator
{
	[MenuItem("Tools/Create StructureData")]
	public static void Create()
	{
		EnsureFolderExists();

		Texture2D texture =
			Selection.activeObject as Texture2D;

		if (texture == null)
		{
			Debug.LogError("Create StructureData failed: select a Texture2D first.");
			return;
		}

		ColorMapParser parser = new();

		StructureData data =
			parser.Parse(texture);

		if (data == null)
		{
			return;
		}

		StructureData asset =
			ScriptableObject.CreateInstance<StructureData>();

		asset.StructureID = data.StructureID;
		asset.Size = data.Size;

		asset.Structures.AddRange(data.Structures);
		asset.Doors.AddRange(data.Doors);

		string path =
			AssetDatabase.GenerateUniqueAssetPath(
				"Assets/Data/ColorMap/StructureData/" +
				texture.name +
				".asset");

		AssetDatabase.CreateAsset(
			asset,
			path);

		AssetDatabase.SaveAssets();
		AssetDatabase.Refresh();

		Debug.Log($"StructureData created: {path}");
	}

	private static void EnsureFolderExists()
	{
		if (!AssetDatabase.IsValidFolder("Assets/Data"))
		{
			AssetDatabase.CreateFolder("Assets", "Data");
		}

		if (!AssetDatabase.IsValidFolder("Assets/Data/ColorMap"))
		{
			AssetDatabase.CreateFolder("Assets/Data", "ColorMap");
		}

		if (!AssetDatabase.IsValidFolder("Assets/Data/ColorMap/StructureData"))
		{
			AssetDatabase.CreateFolder(
				"Assets/Data/ColorMap",
				"StructureData");
		}
	}
}
#endif
