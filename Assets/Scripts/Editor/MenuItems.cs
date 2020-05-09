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
}