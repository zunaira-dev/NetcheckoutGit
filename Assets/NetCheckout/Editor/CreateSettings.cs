using UnityEngine;
using UnityEditor;
using System.IO;
using NetCheckout;

public class CreateSettings
{
    [MenuItem("Assets/Create/NetCheckout Settings")]
    public static void CreateSettingsObject()
    {
		string name = "Settings.asset";
		Settings settings = ScriptableObject.CreateInstance<Settings>();
        
		AssetDatabase.CreateAsset(settings, Path.Combine(GetSelectedPathOrFallback(), name));
		AssetDatabase.SaveAssets();

		EditorUtility.FocusProjectWindow();
		Selection.activeObject = settings;
	}

	// credit: https://gist.github.com/allanolivei/9260107
	private static string GetSelectedPathOrFallback()
	{
		string path = "Assets";

		foreach (UnityEngine.Object obj in Selection.GetFiltered(typeof(UnityEngine.Object), SelectionMode.Assets))
		{
			path = AssetDatabase.GetAssetPath(obj);
			if (!string.IsNullOrEmpty(path) && File.Exists(path))
			{
				path = Path.GetDirectoryName(path);
				break;
			}
		}
		return path;
	}
}
