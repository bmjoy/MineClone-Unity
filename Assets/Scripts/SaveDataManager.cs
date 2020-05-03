using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
public class SaveDataManager
{
	public static SaveDataManager instance { get; private set; }
	public bool busy { get; private set; }
	private DirectoryInfo saveDirectory;
	private FileInfo worldInfoFile;
	private DirectoryInfo worldDirectory;
	public SaveDataManager()
	{
		instance = this;
		saveDirectory = new DirectoryInfo(Application.persistentDataPath + "/Worlds");
		if (!saveDirectory.Exists) saveDirectory.Create();
	}
	public WorldInfo Initialize(WorldInfo worldInfo)
	{
		worldDirectory = new DirectoryInfo(saveDirectory.FullName + "/" + worldInfo.name);
		worldInfoFile = new FileInfo(worldDirectory + "/Info.json");
		if (worldDirectory.Exists)
		{
			Debug.Log("World already exists, Loading world info");
			worldInfo = JsonUtility.FromJson<WorldInfo>(File.ReadAllText(worldInfoFile.FullName));
		}
		else
		{
			Debug.Log("Creating world");
			worldDirectory.Create();
			File.WriteAllText(worldInfoFile.FullName, JsonUtility.ToJson(worldInfo));
		}
		return worldInfo;
	}
	public void Save(ChunkSaveData chunkData)
	{
	
		Vector2Int position = chunkData.position;
		Debug.Log("Saving changes to chunk " + position);
		FileInfo fileInfo = new FileInfo(worldDirectory + "/" + GetFileName(position));
		WriteChunk(fileInfo, chunkData);
	}

	public ChunkSaveData Load(Vector2Int position)
	{
		ChunkSaveData saveData = new ChunkSaveData(position);
		FileInfo fileInfo = new FileInfo(worldDirectory + "/" + GetFileName(position));
		if (fileInfo.Exists)
		{
			ReadChunk(fileInfo, saveData);
			Debug.Log("SaveManager loaded changes to chunk " + saveData.position);
		}
		else
		{
			
		}
		return saveData;
	}

	private string GetFileName(Vector2Int position)
	{
		return $"R.{position.x}.{position.y}.bin";
	}

	private void ReadChunk(FileInfo file, ChunkSaveData saveData)
	{
		while (busy) System.Threading.Thread.Sleep(8);
		busy = true;
		byte[] buffer = new byte[4];
		using (FileStream stream = new FileStream(file.FullName, FileMode.Open))
		{
			while (stream.Position < stream.Length)
			{
				stream.Read(buffer, 0, 4);
				saveData.changes.Add(new ChunkSaveData.C(buffer[0], buffer[1], buffer[2], buffer[3]));
			}
		}
		busy = false;
	}

	private void WriteChunk(FileInfo file, ChunkSaveData saveData)
	{
		while (busy) System.Threading.Thread.Sleep(8);
		busy = true;
		byte[] buffer = new byte[4];
		using (FileStream stream = new FileStream(file.FullName, FileMode.Create))
		{
			for (int i = 0; i < saveData.changes.Count; ++i)
			{
				buffer[0] = saveData.changes[i].x;
				buffer[1] = saveData.changes[i].y;
				buffer[2] = saveData.changes[i].z;
				buffer[3] = saveData.changes[i].b;
				stream.Write(buffer, 0, 4);
			}
		}
		Debug.Log("SaveManager saved changes to chunk " + saveData.position);

		busy = false;
	}
}
