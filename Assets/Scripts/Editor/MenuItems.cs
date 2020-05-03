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
}