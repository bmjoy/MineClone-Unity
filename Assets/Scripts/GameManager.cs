using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
	public static GameManager instance { get; private set; }
	public bool showLoadingScreen = true;
	public World world;
	public GameSettings gameSettings;
	public UI ui;
	private SaveDataManager saveDataManager;
	public TextureMapper textureMapper;
	public AudioManager audioManager;
	public bool isInStartup;
	public WorldInfo testWorld;
	public Texture2D textures;

	private void Start()
	{
		instance = this;
		Initialize();
		BlockTypes.Initialize();
		textureMapper = new TextureMapper();
		audioManager.Initialize();
		
		CreateTextures();
		Structure.Initialize();
		InitializeWorld(testWorld);
		ui.Initialize();

		//_ColorHorizon, _ColorTop, _ColorBottom;
		Shader.SetGlobalColor("_SkyColorTop",new Color( 0.7692239f, 0.7906416f, 0.8113208f,1f));
		Shader.SetGlobalColor("_SkyColorHorizon", new Color(0.3632075f, 0.6424405f, 1f, 1f));
		Shader.SetGlobalColor("_SkyColorBottom", new Color(0.1632253f, 0.2146282f, 0.2641509f, 1f));
		Shader.SetGlobalFloat("_MinLightLevel", gameSettings.minimumLightLevel);
#if !UNITY_EDITOR
		showLoadingScreen = true;
#endif
		if (showLoadingScreen)
		{
			isInStartup = true;
			world.chunkManager.isInStartup = true;
			ui.loadingScreen.gameObject.SetActive(true);
		}
	}

	private void Update()
	{
		if (!audioManager.IsPlayingMusic())
		{
			if (isInStartup)
			{
				audioManager.PlayNewPlaylist(audioManager.music.menu.clips);
			}
			else
			{
				audioManager.PlayNewPlaylist(audioManager.music.game.clips);

			}
		}
		if (isInStartup)
		{
			if (world.chunkManager.StartupFinished())
			{
				world.chunkManager.isInStartup = false;
				isInStartup = false;
				ui.loadingScreen.gameObject.SetActive(false);
				audioManager.PlayNewPlaylist(audioManager.music.game.clips);
				System.GC.Collect();
			}
		}
		ui.UpdateUI();
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
