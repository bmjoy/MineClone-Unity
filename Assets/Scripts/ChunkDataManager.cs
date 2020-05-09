using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class ChunkDataManager
{
	public Dictionary<Vector2Int, ChunkData> data;
	private TextureMapper textureMapper;
	private List<Vector2Int> loadQueue;
	private List<Vector2Int> dirtyChunks;
	private readonly Vector2Int nFront = new Vector2Int(0, 1);
	private readonly Vector2Int nBack = new Vector2Int(0, -1);
	private readonly Vector2Int nLeft = new Vector2Int(-1, 0);
	private readonly Vector2Int nRight = new Vector2Int(1,0);


	public ChunkDataManager()
	{
		data = new Dictionary<Vector2Int, ChunkData>();
		loadQueue = new List<Vector2Int>();
		dirtyChunks = new List<Vector2Int>();
		textureMapper = GameManager.instance.textureMapper;
	}

	public void Update()
	{
		for (int i = loadQueue.Count-1; i > -1; --i)
		{
			Vector2Int position = loadQueue[i];
			ChunkData chunkData = data[position];
			if (!chunkData.startedLoadingDetails)
			{
				bool canStartLoadingDetails = true;
				canStartLoadingDetails &= chunkData.terrainReady;
				ChunkData front = data[position + nFront];
				ChunkData back = data[position + nBack];
				ChunkData left = data[position + nLeft];
				ChunkData right = data[position + nRight];
				canStartLoadingDetails &= front.terrainReady;
				canStartLoadingDetails &= back.terrainReady;
				canStartLoadingDetails &= left.terrainReady;
				canStartLoadingDetails &= right.terrainReady;
				if (canStartLoadingDetails)
				{
					chunkData.StartDetailsLoading(front, left, back, right);
				}
			}
			else
			{
				if (chunkData.chunkReady) loadQueue.RemoveAt(i);
			}
		}
	}

	public int GetChunksInMemoryCount()
	{
		return data.Count;
	}


	//returns true if ready
	public bool Load(Vector2Int position)
	{
		//to render, all neighbors need to be <chunkReady>
		ChunkData chunkData = LoadCompletely(position, position);
		ChunkData front = LoadCompletely(position + nFront, position);
		ChunkData back = LoadCompletely(position + nBack, position);
		ChunkData left = LoadCompletely(position + nLeft, position);
		ChunkData right = LoadCompletely(position + nRight, position);
		ChunkData frontLeft = LoadCompletely(position + nFront + nLeft, position);
		ChunkData frontRight = LoadCompletely(position + nFront + nRight, position);
		ChunkData backLeft = LoadCompletely(position + nBack + nLeft, position);
		ChunkData backRight = LoadCompletely(position + nBack + nRight, position);
		bool ready = true;
		ready &= chunkData.chunkReady;
		ready &= front.chunkReady;
		ready &= back.chunkReady;
		ready &= left.chunkReady;
		ready &= right.chunkReady;
		ready &= frontLeft.chunkReady;
		ready &= frontRight.chunkReady;
		ready &= backLeft.chunkReady;
		ready &= backRight.chunkReady;
		return ready;
	}

	private ChunkData LoadCompletely(Vector2Int position, Vector2Int reference)
	{
		ChunkData chunkData = OpenOrCreateChunkData(position);
		chunkData.references.Add(reference);
		OpenOrCreateChunkData(position + nFront).references.Add(reference);
		OpenOrCreateChunkData(position + nBack).references.Add(reference);
		OpenOrCreateChunkData(position + nLeft).references.Add(reference);
		OpenOrCreateChunkData(position + nRight).references.Add(reference);
		if (!loadQueue.Contains(position)) loadQueue.Add(position);
		return chunkData;
	}

	private ChunkData OpenOrCreateChunkData(Vector2Int position)
	{
		if (data.ContainsKey(position)) return data[position];
		ChunkData chunkData = new ChunkData(position);
		chunkData.StartTerrainLoading();
		data.Add(position, chunkData);
		return chunkData;
	}


	public byte GetBlock(Vector2Int chunk, int x, int y, int z)
	{
		if (!data[chunk].chunkReady) throw new System.Exception($"Chunk {chunk} is not ready");
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
		if (y > 255) return 0;
		if (y < 0) return 0;
		return data[chunk].GetBlocks()[x, y, z];
	}

	public void Modify(Vector2Int chunk, int x, int y, int z, byte blockType)
	{
		ChunkData chunkData = data[chunk];
		chunkData.Modify(x, y, z, blockType);
		chunkData.isDirty = true;
		dirtyChunks.Add(chunkData.position);
		byte lightLevel = BlockTypes.lightLevel[blockType];
		Vector3Int position = new Vector3Int(x, y, z);
		if (lightLevel > 0)
		{
			chunkData.lightSources[position] = lightLevel;
			Debug.Log($"LightSource added ({blockType})");
		}
		else
		{
			if (chunkData.lightSources.ContainsKey(position))
			{
				chunkData.lightSources.Remove(position);
				Debug.Log($"LightSource removed");
			}
		}

	}

	public void UnloadChunk(Vector2Int position)
	{
		RemoveReferenceInNeighbors(position, position);
		RemoveReferenceInNeighbors(position + nFront, position);
		RemoveReferenceInNeighbors(position + nBack, position);
		RemoveReferenceInNeighbors(position + nLeft, position);
		RemoveReferenceInNeighbors(position + nRight, position);
		RemoveReferenceInNeighbors(position + nFront + nLeft, position);
		RemoveReferenceInNeighbors(position + nFront + nRight, position);
		RemoveReferenceInNeighbors(position + nBack + nLeft, position);
		RemoveReferenceInNeighbors(position + nBack + nRight, position);
	}

	private void RemoveReferenceInNeighbors(Vector2Int position, Vector2Int reference)
	{
		RemoveReference(position, reference);
		RemoveReference(position + nFront, reference);
		RemoveReference(position + nBack, reference);
		RemoveReference(position + nLeft, reference);
		RemoveReference(position + nRight, reference);
	}

	private void RemoveReference(Vector2Int position, Vector2Int reference)
	{
		if (data.ContainsKey(position))
		{
			ChunkData chunkData = data[position];
			chunkData.references.Remove(reference);
			if (chunkData.references.Count == 0)
			{
				chunkData.Unload();
				data.Remove(position);
			}
		}
	}
}
