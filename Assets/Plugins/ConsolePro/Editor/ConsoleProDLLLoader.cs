
// -------------------------------------------------------------
// Console Pro DLL Loader Helper Class
// This class ensures that the correct ConsolePro.Editor.dll is enabled
// based on the current Unity version (DLLs are in version-specific subfolders)
//
// Support: figbash@gmail.com
// -------------------------------------------------------------

#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;

namespace FlyingWormConsole3
{

	[InitializeOnLoad]
	public class ConsoleProDLLLoader
	{
		const string DLLName = "ConsolePro.Editor.dll";

		static ConsoleProDLLLoader()
		{
			EditorApplication.delayCall += LoadCorrectDLL;
		}

		static string[] ValidFolders = new string[] { "2021_3_Plus", "6000_3_Plus" };

		static void LoadCorrectDLL()
		{
			string[] dllGuids = AssetDatabase.FindAssets("ConsolePro.Editor");
			if (dllGuids.Length == 0)
			{
				Debug.LogError("#Console Pro# No ConsolePro.Editor.dll files found. Console Pro installation may be corrupted.");
				return;
			}

			string[] allDllPaths = dllGuids.Select(guid => AssetDatabase.GUIDToAssetPath(guid))
				.Where(path => path.EndsWith(DLLName))
				.ToArray();

			foreach (string assetPath in allDllPaths)
			{
				string folderName = Path.GetFileName(Path.GetDirectoryName(assetPath));
				if (System.Array.IndexOf(ValidFolders, folderName) < 0)
				{
					Debug.Log($"#Console Pro# Found legacy DLL not in version folder, deleting: {assetPath}");
					AssetDatabase.DeleteAsset(assetPath);
				}
			}

			string[] dllPaths = dllGuids.Select(guid => AssetDatabase.GUIDToAssetPath(guid))
				.Where(path => path.EndsWith(DLLName) && System.Array.IndexOf(ValidFolders, Path.GetFileName(Path.GetDirectoryName(path))) >= 0)
				.ToArray();

			if (dllPaths.Length == 0)
			{
				Debug.LogError("#Console Pro# No ConsolePro.Editor.dll files found. Console Pro installation may be corrupted.");
				return;
			}

			string targetFolder = null;

	#if UNITY_6000_3_OR_NEWER
		targetFolder = "6000_3_Plus";
#else
		targetFolder = "2021_3_Plus";
#endif


			if (string.IsNullOrEmpty(targetFolder))
			{
				Debug.LogError($"#Console Pro# Could not determine correct DLL folder for Unity version {Application.unityVersion}. Minimum supported Unity version is 2021.3.0f1.");
				return;
			}

			bool targetDLLExists = false;
			int enabledDLLCount = 0;
			string targetAssetPath = null;
			bool needsRefresh = false;

			foreach (string assetPath in dllPaths)
			{
				string folderName = Path.GetFileName(Path.GetDirectoryName(assetPath));
				PluginImporter importer = AssetImporter.GetAtPath(assetPath) as PluginImporter;

				if (importer != null)
				{
					bool shouldBeEnabled = (folderName == targetFolder);
					bool currentlyEnabled = importer.GetCompatibleWithEditor();

					if (shouldBeEnabled)
					{
						targetDLLExists = true;
					}

					if (currentlyEnabled)
					{
						enabledDLLCount++;
					}

					if (currentlyEnabled != shouldBeEnabled)
					{
						if (shouldBeEnabled)
						{
							Debug.Log($"#Console Pro# Activating DLL for current Unity version: {folderName}/{DLLName}");
							targetAssetPath = assetPath;
						}
						else
						{
							Debug.Log($"#Console Pro# Deactivating DLL: {folderName}/{DLLName}");
						}

						importer.SetCompatibleWithEditor(shouldBeEnabled);
						importer.SaveAndReimport();
						needsRefresh = true;
					}
				}
				else
				{
					Debug.LogError("#Console Pro# Could not get PluginImporter for: " + assetPath);
				}
			}

			if (enabledDLLCount > 1 && !needsRefresh)
			{
				Debug.LogError($"#Console Pro# Multiple DLLs are currently enabled ({enabledDLLCount}). This may indicate a corrupted installation.");
			}

			if (!targetDLLExists)
			{
				Debug.LogError($"#Console Pro# Target DLL folder not found: {targetFolder}. Console Pro is not installed correctly.");
			}

			if (needsRefresh)
			{
				Debug.Log("#Console Pro# DLL configuration changed, refreshing assets...");
				if (!string.IsNullOrEmpty(targetAssetPath))
				{
					AssetDatabase.ImportAsset(targetAssetPath, ImportAssetOptions.ForceUpdate);
				}
			}
		}
	}

}
#endif