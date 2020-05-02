using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class World : MonoBehaviour
{
	public static World activeWorld;
	public Camera mainCamera;
	public ChunkManager chunkManager;
	public int seed = 0;
	private bool didModifyThisFrame = false;
	private void Awake()
	{
		activeWorld = this;
		chunkManager.Initialize();
		SimplexNoise.Noise.Seed = seed;
	}
	void LateUpdate()
    {
		//update chunks if no modifications have happened this frame
		//only rebuild 1 chunk per frame to avoid framedrops
		if(!didModifyThisFrame) chunkManager.UpdateChunks(mainCamera);
		didModifyThisFrame = false;
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
		Debug.Log($"World Modifying {x} {y} {z} {(int)blockType}. Chunk {chunkX} {chunkY}. Relative {relativeX} {y} {relativeZ}");
		chunkManager.Modify(new Vector2Int(chunkX, chunkY), relativeX, y, relativeZ, blockType);
		didModifyThisFrame = true;
	}
}