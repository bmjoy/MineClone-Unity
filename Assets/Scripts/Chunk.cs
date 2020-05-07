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

		ChunkData chunkData = chunkDataManager.data[position];

		byte[,] highestNonAirBlock = chunkData.highestNonAirBlock;

		UnityEngine.Profiling.Profiler.BeginSample("GRABBING BLOCK DATA");
		//don't cache these byte references, only use them in this function
		byte[,,] blockData = chunkData.GetBlocks();
		byte[,,] blockDataFront = chunkDataManager.data[position + new Vector2Int(0, 1)].GetBlocks();
		byte[,,] blockDataBack = chunkDataManager.data[position + new Vector2Int(0, -1)].GetBlocks();
		byte[,,] blockDataLeft = chunkDataManager.data[position + new Vector2Int(-1, 0)].GetBlocks();
		byte[,,] blockDataRight = chunkDataManager.data[position + new Vector2Int(1, 0)].GetBlocks();

		byte[,,] lightData = chunkDataManager.data[position].NewLights();
		byte[,,] lightDataFront = chunkDataManager.data[position + new Vector2Int(0, 1)].GetLights();
		byte[,,] lightDataBack = chunkDataManager.data[position + new Vector2Int(0, -1)].GetLights();
		byte[,,] lightDataLeft = chunkDataManager.data[position + new Vector2Int(-1, 0)].GetLights();
		byte[,,] lightDataRight = chunkDataManager.data[position + new Vector2Int(1, 0)].GetLights();

		UnityEngine.Profiling.Profiler.EndSample();

		UnityEngine.Profiling.Profiler.BeginSample("SIMULATING LIGHT");


		Queue<Vector3Int> simulateQueue = new Queue<Vector3Int>();
		for (int z = 0; z < 16; ++z)
		{
			for (int x = 0; x < 16; ++x)
			{
				int y = highestNonAirBlock[x, z];
				if(x>0) y = Mathf.Max(highestNonAirBlock[x - 1, z], y);
				if(x<15) y = Mathf.Max(highestNonAirBlock[x + 1, z], y);
				if(z>0) y = Mathf.Max(highestNonAirBlock[x, z-1], y);
				if (z<15) y = Mathf.Max(highestNonAirBlock[x, z+1], y);

				y = Mathf.Min(y+1, 255);
				simulateQueue.Enqueue(new Vector3Int(x, y, z));
				while (y < 255)
				{
					lightData[x, y, z] = 15;
					y++;
				}
			}
		}

		foreach (KeyValuePair<Vector3Int, byte> kv in chunkData.lightSources)
		{
			Vector3Int position = kv.Key;
			lightData[position.x, position.y, position.z] = kv.Value;
			simulateQueue.Enqueue(position);
		}

		int simulateCount = 0;
		while (simulateQueue.Count > 0)
		{
			Vector3Int position = simulateQueue.Dequeue();
			int x = position.x;
			int y = position.y;
			int z = position.z;

			byte right = (x == 15 ? blockDataRight[0, y, z] : blockData[x + 1, y, z]);
			byte left = (x == 0 ? blockDataLeft[15, y, z] : blockData[x - 1, y, z]);
			byte front = (z == 15 ? blockDataFront[x, y, 0] : blockData[x, y, z + 1]);
			byte back = (z == 0 ? blockDataBack[x, y, 15] : blockData[x, y, z - 1]);
			byte up = (y == 255 ? (byte)0 : blockData[x, y + 1, z]);
			byte down = (y == 0 ? (byte)0 : blockData[x, y - 1, z]);

			byte light = lightData[x, y, z];

			if (right == BlockTypes.AIR)
			{
				byte lightR = (x == 15 ? (byte)15 : lightData[x + 1, y, z]);
				if (lightR < light - 1)
				{
					lightData[x + 1, y, z] = (byte)(light - 1);
					simulateQueue.Enqueue(new Vector3Int(x + 1, y, z));
				}
			}
			if (left == BlockTypes.AIR)
			{
				byte lightL = (x == 0 ? (byte)15 : lightData[x - 1, y, z]);
				if (lightL < light - 1)
				{
					lightData[x - 1, y, z] = (byte)(light - 1);
					simulateQueue.Enqueue(new Vector3Int(x - 1, y, z));
				}
			}
			if (down == BlockTypes.AIR)
			{
				if (light == 15)
				{
					lightData[x, y - 1, z] = light;
					simulateQueue.Enqueue(new Vector3Int(x, y - 1, z));
				}
				else
				{
					byte lightD = (y == 0 ? (byte)15 : lightData[x, y - 1, z]);
					if (lightD < light - 1)
					{
						lightData[x, y - 1, z] = (byte)(light - 1);
						simulateQueue.Enqueue(new Vector3Int(x, y - 1, z));
					}
				}
			}
			if (up == BlockTypes.AIR)
			{
				byte lightU = (y == 255 ? (byte)15 : lightData[x, y + 1, z]);
				if (lightU < light - 1)
				{
					lightData[x, y + 1, z] = (byte)(light - 1);
					simulateQueue.Enqueue(new Vector3Int(x, y + 1, z));
				}
			}
			if (front == BlockTypes.AIR)
			{
				byte lightF = (z == 15 ? (byte)15 : lightData[x, y, z + 1]);
				if (lightF < light - 1)
				{
					lightData[x, y, z + 1] = (byte)(light - 1);
					simulateQueue.Enqueue(new Vector3Int(x, y, z + 1));
				}
			}
			if (back == BlockTypes.AIR)
			{
				byte lightB = (z == 0 ? (byte)15 : lightData[x, y, z - 1]);
				if (lightB < light - 1)
				{
					lightData[x, y, z - 1] = (byte)(light - 1);
					simulateQueue.Enqueue(new Vector3Int(x, y, z - 1));
				}
			}
			simulateCount++;
		}
		Debug.Log("Did " + simulateCount + " light simulations");

		UnityEngine.Profiling.Profiler.EndSample();


		UnityEngine.Profiling.Profiler.BeginSample("CREATING FACES");
		for (int z = 0; z < 16; ++z)
		{
			for (int y = 0; y < 256; ++y)
			{
				for (int x = 0; x < 16; ++x)
				{
					byte c = blockData[x, y, z];
					if (c != BlockTypes.AIR)
					{
						byte right = (x == 15 ? blockDataRight[0, y, z] : blockData[x+1, y, z]);
						byte left =( x == 0 ? blockDataLeft[15, y, z] : blockData[x-1, y, z]);
						byte front =( z == 15 ? blockDataFront[x, y, 0] : blockData[x, y, z+1]);
						byte back = (z == 0 ? blockDataBack[x, y, 15] : blockData[x, y, z-1]);
						byte up = (y == 255 ? BlockTypes.AIR : blockData[x, y + 1, z]);
						byte down = (y == 0 ? BlockTypes.AIR : blockData[x, y - 1, z]);

						byte lightR = (x == 15 ? lightDataRight[0, y, z] : lightData[x + 1, y, z]);
						byte lightL = (x == 0 ? lightDataLeft[15, y, z] : lightData[x - 1, y, z]);
						byte lightF = (z == 15 ? lightDataFront[x, y, 0] : lightData[x, y, z + 1]);
						byte lightB = (z == 0 ? lightDataBack[x, y, 15] : lightData[x, y, z - 1]);
						byte lightU = (y == 255 ? (byte)15 : lightData[x, y + 1, z]);
						byte lightD = (y == 0 ? (byte)15 : lightData[x, y - 1, z]);

						TextureMapper.TextureMap textureMap;
						try
						{
							textureMap = chunkDataManager.textureMapper.map[c];
						}
						catch (System.Exception e)
						{
							Debug.LogWarning($"{c} at {x}-{y}-{z} in {gameObject.name}");
							throw e;
						}


						if (right>127)
						{
							AddFace(
								new Vector3(x + 1, y, z),
								new Vector3(x + 1, y + 1, z),
								new Vector3(x + 1, y + 1, z + 1),
								new Vector3(x + 1, y, z + 1),
								Vector3.right
							);
							AddTextureFace(textureMap.right);
							AddColors(textureMap,lightR);
						}
						if (left > 127)
						{
							AddFace(
								new Vector3(x, y, z + 1),
								new Vector3(x, y + 1, z + 1),
								new Vector3(x, y + 1, z),
								new Vector3(x, y, z),
								-Vector3.right
							);
							AddTextureFace(textureMap.left);
							AddColors(textureMap,lightL);

						}

						if (up > 127)
						{
							AddFace(
								new Vector3(x, y + 1, z),
								new Vector3(x, y + 1, z + 1),
								new Vector3(x + 1, y + 1, z + 1),
								new Vector3(x + 1, y + 1, z),
								Vector3.up
							);
							AddTextureFace(textureMap.top);
							AddColors(textureMap,lightU);

						}
						if (down > 127)
						{
							AddFace(
								new Vector3(x, y, z),
								new Vector3(x+1, y, z),
								new Vector3(x+1, y, z+1),
								new Vector3(x, y, z+1),
								-Vector3.up
							);
							AddTextureFace(textureMap.bottom);
							AddColors(textureMap,lightD);

						}

						if (front > 127)
						{
							AddFace(
								new Vector3(x + 1, y, z + 1),
								new Vector3(x + 1, y + 1, z + 1),
								new Vector3(x, y + 1, z + 1),
								new Vector3(x, y, z + 1),
								Vector3.forward
							);
							AddTextureFace(textureMap.front);
							AddColors(textureMap,lightF);

						}
						if (back > 127)
						{
							AddFace(
								new Vector3(x, y, z),
								new Vector3(x, y + 1, z),
								new Vector3(x + 1, y + 1, z),
								new Vector3(x + 1, y, z),
								-Vector3.forward
							);
							AddTextureFace(textureMap.back);
							AddColors(textureMap,lightB);

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

	private void AddColors(TextureMapper.TextureMap textureMap, byte lightLevel)
	{
		Color32 c = textureMap.defaultColor;
		c.a = lightLevel;
		colors.Add(c);
		colors.Add(c);
		colors.Add(c);
		colors.Add(c);
	}

	public void Unload()
	{
		mesh.Clear();
		gameObject.SetActive(false);
	}
}