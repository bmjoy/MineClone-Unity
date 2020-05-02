using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class ChunkDataManager
{
	public ChunkDataManager()
	{
		data = new Dictionary<Vector2Int, ChunkData>();
		currentlyLoading = new List<ChunkData>();
		textureMapper = new TextureMapper();
	}
	public Dictionary<Vector2Int, ChunkData> data;
	public TextureMapper textureMapper;

	private List<ChunkData> currentlyLoading;
	public void Update()
	{
		for (int i = currentlyLoading.Count - 1; i > -1; --i)
		{
			ChunkData chunkData = currentlyLoading[i];
			if (data.ContainsKey(chunkData.position))
			{
				//duplicate
				currentlyLoading.RemoveAt(i);
				continue;
			}
			if (chunkData.Ready())
			{
				data.Add(chunkData.position, chunkData);
				currentlyLoading.RemoveAt(i);
			}
		}
	}

	public bool CanRender(Vector2Int chunk)
	{
		bool result = true;
		result &= IsAvailableOtherwiseLoad(chunk);
		result &= IsAvailableOtherwiseLoad(chunk+new Vector2Int(1,0));
		result &= IsAvailableOtherwiseLoad(chunk + new Vector2Int(-1, 0));
		result &= IsAvailableOtherwiseLoad(chunk + new Vector2Int(0, 1));
		result &= IsAvailableOtherwiseLoad(chunk + new Vector2Int(0, -1));
		return result;
	}

	private bool IsAvailableOtherwiseLoad(Vector2Int position)
	{
		if (data.ContainsKey(position)) return true;
		ChunkData chunkData = new ChunkData(position);
		currentlyLoading.Add(chunkData);
		return false;
	}

	public char GetBlock(Vector2Int chunk, int x, int y, int z)
	{
		if (x > 15)
		{
			x -= 16;
			chunk.x += 1;
		}
		if (x < 0)
		{
			x += 16;
			chunk.x -= 1;
		}
		if (z > 15)
		{
			z -= 16;
			chunk.y += 1;
		}
		if (z < 0)
		{
			z += 16;
			chunk.y -= 1;
		}
		if (y > 255) return (char)0;
		if (y < 0) return (char)0;
		ChunkData chunkData = data[chunk];
		try
		{
			return chunkData.GetBlocks()[x, y, z];
		}
		catch (System.Exception e)
		{
			Debug.Log(x + " - " + y + " - " + z);
			Debug.LogError(e.Message + " - "+e.StackTrace );
			throw new System.Exception(e.Message);
		}
	}
}
