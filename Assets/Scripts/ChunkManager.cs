using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System.Linq;

public class ChunkManager : MonoBehaviour
{
	public Chunk chunkPrefab;
	private ChunkDataManager chunkDataManager;

	private int renderDistance;
	private int maximumLoadQueueSize;
	private int[,] surroundingArea;
	
	private Queue<Chunk> chunkPool;
	private Queue<Vector2Int> modifiedRebuildQueue;
	public Dictionary<Vector2Int, Chunk> chunkMap;

	//Camera info (multiple threads)
	private Vector3 cameraPosition;
	private Vector3 cameraForward;

	//ShouldRender thread
	private Thread shouldRenderThread;
	private System.Object shouldRenderLock = new System.Object();
	private bool shouldRenderWaitForUpdate = false;
	private volatile List<Vector2Int> loadQueue;
	private volatile Queue<Vector2Int> unloadQueue;
	private volatile List<Vector2Int> activeChunks;

	private readonly Vector2Int nFront = new Vector2Int(0, 1);
	private readonly Vector2Int nBack = new Vector2Int(0, -1);
	private readonly Vector2Int nLeft = new Vector2Int(-1, 0);
	private readonly Vector2Int nRight = new Vector2Int(1, 0);

	public void Initialize()
	{
		renderDistance = GameManager.instance.gameSettings.RenderDistance;
		maximumLoadQueueSize = GameManager.instance.gameSettings.maximumLoadQueueSize;

		surroundingArea = new int[renderDistance*2, renderDistance*2];
		chunkDataManager = new ChunkDataManager();
		chunkMap = new Dictionary<Vector2Int, Chunk>();
		chunkPool = new Queue<Chunk>();
		activeChunks = new List<Vector2Int>();
		loadQueue = new List<Vector2Int>();
		unloadQueue = new Queue<Vector2Int>();
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
		UnityEngine.Profiling.Profiler.BeginSample("UPDATING CHUNKS");
		//Debug.Log("Active chunks: " + activeChunks.Count);
		chunkDataManager.Update();

		cameraPosition = mainCamera.transform.position;
		cameraForward = mainCamera.transform.forward;

		int loadQueueCount = 0;

		UnityEngine.Profiling.Profiler.BeginSample("LOCK SHOULD RENDER");

		lock (shouldRenderLock)
		{

			loadQueueCount = loadQueue.Count;
			if (modifiedRebuildQueue.Count > 0)
			{

				UnityEngine.Profiling.Profiler.BeginSample("MODIFY CHUNK");
				Vector2Int position = modifiedRebuildQueue.Dequeue();
				if (activeChunks.Contains(position))
				{
					chunkMap[position].Build(chunkDataManager);
				}
				UnityEngine.Profiling.Profiler.EndSample();

			}
			else
			{

				UnityEngine.Profiling.Profiler.BeginSample("UNLOADING");
				while (true)//loop until everything is unloaded
				{

					if (unloadQueue.Count == 0) break;

					Vector2Int position = unloadQueue.Dequeue();

					if (!activeChunks.Contains(position)) continue;

					Chunk chunk = chunkMap[position];
					chunk.Unload();
					chunkPool.Enqueue(chunk);

					activeChunks.Remove(position);
					chunkMap.Remove(position);
					chunkDataManager.UnloadChunk(position);
				}
				UnityEngine.Profiling.Profiler.EndSample();

				UnityEngine.Profiling.Profiler.BeginSample("BUILD CHUNK CHECK");

				bool buildChunk = false;
				Vector2Int chunkToBuild = Vector2Int.zero;
				for (int i = 0; i < loadQueue.Count; ++i)
				{
					Vector2Int position = loadQueue[i];
					if (chunkDataManager.Load(position))
					{
						if (!buildChunk)
						{
							buildChunk = true;
							chunkToBuild = position;
							//Debug.Log("Building chunk " + position);
						}
					}
				}

				UnityEngine.Profiling.Profiler.EndSample();

				UnityEngine.Profiling.Profiler.BeginSample("BUILD CHUNK");

				if (buildChunk)
				{
					loadQueue.Remove(chunkToBuild);
					Chunk chunk = null;
					if (!chunkMap.ContainsKey(chunkToBuild))
					{
						chunk = chunkPool.Dequeue();
						chunk.Initialize(chunkToBuild);
						chunkMap.Add(chunkToBuild, chunk);
						chunk.Build(chunkDataManager);
						activeChunks.Add(chunkToBuild);
					}
				}

				UnityEngine.Profiling.Profiler.EndSample();

			}
		}
		shouldRenderWaitForUpdate = false;
		UnityEngine.Profiling.Profiler.EndSample();
		int activeChunksCount = activeChunks.Count;
		int rebuildQueueCount = modifiedRebuildQueue.Count;
		int chunksInMemoryCount = chunkDataManager.GetChunksInMemoryCount();
		World.activeWorld.debugText.text += $" / Chunks (Q:{loadQueueCount} R:{rebuildQueueCount} A:{activeChunksCount} M:{chunksInMemoryCount})";
		UnityEngine.Profiling.Profiler.EndSample();
	}

	private void ShouldRenderThread()
	{
		List<Vector2Int> visiblePoints = new List<Vector2Int>();
		List<Vector2Int> inRangePoints = new List<Vector2Int>();
		List<Vector2Int> copyOfActiveChunks = new List<Vector2Int>();
		Queue<Vector2Int> copyOfUnload = new Queue<Vector2Int>();
		Queue<Vector2Int> copyOfLoad = new Queue<Vector2Int>();
		while (true)
		{
			visiblePoints.Clear();
			inRangePoints.Clear();
			copyOfActiveChunks.Clear();
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

					bool inRange = toChunk.magnitude < (renderDistance * 16);
					bool inAngle = Vector3.Angle(toChunk, cameraForwardFloor) < 70;
					bool isClose = toChunk.magnitude < (16 * 3);
					if (inRange) inRangePoints.Add(c);
					if ((inAngle && inRange) || isClose) visiblePoints.Add(c) ;
				}
			}

			List<Vector2Int> ordered = visiblePoints.OrderBy(vp => Vector2Int.Distance(cameraChunkPos, vp)).ToList<Vector2Int>();

			while (shouldRenderWaitForUpdate) Thread.Sleep(8);
			shouldRenderWaitForUpdate = true;

			long startTime;
			startTime = TimeStamp();
			lock (shouldRenderLock)
			{
				for (int i = 0; i < activeChunks.Count; ++i)
				{
					copyOfActiveChunks.Add(activeChunks[i]);
				}
			}
			//Debug.Log($"Locked main thread for {TimeStamp() - startTime} MS to copy active chunk list");

			for (int i = copyOfActiveChunks.Count-1; i >-1; --i)
			{
				Vector2Int position = copyOfActiveChunks[i];
				if (!inRangePoints.Contains(position))
				{
					copyOfUnload.Enqueue(position);
				}
			}

			for (int i = 0; i < ordered.Count; ++i)
			{
				if (copyOfLoad.Count == maximumLoadQueueSize) break;
				Vector2Int position = ordered[i];
				if (!copyOfActiveChunks.Contains(position))
				{
					copyOfLoad.Enqueue(position);
				}
			}

			startTime = TimeStamp();
			lock (shouldRenderLock)
			{
				while (copyOfUnload.Count > 0)
				{
					unloadQueue.Enqueue(copyOfUnload.Dequeue());
				}
				while (copyOfLoad.Count > 0)
				{
					if (loadQueue.Count == maximumLoadQueueSize) break;
					Vector2Int position = copyOfLoad.Dequeue();
					if (!loadQueue.Contains(position))
					{
						loadQueue.Add(position);
					}
				}
			}
			//Debug.Log($"Locked main thread for {TimeStamp() - startTime} MS to schedule loading");
		}
	}

	public bool Modify(Vector2Int chunk, int x, int y, int z, byte blockType)
	{
		if (modifiedRebuildQueue.Count > 0) return false;
		if (!chunkMap.ContainsKey(chunk)) throw new System.Exception("Chunk is not available");
		Debug.Log($"Chunk {chunk} Modifying {x} {y} {z} {blockType}");
		chunkDataManager.Modify(chunk, x, y, z, blockType);
		modifiedRebuildQueue.Enqueue(chunk);
		modifiedRebuildQueue.Enqueue(chunk + nLeft);
		modifiedRebuildQueue.Enqueue(chunk + nRight);
		modifiedRebuildQueue.Enqueue(chunk + nFront);
		modifiedRebuildQueue.Enqueue(chunk + nBack);
		modifiedRebuildQueue.Enqueue(chunk + nFront+ nLeft);
		modifiedRebuildQueue.Enqueue(chunk + nFront+ nRight);
		modifiedRebuildQueue.Enqueue(chunk + nBack+ nLeft);
		modifiedRebuildQueue.Enqueue(chunk + nBack+ nRight);
		return true;
	}

	private void OnDestroy()
	{
		shouldRenderThread.Abort();
		Thread.Sleep(30);
	}

	private long TimeStamp()
	{
		return System.DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
	}
}