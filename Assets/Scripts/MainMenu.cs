using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenu : MonoBehaviour
{
	public AudioManager audioManager;
	public int sceneToLoad = 1;
	private void Start()
	{
		if (AudioManager.instance == null)
		{
			audioManager.Initialize();
		}
		audioManager = AudioManager.instance;
	}
	private void Update()
	{
		if (!audioManager.IsPlayingMusic())
		{
			audioManager.PlayNewPlaylist(audioManager.music.menu.clips);
		}
	}
	public void StartGame()
	{
		UnityEngine.SceneManagement.SceneManager.LoadScene(sceneToLoad);
	}
}
