using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System.Linq;

public class ChunkManager : MonoBehaviour
{
	public Chunk chunkPrefab;
	private ChunkDataManager chunkDataManager;
	
	private int[,] surroundingArea = new int[32, 32];
	public int renderDistance = 16;
	private Queue<Chunk> chunkPool;
	private Queue<Vector2Int> generateBuildQueue;
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
	private Thread calculateShouldRenderThread;
	private List<Vector2Int> shouldRender;
	private System.Object calculateShouldRenderLock = new System.Object();
	
	//ShouldGenerate thread
	private Thread shouldGenerateThread;
	private System.Object shouldGenerateQueueLock = new System.Object();
	private Queue<Vector2Int> shouldGenerateQueue;

	//ShouldUnload thread
	private Thread shouldUnloadThread;
	private System.Object shouldUnloadQueueLock = new System.Object();
	private Queue<Vector2Int> shouldUnloadQueue;

	public void Initialize()
	{
		chunkDataManager = new ChunkDataManager();
		chunkMap = new Dictionary<Vector2Int, Chunk>();
		chunkPool = new Queue<Chunk>();
		activeChunks = new List<Vector2Int>();
		shouldRender = new List<Vector2Int>();
		shouldGenerateQueue = new Queue<Vector2Int>();
		shouldUnloadQueue = new Queue<Vector2Int>();
		generateBuildQueue = new Queue<Vector2Int>();
		modifiedRebuildQueue = new Queue<Vector2Int>();
		for (int i = 0; i < 2048; ++i)
		{
			Chunk c = Instantiate(chunkPrefab);
			c.gameObject.SetActive(false);
			chunkPool.Enqueue(c);
			c.transform.SetParent(chunkPrefab.transform.parent);
		}

		calculateShouldRenderThread = new Thread(CalculateShouldRenderThread);
		calculateShouldRenderThread.IsBackground = true;
		calculateShouldRenderThread.Start();

		shouldGenerateThread = new Thread(ShouldGenerateThread);
		shouldGenerateThread.IsBackground = true;
		shouldGenerateThread.Start();

		shouldUnloadThread = new Thread(ShouldUnloadThread);
		shouldUnloadThread.IsBackground = true;
		shouldUnloadThread.Start();
	}

	public void UpdateChunks(Camera mainCamera)
	{
		chunkDataManager.Update();

		cameraPosition = mainCamera.transform.position;
		cameraForward = mainCamera.transform.forward;


		bool shouldGenerate = false;
		Vector2Int shouldGeneratePosition=Vector2Int.zero;
		lock (shouldGenerateQueueLock)
		{
			if (shouldGenerateQueue.Count > 0)
			{
				shouldGeneratePosition = shouldGenerateQueue.Peek();
				if (chunkDataManager.CanRender(shouldGeneratePosition))
				{
					shouldGenerateQueue.Dequeue(); // actually dequeue if can render
					shouldGenerate = true;
				}
			}
		}
		lock (chunkMapLock)
		{
			if (chunkMap.ContainsKey(shouldGeneratePosition)) shouldGenerate = false;
		}
		if (shouldGenerate)
		{
			Chunk chunk = chunkPool.Dequeue();
			chunk.Initialize(shouldGeneratePosition);
			chunkMap.Add(shouldGeneratePosition, chunk);
			generateBuildQueue.Enqueue(shouldGeneratePosition);

		}

		bool shouldUnload = false;
		Vector2Int shouldUnloadPosition = Vector2Int.zero;
		lock (shouldUnloadQueueLock)
		{
			if (shouldUnloadQueue.Count > 0)
			{
				shouldUnloadPosition = shouldUnloadQueue.Dequeue();
				shouldUnload = true;
			}
		}
		lock (chunkMapLock)
		{
			if (!chunkMap.ContainsKey(shouldUnloadPosition)) shouldUnload = false;
		}
		if (shouldUnload)
		{
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
		}

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
			if (generateBuildQueue.Count > 0)
			{
				Vector2Int chunkToBuild = generateBuildQueue.Dequeue();
				Chunk chunk=null;
				lock (chunkMapLock)
				{
					if (chunkMap.ContainsKey(chunkToBuild))
					{
						chunk = chunkMap[chunkToBuild];
					}
				}
				if (chunk!=null)
				{
					chunk.Build(chunkDataManager);
					lock (activeChunksLock)
					{
						activeChunks.Add(shouldGeneratePosition);
					}
				}
			}
		}

		
	}

	private void CalculateShouldRenderThread()
	{
		List<Vector2Int> visiblePoints = new List<Vector2Int>();

		while (true)
		{
			Thread.Sleep(30);
			visiblePoints.Clear();

			Vector2Int cameraChunkPos;
			cameraChunkPos = new Vector2Int((int)cameraPosition.x / 16, (int)cameraPosition.z / 16);
			Vector3 cameraPositionFloor = new Vector3(cameraPosition.x, 0, cameraPosition.z);
			Vector3 cameraForwardFloor = cameraForward;
			cameraForwardFloor.y = 0;
			cameraForwardFloor.Normalize();
			for (int y = 0; y < 32; ++y)
			{
				for (int x = 0; x < 32; ++x)
				{
					Vector2Int c = cameraChunkPos - new Vector2Int(16, 16) + new Vector2Int(x, y);
					Vector3 renderPosition = new Vector3(c.x * 16,0, c.y * 16);
					
					Vector3 toChunk = renderPosition - cameraPositionFloor;
					if (Vector3.Angle(toChunk, cameraForwardFloor) < 45 || Vector2Int.Distance(cameraChunkPos,c)<3) visiblePoints.Add(c);
				}
			}

			Vector2Int[] ordered = visiblePoints.OrderBy(vp => Vector2Int.Distance(cameraChunkPos, vp)).ToArray();

			lock (calculateShouldRenderLock)
			{
				shouldRender.Clear();
				for (int i = 0; i < ordered.Count(); ++i)
				{
					shouldRender.Add(ordered.ElementAt(i));
				}
			}
		}
	}

	private void ShouldGenerateThread()
	{
		while (true)
		{
			Thread.Sleep(30);
			lock (calculateShouldRenderLock)
			{
				Vector2Int cameraChunkPosition = new Vector2Int(
					(int)cameraPosition.x / 16,
					(int)cameraPosition.z / 16);
				for (int i = 0; i < shouldRender.Count; ++i)
				{
					Vector2Int pos = shouldRender[i];
					lock (chunkMapLock)
					{
						if (!chunkMap.ContainsKey(pos))
						{
							if (Vector2Int.Distance(pos, cameraChunkPosition) < renderDistance)
							{
								lock (shouldGenerateQueueLock)
								{
									if (shouldGenerateQueue.Count < 2)
									{
										shouldGenerateQueue.Enqueue(pos);
									}
								}
								break;
							}
						}
					}
				}
			}
		}
	}

	private void ShouldUnloadThread()
	{
		while (true)
		{
			Thread.Sleep(5);
			Vector2Int cameraChunkPosition = new Vector2Int(
					(int)cameraPosition.x / 16,
					(int)cameraPosition.z / 16);
			bool shouldUnload = false;
			Vector2Int chunkPosition = Vector2Int.zero;
			lock (activeChunksLock)
			{
				for (int i = 0; i < activeChunks.Count; ++i)
				{
					if (Vector2Int.Distance(cameraChunkPosition, activeChunks[i]) > renderDistance)
					{
						shouldUnload = true;
						chunkPosition = activeChunks[i];
						break;
					}
				}
			}
			if (shouldUnload)
			{
				lock (shouldUnloadQueueLock)
				{
					if (!shouldUnloadQueue.Contains(chunkPosition))
					{
						shouldUnloadQueue.Enqueue(chunkPosition);
					}
				}
			}
		}
	}

	public void Modify(Vector2Int chunk, int x, int y, int z, char blockType)
	{
		Debug.Log($"Chunk {chunk} Modifying {x} {y} {z} {blockType}");
		if (!chunkMap.ContainsKey(chunk)) throw new System.Exception("Chunk is not available");
		chunkDataManager.data[chunk].Modify(x, y, z, blockType);
		chunkMap[chunk].Build(chunkDataManager);
		//if (x == 15) chunkMap[chunk + new Vector2Int(1, 0)].Build( chunkDataManager);
		//if (x == 0) chunkMap[chunk + new Vector2Int(-1, 0)].Build( chunkDataManager);
		//if (z == 15) chunkMap[chunk + new Vector2Int(0, 1)].Build( chunkDataManager);
		//if (z == 0) chunkMap[chunk + new Vector2Int(0, -1)].Build( chunkDataManager);
		if (x == 15) modifiedRebuildQueue.Enqueue(chunk + new Vector2Int(1, 0));
		if (x == 0) modifiedRebuildQueue.Enqueue(chunk + new Vector2Int(-1, 0));
		if (z == 15) modifiedRebuildQueue.Enqueue(chunk + new Vector2Int(0, 1));
		if (z == 0) modifiedRebuildQueue.Enqueue(chunk + new Vector2Int(0, -1));
	}

	private void OnDestroy()
	{
		calculateShouldRenderThread.Abort();
		shouldGenerateThread.Abort();
		shouldUnloadThread.Abort();
		Thread.Sleep(30);
	}
}