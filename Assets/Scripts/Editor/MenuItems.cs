using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
public class MenuItems : MonoBehaviour
{
	[MenuItem("Minecraft-Unity/Open Save Folder")]
	static void OpenSaveFolder()
	{
		EditorUtility.RevealInFinder(Application.persistentDataPath + "/Worlds");
	}
	[MenuItem("Minecraft-Unity/Open Build Folder")]
	static void OpenBuildFolder()
	{
		EditorUtility.RevealInFinder(new DirectoryInfo( Application.dataPath).Parent.FullName + "/Build");
	}
	[MenuItem("Minecraft-Unity/Run Latest Build")]
	static void RunLatestBuild()
	{
		string file = new DirectoryInfo(Application.dataPath).Parent.FullName + "/Build/Minecraft-In-Unity.exe";
		System.Diagnostics.Process process = new System.Diagnostics.Process();
		System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
		startInfo.FileName = file;
		process.StartInfo = startInfo;
		process.Start();
	}
	[MenuItem("Minecraft-Unity/Create Audio Placeholders")]
	static void CreateAudioPlaceholders()
	{
		CreateAudioPlaceholders(new DirectoryInfo(new DirectoryInfo(Application.dataPath).FullName + "/Resources/Audio"));
		AssetDatabase.Refresh();
	}

	static void CreateAudioPlaceholders(DirectoryInfo directory)
	{
		foreach (DirectoryInfo d in directory.GetDirectories())
		{
			CreateAudioPlaceholders(d);
		}
		FileInfo[] files = directory.GetFiles();
		for (int i = 0; i < files.Length; ++i)
		{
			FileInfo file = files[i];
			if (file.Name.EndsWith(".ogg"))
			{
				FileInfo placeHolder = new FileInfo(file.FullName + ".placeholder");
				if (!placeHolder.Exists) placeHolder.Create();
			}
		}
	}
}