using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
	public static GameManager instance { get; private set; }
	public World world;
	public GameSettings gameSettings;
	private SaveDataManager saveDataManager;

	public WorldInfo testWorld;
	public Texture2D textures;

	private void Start()
	{
		instance = this;
		Initialize();
		InitializeWorld(testWorld);

		CreateTextures();
	}

	private void Initialize()
	{
		saveDataManager = new SaveDataManager();
	}

	public void InitializeWorld(WorldInfo worldInfo)
	{
		worldInfo = saveDataManager.Initialize(worldInfo);
		world.Initialize(worldInfo);
	}

	private void CreateTextures()
	{
		Texture2D temp = new Texture2D(textures.width, textures.height, TextureFormat.ARGB32, 5, false);
		temp.SetPixels(textures.GetPixels());
		temp.filterMode = FilterMode.Point;
		temp.Apply();
		textures = temp;
		Shader.SetGlobalTexture("_BlockTextures", textures);
	}
}
