using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chunk : MonoBehaviour
{
	[SerializeField] private MeshFilter meshFilter;
	[SerializeField] private MeshRenderer meshRenderer;
	[SerializeField] private MeshCollider meshCollider;
	public Vector2Int position;
	private Mesh mesh;
	private List<Vector3> vertices;
	private List<int> triangles;
	private List<Vector3> normals;
	private List<Vector2> uvs;
	private List<Color32> colors;

	//for generating
	private ChunkData[,] chunkMap;
	private readonly Vector2Int nFront = new Vector2Int(0, 1);
	private readonly Vector2Int nBack = new Vector2Int(0, -1);
	private readonly Vector2Int nLeft = new Vector2Int(-1, 0);
	private readonly Vector2Int nRight = new Vector2Int(1, 0);

	public void Awake()
	{
		mesh = new Mesh();
		meshFilter.sharedMesh = mesh;
		mesh.name = "ChunkMesh";
		mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
		mesh.MarkDynamic();
		meshCollider.sharedMesh = mesh;
		vertices = new List<Vector3>();
		normals = new List<Vector3>();
		uvs = new List<Vector2>();
		triangles = new List<int>();
		colors = new List<Color32>();
		chunkMap = new ChunkData[3, 3]; //start at backleft
	}

	public void Initialize(Vector2Int position)
	{
		this.position = position;
	}

	public void Build(ChunkDataManager chunkDataManager)
	{
//#if UNITY_EDITOR
		UnityEngine.Profiling.Profiler.BeginSample("BUILDING CHUNK");
//#endif
		Vector2Int renderPosition = 16 * position;
		transform.position = new Vector3(renderPosition.x, 0, renderPosition.y);
		mesh.Clear();

		UnityEngine.Profiling.Profiler.BeginSample("GRABBING BLOCK DATA");

		ChunkData chunkData = chunkDataManager.data[position];
		ChunkData front = chunkDataManager.data[position + nFront];
		ChunkData back = chunkDataManager.data[position + nBack];
		ChunkData left = chunkDataManager.data[position + nLeft];
		ChunkData right = chunkDataManager.data[position + nRight];
		ChunkData frontLeft = chunkDataManager.data[position + nFront+nLeft];
		ChunkData frontRight = chunkDataManager.data[position + nFront + nRight];
		ChunkData backLeft = chunkDataManager.data[position + nBack + nLeft];
		ChunkData backRight = chunkDataManager.data[position + nBack + nRight];

		byte[,,] lightMap = new byte[48, 256, 48];

		chunkMap[0, 0] = backLeft;
		chunkMap[1, 0] = back;
		chunkMap[2, 0] = backRight;
		chunkMap[0, 1] = left;
		chunkMap[1, 1] = chunkData;
		chunkMap[2, 1] = right;
		chunkMap[0, 2] = frontLeft;
		chunkMap[1, 2] = front;
		chunkMap[2, 2] = frontRight;

		UnityEngine.Profiling.Profiler.EndSample();

		UnityEngine.Profiling.Profiler.BeginSample("SIMULATING LIGHT");


		Queue<Vector3Int> simulateQueue = new Queue<Vector3Int>();
		//sunray tracing needs to start above the highest non-air block to increase performance
		//all blocks above that block need to be set to 15
		for (int z =0; z < 48; ++z)
		{
			for (int x = 0; x < 48; ++x)
			{
				if ((x % 47) * (z % 47) == 0) //filters outer edges
				{
					//Debug.Log($"these should at least 0 or 47  ->  {x} {z}"); 
					for (int yy = 0; yy < 256; ++yy) //dont do outer edges
					{
						lightMap[x, yy, z] = 15; //set all edges to 15 to stop tracing at edges
					}
					continue;
				}
				int y = GetHighestNonAir(chunkMap, x, z);
				if (x < 46) y = Mathf.Max(y, GetHighestNonAir(chunkMap, x + 1, z));
				if (x > 1) y = Mathf.Max(y, GetHighestNonAir(chunkMap, x - 1, z));
				if (z < 46) y = Mathf.Max(y, GetHighestNonAir(chunkMap, x, z + 1));
				if (z > 1) y = Mathf.Max(y, GetHighestNonAir(chunkMap, x, z - 1));
				y = Mathf.Min(y + 1, 255);
				simulateQueue.Enqueue(new Vector3Int(x, y, z));

				while (y < 255)
				{
					lightMap[x, y, z] = 15;
					y++;
				}
			}
		}

		for (int y = 0; y < 3; ++y)
		{
			for (int x = 0; x < 3; ++x)
			{
				foreach (KeyValuePair<Vector3Int, byte> kv in chunkMap[x,y].lightSources)
				{
					Vector3Int position = kv.Key;
					int lX = (16 * x) + position.x;
					int lY = position.y;
					int lZ = (16 * y) + position.z;
					lightMap[lX, lY, lZ] = kv.Value;
					simulateQueue.Enqueue(new Vector3Int(lX, lY, lZ));
				}
			}
		}

		int simulateCount = 0;
		while (simulateQueue.Count > 0)
		{
			Vector3Int position = simulateQueue.Dequeue();
			int x = position.x;
			int y = position.y;
			int z = position.z;

			byte bR = (x == 47 ? BlockTypes.BEDROCK : GetBlockFromMap(chunkMap, x + 1, y, z));
			byte bL = (x == 0 ? BlockTypes.BEDROCK : GetBlockFromMap(chunkMap, x - 1, y, z));
			byte bF = (z == 47 ? BlockTypes.BEDROCK : GetBlockFromMap(chunkMap, x, y, z + 1));
			byte bB = (z == 0 ? BlockTypes.BEDROCK : GetBlockFromMap(chunkMap, x, y, z - 1));
			byte bU = (y == 255 ? BlockTypes.BEDROCK : GetBlockFromMap(chunkMap, x, y + 1, z));
			byte bD = (y == 0 ? BlockTypes.BEDROCK : GetBlockFromMap(chunkMap, x, y - 1, z));

			byte light = lightMap[x, y, z];

			if (bR == BlockTypes.AIR)
			{
				byte lightR = lightMap[x + 1, y, z];
				if (lightR < light - 1)
				{
					lightMap[x + 1, y, z] = (byte)(light - 1);
					simulateQueue.Enqueue(new Vector3Int(x + 1, y, z));
				}
			}
			if (bL == BlockTypes.AIR)
			{
				byte lightL = lightMap[x - 1, y, z];
				if (lightL < light - 1)
				{
					lightMap[x - 1, y, z] = (byte)(light - 1);
					//if (x - 1 == 0) Debug.LogError("THIS SHOULD NOT HAPPEN");
					simulateQueue.Enqueue(new Vector3Int(x - 1, y, z));
				}
			}
			if (bD == BlockTypes.AIR)
			{
				if (light == 15)
				{
					lightMap[x, y - 1, z] = light;
					simulateQueue.Enqueue(new Vector3Int(x, y - 1, z));
				}
				else
				{
					byte lightD = lightMap[x, y - 1, z];
					if (lightD < light - 1)
					{
						lightMap[x, y - 1, z] = (byte)(light - 1);
						simulateQueue.Enqueue(new Vector3Int(x, y - 1, z));
					}
				}
			}
			if (bU == BlockTypes.AIR)
			{
				byte lightU = lightMap[x, y + 1, z];
				if (lightU < light - 1)
				{
					lightMap[x, y + 1, z] = (byte)(light - 1);
					simulateQueue.Enqueue(new Vector3Int(x, y + 1, z));
				}
			}
			if (bF == BlockTypes.AIR)
			{
				byte lightF = lightMap[x, y, z + 1];
				if (lightF < light - 1)
				{
					lightMap[x, y, z + 1] = (byte)(light - 1);
					simulateQueue.Enqueue(new Vector3Int(x, y, z + 1));
				}
			}
			if (bB == BlockTypes.AIR)
			{
				byte lightB = lightMap[x, y, z - 1];
				if (lightB < light - 1)
				{
					lightMap[x, y, z - 1] = (byte)(light - 1);
					simulateQueue.Enqueue(new Vector3Int(x, y, z - 1));
				}
			}
			simulateCount++;
		}
		//Debug.Log("Did " + simulateCount + " light simulations");

		UnityEngine.Profiling.Profiler.EndSample();


		UnityEngine.Profiling.Profiler.BeginSample("CREATING FACES");
		TextureMapper textureMapper = GameManager.instance.textureMapper;
		for (int z = 0; z < 16; ++z)
		{
			for (int y = 0; y < 256; ++y)
			{
				for (int x = 0; x < 16; ++x)
				{
					byte c = chunkData.GetBlocks()[x, y, z];
					if (c != BlockTypes.AIR)
					{
						int lx = x + 16;
						int ly = y;
						int lz = z + 16;

						byte bR = (x == 15 ? right.GetBlocks()[0, y, z] : chunkData.GetBlocks()[x+1, y, z]);
						byte bL =( x == 0 ? left.GetBlocks()[15, y, z] : chunkData.GetBlocks()[x-1, y, z]);
						byte bF =( z == 15 ? front.GetBlocks()[x, y, 0] : chunkData.GetBlocks()[x, y, z+1]);
						byte bB = (z == 0 ? back.GetBlocks()[x, y, 15] : chunkData.GetBlocks()[x, y, z-1]);
						byte bU = (y == 255 ? BlockTypes.AIR : chunkData.GetBlocks()[x, y + 1, z]);
						byte bD = (y == 0 ? BlockTypes.AIR : chunkData.GetBlocks()[x, y - 1, z]);

						byte lightR = lightMap[lx + 1, ly, lz];
						byte lightL = lightMap[lx - 1, ly, lz];
						byte lightF = lightMap[lx, ly, lz + 1];
						byte lightB = lightMap[lx, ly, lz - 1];
						byte lightU = (y == 255 ? (byte)15 : lightMap[lx, ly + 1, lz]);
						byte lightD = (y == 0 ? (byte)15 : lightMap[lx, ly - 1, lz]);

						TextureMapper.TextureMap textureMap=textureMapper.map[c];

						if (bR>127)
						{
							AddFace(
								new Vector3(x + 1, y, z),
								new Vector3(x + 1, y + 1, z),
								new Vector3(x + 1, y + 1, z + 1),
								new Vector3(x + 1, y, z + 1),
								Vector3.right
							);
							AddTextureFace(textureMap.right);
							int b = (y == 0 ? 0 : 1);
							int t = (y == 255 ? 0 : 1);
							byte bl = (byte)((lightMap[lx+1, ly, lz] + lightMap[lx + 1, ly, lz - 1] + lightMap[lx + 1, ly - b, lz] + lightMap[lx + 1, ly - b, lz - 1]) / 4);
							byte tl = (byte)((lightMap[lx + 1, ly, lz] + lightMap[lx + 1, ly, lz - 1] + lightMap[lx + 1, ly + t, lz] + lightMap[lx + 1, ly + t, lz - 1]) / 4);
							byte tr = (byte)((lightMap[lx + 1, ly, lz] + lightMap[lx + 1, ly, lz + 1] + lightMap[lx + 1, ly + t, lz ] + lightMap[lx + 1, ly + t, lz + 1]) / 4);
							byte br = (byte)((lightMap[lx + 1, ly, lz] + lightMap[lx + 1, ly, lz + 1] + lightMap[lx + 1, ly - b, lz ] + lightMap[lx + 1, ly - b, lz + 1]) / 4);
							AddColors(textureMap,bl, tl, tr, br);
						}
						if (bL > 127)
						{
							AddFace(
								new Vector3(x, y, z + 1),
								new Vector3(x, y + 1, z + 1),
								new Vector3(x, y + 1, z),
								new Vector3(x, y, z),
								-Vector3.right
							);
							AddTextureFace(textureMap.left);
							int b = (y == 0 ? 0 : 1);
							int t = (y == 255 ? 0 : 1);
							byte br = (byte)((lightMap[lx - 1, ly, lz] + lightMap[lx - 1, ly, lz - 1] + lightMap[lx - 1, ly - b, lz] + lightMap[lx - 1, ly - b, lz - 1]) / 4);
							byte tr = (byte)((lightMap[lx - 1, ly, lz] + lightMap[lx - 1, ly, lz - 1] + lightMap[lx - 1, ly + t, lz] + lightMap[lx - 1, ly + t, lz - 1]) / 4);
							byte tl = (byte)((lightMap[lx - 1, ly, lz] + lightMap[lx - 1, ly, lz + 1] + lightMap[lx - 1, ly + t, lz] + lightMap[lx - 1, ly + t, lz + 1]) / 4);
							byte bl = (byte)((lightMap[lx - 1, ly, lz] + lightMap[lx - 1, ly, lz + 1] + lightMap[lx - 1, ly - b, lz] + lightMap[lx - 1, ly - b, lz + 1]) / 4);
							AddColors(textureMap, bl, tl, tr, br);

						}

						if (bU > 127)
						{
							AddFace(
								new Vector3(x, y + 1, z),
								new Vector3(x, y + 1, z + 1),
								new Vector3(x + 1, y + 1, z + 1),
								new Vector3(x + 1, y + 1, z),
								Vector3.up
							);
							AddTextureFace(textureMap.top);
							int b = (y == 0 ? 0 : 1);
							int t = (y == 255 ? 0 : 1);
							byte bl = (byte)((lightMap[lx, ly+ t, lz ] + lightMap[lx - 1, ly + t, lz] + lightMap[lx, ly + t, lz - 1] + lightMap[lx - 1, ly + t, lz - 1]) / 4);
							byte tl = (byte)((lightMap[lx, ly + t, lz ] + lightMap[lx - 1, ly + t, lz ] + lightMap[lx, ly + t, lz + 1] + lightMap[lx - 1, ly + t, lz + 1]) / 4);
							byte tr = (byte)((lightMap[lx, ly + t, lz ] + lightMap[lx + 1, ly + t, lz ] + lightMap[lx, ly + t, lz + 1] + lightMap[lx + 1, ly + t, lz + 1]) / 4);
							byte br = (byte)((lightMap[lx, ly + t, lz ] + lightMap[lx + 1, ly + t, lz ] + lightMap[lx, ly + t, lz - 1] + lightMap[lx + 1, ly + t, lz - 1]) / 4);
							AddColors(textureMap, bl, tl, tr, br);

						}
						if (bD > 127)
						{
							AddFace(
								new Vector3(x, y, z + 1),
								new Vector3(x, y, z),
								new Vector3(x+1, y, z),
								new Vector3(x+1, y, z+1),
								
								-Vector3.up
							);
							AddTextureFace(textureMap.bottom);
							int b = (y == 0 ? 0 : 1);
							int t = (y == 255 ? 0 : 1);
							byte tl = (byte)((lightMap[lx, ly -b, lz] + lightMap[lx - 1, ly - b, lz] + lightMap[lx, ly - b, lz - 1] + lightMap[lx - 1, ly - b, lz - 1]) / 4);
							byte bl = (byte)((lightMap[lx, ly - b, lz] + lightMap[lx - 1, ly - b, lz] + lightMap[lx, ly - b, lz + 1] + lightMap[lx - 1, ly - b, lz + 1]) / 4);
							byte br = (byte)((lightMap[lx, ly - b, lz] + lightMap[lx + 1, ly - b, lz] + lightMap[lx, ly - b, lz + 1] + lightMap[lx + 1, ly - b, lz + 1]) / 4);
							byte tr = (byte)((lightMap[lx, ly - b, lz] + lightMap[lx + 1, ly - b, lz] + lightMap[lx, ly - b, lz - 1] + lightMap[lx + 1, ly - b, lz - 1]) / 4);
							AddColors(textureMap, bl, tl, tr, br);

						}

						if (bF > 127)
						{
							AddFace(
								new Vector3(x + 1, y, z + 1),
								new Vector3(x + 1, y + 1, z + 1),
								new Vector3(x, y + 1, z + 1),
								new Vector3(x, y, z + 1),
								Vector3.forward
							);
							AddTextureFace(textureMap.front);
							int b = (y == 0 ? 0 : 1);
							int t = (y == 255 ? 0 : 1);
							byte br = (byte)((lightMap[lx, ly, lz + 1] + lightMap[lx - 1, ly, lz + 1] + lightMap[lx, ly - b, lz + 1] + lightMap[lx - 1, ly - b, lz + 1]) / 4);
							byte tr = (byte)((lightMap[lx, ly, lz + 1] + lightMap[lx - 1, ly, lz + 1] + lightMap[lx, ly + t, lz + 1] + lightMap[lx - 1, ly + t, lz + 1]) / 4);
							byte tl = (byte)((lightMap[lx, ly, lz + 1] + lightMap[lx + 1, ly, lz + 1] + lightMap[lx, ly + t, lz + 1] + lightMap[lx + 1, ly + t, lz + 1]) / 4);
							byte bl = (byte)((lightMap[lx, ly, lz + 1] + lightMap[lx + 1, ly, lz + 1] + lightMap[lx, ly - b, lz + 1] + lightMap[lx + 1, ly - b, lz + 1]) / 4);
							AddColors(textureMap,bl, tl, tr, br);

						}
						if (bB > 127)
						{
							AddFace(
								new Vector3(x, y, z),
								new Vector3(x, y + 1, z),
								new Vector3(x + 1, y + 1, z),
								new Vector3(x + 1, y, z),
								-Vector3.forward
							);
							AddTextureFace(textureMap.back);
							int b = (y == 0 ? 0 : 1);
							int t = (y == 255 ? 0 : 1);
							byte bl = (byte)((lightMap[lx, ly, lz-1] + lightMap[lx-1, ly, lz-1] + lightMap[lx, ly-b, lz-1] + lightMap[lx-1, ly-b, lz-1])/4);
							byte tl = (byte)((lightMap[lx, ly, lz - 1] + lightMap[lx-1, ly, lz - 1] + lightMap[lx, ly+t, lz - 1] + lightMap[lx-1, ly+t, lz - 1])/4);
							byte tr = (byte)((lightMap[lx, ly, lz - 1] + lightMap[lx+1, ly, lz - 1] + lightMap[lx, ly+t, lz - 1] + lightMap[lx+1, ly+t, lz - 1])/4);
							byte br = (byte)((lightMap[lx, ly, lz - 1] + lightMap[lx+1, ly, lz - 1] + lightMap[lx, ly-b, lz - 1] + lightMap[lx+1, ly-b, lz - 1])/4);
							AddColors(textureMap,bl, tl, tr, br);

						}
					}
				}
			}
		}
		UnityEngine.Profiling.Profiler.EndSample();

		UnityEngine.Profiling.Profiler.BeginSample("APPLYING MESH DATA");
		mesh.SetVertices(vertices);
		mesh.SetTriangles(triangles, 0);
		mesh.SetUVs(0, uvs);
		mesh.SetNormals(normals);
		mesh.SetColors(colors);
		gameObject.SetActive(true);
		vertices.Clear();
		triangles.Clear();
		colors.Clear();
		uvs.Clear();
		normals.Clear();
		meshCollider.sharedMesh = mesh;
		UnityEngine.Profiling.Profiler.EndSample();

		//#if UNITY_EDITOR
		UnityEngine.Profiling.Profiler.EndSample();
//#endif
	}

	private byte GetHighestNonAir(ChunkData[,] chunkMap, int x, int z)
	{
		int cX = x < 16 ? 0 : (x < 32 ? 1 : 2);
		int cZ = z < 16 ? 0 : (z < 32 ? 1 : 2);
		return chunkMap[cX, cZ].highestNonAirBlock[x - (cX * 16), z - (cZ * 16)];

	}
	private byte GetBlockFromMap(ChunkData[,] chunkMap, int x, int y, int z)
	{
		try
		{
			int cX = x < 16 ? 0 : (x < 32 ? 1 : 2);
			int cZ = z < 16 ? 0 : (z < 32 ? 1 : 2);
			return chunkMap[cX, cZ].GetBlocks()[x - (cX * 16), y, z - (cZ * 16)];
		}
		catch (System.Exception e)
		{
			Debug.LogWarning($"{x} {y} {z}");
			throw e;
		}
	}

	private void AddFace(Vector3 a, Vector3 b, Vector3 c, Vector3 d, Vector3 normal)
	{
		int index = vertices.Count;
		vertices.Add(a);
		vertices.Add(b);
		vertices.Add(c);
		vertices.Add(d);
		normals.Add(normal);
		normals.Add(normal);
		normals.Add(normal);
		normals.Add(normal);
		triangles.Add(index + 0);
		triangles.Add(index + 1);
		triangles.Add(index + 2);
		triangles.Add(index + 2);
		triangles.Add(index + 3);
		triangles.Add(index + 0);
	}

	private void AddTextureFace(TextureMapper.TextureMap.Face face)
	{
		uvs.Add(face.bl);
		uvs.Add(face.tl);
		uvs.Add(face.tr);
		uvs.Add(face.br);
	}

	private void AddColors(TextureMapper.TextureMap textureMap, byte lBL,byte lTL, byte lTR, byte lBR)
	{
		Color32 c = textureMap.defaultColor;
		//c.a = lightLevel;
		colors.Add(new Color32(c.r,c.g,c.b,lBL));
		colors.Add(new Color32(c.r, c.g, c.b, lTL));
		colors.Add(new Color32(c.r, c.g, c.b, lTR));
		colors.Add(new Color32(c.r, c.g, c.b, lBR));
	}

	public void Unload()
	{
		mesh.Clear();
		gameObject.SetActive(false);
	}
}