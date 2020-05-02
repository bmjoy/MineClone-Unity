using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class World : MonoBehaviour
{
	public Camera mainCamera;
	public ChunkManager chunkManager;
	public int seed = 0;
	private void Awake()
	{
		chunkManager.Initialize();
		SimplexNoise.Noise.Seed = seed;
	}
	void Update()
    {
		chunkManager.UpdateChunks(mainCamera);
	}

	public void Modify(int x, int y, int z, char blockType)
	{
		if (y < 0 || y > 255)
		{
			Debug.LogWarning("This is outside build limit");
			return;
		}
		int chunkX = Mathf.FloorToInt(x / 16f);
		int chunkY = Mathf.FloorToInt(z / 16f);
		int relativeX = x - (chunkX*16);
		int relativeZ = z - (chunkY*16);

		
		Debug.Log(blockType);
		Debug.Log($"World Modifying {x} {y} {z} {(int)blockType}. Chunk {chunkX} {chunkY}. Relative {relativeX} {y} {relativeZ}");
		chunkManager.Modify(new Vector2Int(chunkX, chunkY), relativeX, y, relativeZ, blockType);
	}
}