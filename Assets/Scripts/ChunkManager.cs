using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System.Linq;

public class ChunkManager : MonoBehaviour
{
	public Chunk chunkPrefab;
	private ChunkDataManager chunkDataManager;

	public int renderDistance = 16;
	private int[,] surroundingArea;
	
	private Queue<Chunk> chunkPool;
	//private Queue<Vector2Int> generateBuildQueue;
	private Queue<Vector2Int> modifiedRebuildQueue;

	//active chunks
	private System.Object activeChunksLock = new System.Object();
	private List<Vector2Int> activeChunks;

	//chunkmap
	public Dictionary<Vector2Int, Chunk> chunkMap;
	private System.Object chunkMapLock = new System.Object();

	//Camera info (multiple threads)
	private Vector3 cameraPosition;
	private Vector3 cameraForward;

	//ShouldRender thread
	private Thread shouldRenderThread;
	private System.Object shouldRenderLock = new System.Object();
	private List<Vector2Int> shouldRender;

	//should unload
	private System.Object shouldUnloadQueueLock = new System.Object();
	private Queue<Vector2Int> shouldUnloadQueue;

	public void Initialize()
	{
		surroundingArea = new int[renderDistance*2, renderDistance*2];
		chunkDataManager = new ChunkDataManager();
		chunkMap = new Dictionary<Vector2Int, Chunk>();
		chunkPool = new Queue<Chunk>();
		activeChunks = new List<Vector2Int>();
		shouldRender = new List<Vector2Int>();
		shouldUnloadQueue = new Queue<Vector2Int>();
		modifiedRebuildQueue = new Queue<Vector2Int>();
		int chunkPoolSize = renderDistance * renderDistance * 4;
		Debug.Log("Chunk pool size: " + chunkPoolSize);
		for (int i = 0; i < chunkPoolSize; ++i)
		{
			Chunk c = Instantiate(chunkPrefab);
			c.gameObject.SetActive(false);
			c.gameObject.name = "Chunk " + i.ToString();
			chunkPool.Enqueue(c);
			c.transform.SetParent(chunkPrefab.transform.parent);
		}

		shouldRenderThread = new Thread(ShouldRenderThread);
		shouldRenderThread.IsBackground = true;
		shouldRenderThread.Start();
	}

	public void UpdateChunks(Camera mainCamera)
	{
		//Debug.Log("Active chunks: " + activeChunks.Count);
		chunkDataManager.Update();

		cameraPosition = mainCamera.transform.position;
		cameraForward = mainCamera.transform.forward;

		if (modifiedRebuildQueue.Count > 0)
		{
			Vector2Int chunkToRebuild = modifiedRebuildQueue.Dequeue();
			lock (chunkMapLock)
			{
				if (chunkMap.ContainsKey(chunkToRebuild))
				{
					chunkMap[chunkToRebuild].Build(chunkDataManager);
				}
			}
		}
		else
		{
			lock (shouldRenderLock)
			{
				for (int i = 0; i < shouldRender.Count; ++i)
				{
					Vector2Int shouldGeneratePosition = shouldRender[i];
					if (chunkDataManager.CanRender(shouldGeneratePosition))
					{
						shouldRender.RemoveAt(i);
						Chunk chunk = null;
						lock (chunkMap)
						{
							if (chunkMap.ContainsKey(shouldGeneratePosition)) break; //already present
							chunk = chunkPool.Dequeue();
							chunk.Initialize(shouldGeneratePosition);
							chunkMap.Add(shouldGeneratePosition, chunk);
						}
						Debug.Log($"Chunk {shouldGeneratePosition} can render. Adding to build queue");
						chunk.Build(chunkDataManager);
						lock (activeChunksLock)
						{
							activeChunks.Add(shouldGeneratePosition);
						}
						break;
					}
				}
			}
		}

		Vector2Int shouldUnloadPosition = Vector2Int.zero;
		while (true)//loop until everything is unloaded
		{
			lock (shouldUnloadQueueLock)
			{
				if (shouldUnloadQueue.Count == 0) break;
				shouldUnloadPosition = shouldUnloadQueue.Dequeue();
			}
			lock (chunkMapLock)
			{
				if (!chunkMap.ContainsKey(shouldUnloadPosition)) continue;
			}
			lock (activeChunks)
			{
				activeChunks.Remove(shouldUnloadPosition);
			}
			Chunk chunkToUnload;
			lock (chunkMapLock)
			{
				chunkToUnload = chunkMap[shouldUnloadPosition];
				chunkMap.Remove(shouldUnloadPosition);
			}
			chunkToUnload.Unload();
			chunkPool.Enqueue(chunkToUnload);
			//chunkDataManager.data.Remove(shouldUnloadPosition);
		}

		World.activeWorld.debugText.text += $" / Active chunks: {activeChunks.Count}";

	}

	private void ShouldRenderThread()
	{
		List<Vector2Int> visiblePoints = new List<Vector2Int>();
		List<Vector2Int> farAwayPoints = new List<Vector2Int>();
		List<Vector2Int> reallyFarAwayPoints = new List<Vector2Int>();
		while (true)
		{
			Thread.Sleep(8);
			long startTime = System.DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
			visiblePoints.Clear();

			Vector2Int cameraChunkPos;
			cameraChunkPos = new Vector2Int((int)cameraPosition.x / 16, (int)cameraPosition.z / 16);
			Vector3 cameraPositionFloor = new Vector3(cameraPosition.x, 0, cameraPosition.z);
			Vector3 cameraForwardFloor = cameraForward;
			cameraForwardFloor.y = 0;
			cameraForwardFloor.Normalize();
			for (int y = 0; y < renderDistance*2; ++y)
			{
				for (int x = 0; x < renderDistance*2; ++x)
				{
					Vector2Int c = cameraChunkPos - new Vector2Int(renderDistance, renderDistance) + new Vector2Int(x, y);
					Vector3 renderPosition = new Vector3(c.x * 16,0, c.y * 16);
					
					Vector3 toChunk = renderPosition - cameraPositionFloor;
					if (Vector3.Angle(toChunk, cameraForwardFloor) < 45 || Vector2Int.Distance(cameraChunkPos,c)<3) visiblePoints.Add(c);
				}
			}

			Vector2Int[] ordered = visiblePoints.OrderBy(vp => Vector2Int.Distance(cameraChunkPos, vp)).ToArray();


			for (int i = 0; i < ordered.Length; ++i)
			{
				Vector2Int pos = ordered[i];
				bool alreadyPresent = false;
				lock (chunkMapLock)
				{
					alreadyPresent = chunkMap.ContainsKey(pos);
				}
				if(!alreadyPresent)
				{
					if (Vector2Int.Distance(pos, cameraChunkPos) < renderDistance)
					{
						lock (shouldRenderLock)
						{
							if (shouldRender.Count < 16 && !shouldRender.Contains(pos))
							{
								shouldRender.Add(pos);
							}
						}
						break;
					}
					
				}
			}

			lock (activeChunksLock)
			{
				for (int i = 0; i < activeChunks.Count; ++i)
				{
					if (Vector2Int.Distance(cameraChunkPos, activeChunks[i]) > renderDistance)
					{
						farAwayPoints.Add(activeChunks[i]);
					}
				}
			}

			lock (shouldRenderLock)
			{
				for (int i = farAwayPoints.Count - 1; i > -1; --i)
				{
					bool isStillNeededForOtherChunks = false;
					isStillNeededForOtherChunks |= shouldRender.Contains(farAwayPoints[i]);
					isStillNeededForOtherChunks |= shouldRender.Contains(farAwayPoints[i] + new Vector2Int(1, 0));
					isStillNeededForOtherChunks |= shouldRender.Contains(farAwayPoints[i] + new Vector2Int(-1, 0));
					isStillNeededForOtherChunks |= shouldRender.Contains(farAwayPoints[i] + new Vector2Int(0, 1));
					isStillNeededForOtherChunks |= shouldRender.Contains(farAwayPoints[i] + new Vector2Int(0, -1));
					if(isStillNeededForOtherChunks)
					{
						farAwayPoints.RemoveAt(i);
					}
				}
			}

			lock (shouldUnloadQueueLock)
			{
				for (int i = 0; i < farAwayPoints.Count; ++i)
				{
					if (!shouldUnloadQueue.Contains(farAwayPoints[i]))
					{
						shouldUnloadQueue.Enqueue(farAwayPoints[i]);
					}
				}
			}
			
		}
	}

	public void Modify(Vector2Int chunk, int x, int y, int z, byte blockType)
	{
		Debug.Log($"Chunk {chunk} Modifying {x} {y} {z} {blockType}");
		if (!chunkMap.ContainsKey(chunk)) throw new System.Exception("Chunk is not available");
		chunkDataManager.Modify(chunk, x, y, z, blockType);
		chunkMap[chunk].Build(chunkDataManager);
		if (x == 15) modifiedRebuildQueue.Enqueue(chunk + new Vector2Int(1, 0));
		if (x == 0) modifiedRebuildQueue.Enqueue(chunk + new Vector2Int(-1, 0));
		if (z == 15) modifiedRebuildQueue.Enqueue(chunk + new Vector2Int(0, 1));
		if (z == 0) modifiedRebuildQueue.Enqueue(chunk + new Vector2Int(0, -1));
	}

	private void OnDestroy()
	{
		shouldRenderThread.Abort();
		Thread.Sleep(30);
	}
}