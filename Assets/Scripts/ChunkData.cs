using UnityEngine;
using System.Threading;
using System.Collections.Generic;
public class ChunkData
{
	public Vector2Int position;
	private byte[,,] blocks, light;
	public bool terrainReady { get; private set; }
	public bool startedLoadingDetails { get; private set; }
	public bool chunkReady { get; private set; }

	public bool isDirty;

	//devide by chance ( 1 in X )
	const int STRUCTURE_CHANCE_TREE = (int.MaxValue / 100);

	private Thread loadTerrainThread;
	private Thread loadDetailsThread;

	public HashSet<Vector2Int> references;

	public List<StructureInfo> structures;

	public Dictionary<Vector3Int, byte> lightSources;

	public byte[,] highestNonAirBlock;

	public ChunkSaveData saveData;

	private ChunkData front, left, back, right; //neighbours (only exist while loading structures)

	public struct StructureInfo
	{
		public StructureInfo(Vector3Int position, Structure.Type type)
		{
			this.position = position;
			this.type = type;
		}
		public Vector3Int position;
		public Structure.Type type;
	}


	public ChunkData(Vector2Int position)
	{
		this.position = position;
		terrainReady = false;
		startedLoadingDetails = false;
		chunkReady = false;
		isDirty = false;
		references = new HashSet<Vector2Int>();
		lightSources = new Dictionary<Vector3Int, byte>();
		highestNonAirBlock = new byte[16, 16];
	}

	public byte[,,] GetBlocks()
	{
		//if (!chunkReady) throw new System.Exception($"Chunk {position} has not finished loading");
		return blocks;
	}

	public byte[,,] GetLights()
	{
		//if (!chunkReady) throw new System.Exception($"Chunk {position} has not finished loading");
		return light;
	}

	public byte[,,] NewLights()
	{
		//if (!chunkReady) throw new System.Exception($"Chunk {position} has not finished loading");
		light = new byte[16, 256, 16];
		return light;
	}

	public void StartTerrainLoading()
	{
		//Debug.Log($"Chunk {position} start terrain loading");
		loadTerrainThread = new Thread(LoadTerrain);
		loadTerrainThread.IsBackground = true;
		loadTerrainThread.Start();
	}

	public void StartDetailsLoading(ChunkData front, ChunkData left, ChunkData back, ChunkData right)
	{
		//Debug.Log($"Chunk {position} start structure loading");
		//need to temporarily cache chunkdata of neighbors since generation is on another thread
		this.front = front;
		this.left = left;
		this.right = right;
		this.back = back;

		loadDetailsThread = new Thread(LoadDetails);
		loadDetailsThread.IsBackground = true;
		loadDetailsThread.Start();
		startedLoadingDetails = true;
	}

	public void LoadTerrain() //also loads structures INFO
	{
		blocks = new byte[16, 256, 16];
		light = new byte[16, 256, 16];
		Vector2Int worldPos = position * 16;

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

		string h = World.activeWorld.info.seed.ToString() + position.x.ToString() + position.y.ToString();
		int structuresSeed = h.GetHashCode();
		//Debug.Log("Chunk structures seed is " + structuresSeed);
		System.Random rnd = new System.Random(structuresSeed);
		structures = new List<StructureInfo>();
		bool[,] spotsTaken = new bool[16, 16];

		//trees
		for (int y = 1; y < 15; ++y)
		{
			for (int x = 1; x < 15; ++x)
			{
				if (rnd.Next() < STRUCTURE_CHANCE_TREE)
				{
					if (IsSpotFree(spotsTaken, new Vector2Int(x, y), 2))
					{
						spotsTaken[x, y] = true;
						int height = 255;
						while (height > 0)
						{
							if (blocks[x, height, y] == BlockTypes.GRASS)
							{
								Structure.Type structureType = Structure.Type.OAK_TREE_SMALL_1;
								structures.Add(new StructureInfo(new Vector3Int(x, height + 1, y), structureType));
								//place tree function
								//temp
								blocks[x, height + 1, y] = BlockTypes.LOG_OAK;
								blocks[x, height +2, y] = BlockTypes.LOG_OAK;
								blocks[x, height + 3, y] = BlockTypes.LOG_OAK;
								blocks[x, height + 4, y] = BlockTypes.LOG_OAK;
								blocks[x, height + 5 ,y] = BlockTypes.LOG_OAK;
								break;
							}
							height--;
						}
					}
				}
			}
		}


		//already load changes from disk here (apply later)
		saveData = SaveDataManager.instance.Load(position);

		terrainReady = true;
		//Debug.Log($"Chunk {position} terrain ready");

	}

	private bool IsSpotFree(bool[,] spotsTaken, Vector2Int position, int size) //x area is for example size + 1 + size
	{
		bool spotTaken = false;
		for (int y = Mathf.Max(0, position.y-size); y < Mathf.Min(15, position.y + size + 1); ++y)
		{
			for (int x = Mathf.Max(0, position.x-size); x < Mathf.Min(15, position.x + size + 1); ++x)
			{
				spotTaken |= spotsTaken[x, y];
			}
		}
		return !spotTaken;
	}

	private void PlaceStructure(Vector2Int position)
	{

	}

	private void LoadDetails()
	{
		//load structures

		//remove all references to neighbors to avoid them staying in memory when unloading chunks
		front = null;
		left = null;
		right = null;
		back = null;

		//add sky light
		for (int z = 0; z < 16; ++z)
		{
			for (int x = 0; x < 16; ++x)
			{
				byte ray = 15;
				for (int y = 255; y > -1; --y)
				{
					byte block = blocks[x, y, z];
					if (block == 0)
					{
						light[x, y, z] = ray;
					}
					else
					{
						if (block < 128) break;
						light[x, y, z] = ray;
					}
				}
			}
		}
		//Debug.Log($"Chunk {position} structures ready");
		//load changes
		List<ChunkSaveData.C> changes = saveData.changes;
		for (int i = 0; i < changes.Count; ++i)
		{
			ChunkSaveData.C c = changes[i];
			blocks[c.x, c.y, c.z] = c.b;
			byte lightLevel = BlockTypes.lightLevel[c.b];
			if (lightLevel > 0)
			{
				lightSources.Add(new Vector3Int(c.x, c.y, c.z), lightLevel);
			}
		}

		//get highest non-air blocks to speed up light simulation
		for (int z = 0; z < 16; ++z)
		{
			for (int x = 0; x < 16; ++x)
			{
				highestNonAirBlock[x, z] = 0;
				for (int y = 255; y > -1; --y)
				{
					if (blocks[x, y, z] != BlockTypes.AIR)
					{
						highestNonAirBlock[x, z] = (byte)y;
						break;
					}
				}
			}
		}
		chunkReady = true;
		//Debug.Log($"Chunk {position} ready");
	}

	public void Modify(int x, int y, int z, byte blockType)
	{
		if (!chunkReady) throw new System.Exception("Chunk has not finished loading");
		Debug.Log($"Current highest block at {x}x{z} is {highestNonAirBlock[x, z]}");

		saveData.changes.Add(new ChunkSaveData.C((byte)x, (byte)y, (byte)z, blockType));
		blocks[x, y, z] = blockType;
		if (blockType == BlockTypes.AIR)
		{
			if (highestNonAirBlock[x, z] == y)
			{
				highestNonAirBlock[x, z] = 0;
				for (int yy = y; yy > -1; yy--)
				{
					if (blocks[x, yy, z] != BlockTypes.AIR)
					{
						highestNonAirBlock[x, z] = (byte)yy;
						break;
					}
				}
			}
		}
		else
		{
			highestNonAirBlock[x, z] = (byte)Mathf.Max(highestNonAirBlock[x, z], y);
		}
		Debug.Log($"New highest block at {x}x{z} is {highestNonAirBlock[x, z]}");
	}

	public void Unload()
	{
		if (isDirty)
		{
			SaveDataManager.instance.Save(saveData);
		}
	}
}