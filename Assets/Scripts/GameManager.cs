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

	private void Start()
	{
		instance = this;
		Initialize();
		InitializeWorld(testWorld);
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
}
