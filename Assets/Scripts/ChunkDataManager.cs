using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class ChunkDataManager
{
	public Dictionary<Vector2Int, ChunkData> data;
	public TextureMapper textureMapper;

	private List<ChunkData> renderQueue;
	private List<ChunkData> applyingChangesQueue;
	private List<ChunkData> dirtyChunks;

	public ChunkDataManager()
	{
		data = new Dictionary<Vector2Int, ChunkData>();
		textureMapper = new TextureMapper();
		renderQueue = new List<ChunkData>();
		applyingChangesQueue = new List<ChunkData>();
		dirtyChunks = new List<ChunkData>();
	}

	public void Update()
	{
		if (renderQueue.Count > 0)
		{
			//only process first one
			ChunkData chunkData = renderQueue[0];
			Vector2Int position = chunkData.position;
			if (!chunkData.startedLoadingStructures)
			{
				bool canStart = true;
				ChunkData front = data[position + new Vector2Int(0, 1)];
				ChunkData back = data[position + new Vector2Int(0, -1)];
				ChunkData left = data[position + new Vector2Int(-1, 0)];
				ChunkData right = data[position + new Vector2Int(1, 0)];
				canStart &= chunkData.terrainReady;
				canStart &= front.terrainReady;
				canStart &= back.terrainReady;
				canStart &= left.terrainReady;
				canStart &= right.terrainReady;
				if (canStart)
				{
					chunkData.StartStructuresLoading(front, left, back, right);
				}
			}
			else
			{
				if (chunkData.structuresReady)
				{
					applyingChangesQueue.Add(chunkData);
					renderQueue.RemoveAt(0);
				}
			}
		}
		if (applyingChangesQueue.Count > 0)
		{
			ChunkData chunkData = applyingChangesQueue[0];
			if (chunkData.chunkReady)
			{
				applyingChangesQueue.RemoveAt(0);
			}
		}
		if (dirtyChunks.Count > 0)
		{
			SaveDataManager.instance.Save(dirtyChunks[0].saveData);
			dirtyChunks[0].isDirty = false;
			dirtyChunks.RemoveAt(0);
		}
	}

	public bool CanRender(Vector2Int chunk)
	{
		if (data.ContainsKey(chunk))
		{
			ChunkData chunkData = data[chunk];
			if (!chunkData.chunkReady)
			{
				//data exists but is either still loading terrain, placing structures or applying user changes
				if (applyingChangesQueue.Contains(chunkData))
				{
					//already final stages of being ready, currently applying user changes;
					return false;
				}
				if (!renderQueue.Contains(chunkData))
				{
					//only flagged for terrain loading, add to renderQueue to start loading structures/terrain
					//load terrain data for neighbors if they don't exist yet
					renderQueue.Add(chunkData);
					StartChunkLoadingIfNecessary(chunk + new Vector2Int(1, 0));
					StartChunkLoadingIfNecessary(chunk + new Vector2Int(-1, 0));
					StartChunkLoadingIfNecessary(chunk + new Vector2Int(0, 1));
					StartChunkLoadingIfNecessary(chunk + new Vector2Int(0, -1));
					return false;
				}
				else
				{
					//currently busy with either terrain loading or structures
					//has already been flagged to do structures and user changes
					return false;
				}
			}
			return true;
		}
		else
		{
			ChunkData chunkData = StartChunkLoadingIfNecessary(chunk);
			renderQueue.Add(chunkData);
			StartChunkLoadingIfNecessary(chunk + new Vector2Int(1, 0));
			StartChunkLoadingIfNecessary(chunk + new Vector2Int(-1, 0));
			StartChunkLoadingIfNecessary(chunk + new Vector2Int(0, 1));
			StartChunkLoadingIfNecessary(chunk + new Vector2Int(0, -1));
			return false;
		}
	}

	private ChunkData StartChunkLoadingIfNecessary(Vector2Int position)
	{
		if (data.ContainsKey(position)) return data[position];
		ChunkData chunkData = new ChunkData(position);
		data.Add(position, chunkData);
		chunkData.StartTerrainLoading();
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
		data[chunk].Modify(x, y, z, blockType);
		data[chunk].isDirty = true;
		dirtyChunks.Add(data[chunk]);
	}
}
