using UnityEngine;
using System.Threading;
using System.Collections.Generic;
public class ChunkData
{
	public Vector2Int position;
	private char[,,] blocks;
	private bool ready = false;
	private Thread loadThread;
	public ChunkData(Vector2Int position)
	{
		this.position = position;
		loadThread = new Thread(Load);
		loadThread.IsBackground = true;
		loadThread.Start();
	}

	public bool Ready()
	{
		return ready;
	}

	public char[,,] GetBlocks()
	{
		if (!ready) throw new System.Exception("Chunk has not finished loading");
		return blocks;
	}

	public void Load()
	{
		blocks = new char[16, 256, 16];
		Vector2Int worldPos = position * 16;

		//System.Random random = new System.Random(World.activeWorld.seed);
		//HashSet<Vector2Int> treePositions = new HashSet<Vector2Int>();
		

		for (int z = 0; z < 16; ++z)
		{
			for (int x = 0; x < 16; ++x)
			{
				int noiseX = worldPos.x + x;
				int noiseZ = worldPos.y + z;
				float height = SimplexNoise.Noise.CalcPixel2D(noiseX, noiseZ+50000, 0.01f);
				height = height  * 16 + 64;
				int heightInt = (int)height;

				float bedrock = SimplexNoise.Noise.CalcPixel2D(noiseX, noiseZ+50000, 1f);
				bedrock = bedrock * 3 + 1;
				int bedrockInt = (int)bedrock;

				for (int y = 0; y < 256; ++y)
				{
					//bedrock
					if (y < bedrockInt)
					{
						blocks[x, y, z] = BlockTypes.BEDROCK;
						continue;
					}

					//air
					if (y > heightInt)
					{
						blocks[x, y, z] = BlockTypes.AIR;
						continue;
					}


					//ores
					float o1 = SimplexNoise.Noise.CalcPixel3D(noiseX + 50000, y, noiseZ, 0.1f);
					float o2 = SimplexNoise.Noise.CalcPixel3D(noiseX + 40000, y, noiseZ, 0.1f);
					float o3 = SimplexNoise.Noise.CalcPixel3D(noiseX + 30000, y, noiseZ, 0.04f);
					float o4 = SimplexNoise.Noise.CalcPixel3D(noiseX + 60000, y, noiseZ, 0.1f);
					float o5 = SimplexNoise.Noise.CalcPixel3D(noiseX + 70000, y, noiseZ, 0.1f);
					float o6 = SimplexNoise.Noise.CalcPixel3D(noiseX + 80000, y, noiseZ, 0.03f);

					float heightGradient = Mathf.Pow(Mathf.Clamp01(y / 128f), 2f);

					//caves
					int c = 1;
					float c1 = SimplexNoise.Noise.CalcPixel3D(noiseX, y, noiseZ, 0.1f);
					float c2 = SimplexNoise.Noise.CalcPixel3D(noiseX, y, noiseZ, 0.04f);
					float c3 = SimplexNoise.Noise.CalcPixel3D(noiseX, y, noiseZ, 0.02f);
					float c4 = SimplexNoise.Noise.CalcPixel3D(noiseX, y, noiseZ, 0.01f);
					
					c1 += (heightGradient);
					if (c1 < .5 && c2 < .5 && c3 < .5 && c4 < .5)
					{
						blocks[x, y, z] = BlockTypes.AIR;
						continue;
					}

					

					//grass level
					if (y == heightInt)
					{
						blocks[x, y, z] = BlockTypes.GRASS;
						continue;
					}

					//dirt
					if (y >= heightInt-4)
					{
						blocks[x, y, z] = BlockTypes.DIRT;
						continue;
					}

					

					o5 += (heightGradient);
					if (y < 64 && o5 < .04)
					{
						blocks[x, y, z] = BlockTypes.GOLD;
						continue;
					}

					if (y < 16 && Mathf.Pow(o2, 4f) > .7 && o3 < .1)
					{
						blocks[x, y, z] = BlockTypes.DIAMOND;
						continue;
					}

					if (o4 < .1 && o6 > .8)
					{
						blocks[x, y, z] = BlockTypes.IRON;
						continue;
					}

					if (o1 < .08)
					{
						blocks[x, y, z] = BlockTypes.COAL;
						continue;
					}

					//remaining is stone
					blocks[x, y, z] = BlockTypes.STONE;
					continue;
				}
			}
		}
		ready = true;
	}

	public void Modify(int x, int y, int z, char blockType)
	{
		if (!ready) throw new System.Exception("Chunk has not finished loading");
		blocks[x, y, z] = blockType;
	}
}