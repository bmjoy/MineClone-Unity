using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
public class AudioManager : MonoBehaviour
{
	public static AudioManager instance { get; private set; }
	public Music music=new Music();

	private AudioSource musicAudioSource;
	private AudioClip[] musicPlaylist;
	private bool forceMusicRestart;

	public void Initialize()
	{
		instance = this;
		LoadClips();
		musicAudioSource = gameObject.AddComponent<AudioSource>();
		musicAudioSource.playOnAwake = false;
		musicAudioSource.loop = false;
		StartCoroutine(MusicPlayer());
	}

	public void PlayNewPlaylist(AudioClip[] playlist)
	{
		musicPlaylist = playlist;
		forceMusicRestart = true;
	}

	private IEnumerator MusicPlayer()
	{
		float waitForNextTrack = 0;
		while (true)
		{
			yield return null;
			if (forceMusicRestart)
			{
				if (musicAudioSource.clip != null)
				{
					if (musicAudioSource.isPlaying)
					{
						musicAudioSource.volume = musicAudioSource.volume - Time.deltaTime;
						if (musicAudioSource.volume > 0) continue;
						musicAudioSource.Stop();
					}
					waitForNextTrack = 0;
					musicAudioSource.clip = null;
				}
				forceMusicRestart = false;
			}
			if (!musicAudioSource.isPlaying)
			{
				if (waitForNextTrack > 0)
				{
					waitForNextTrack -= Time.deltaTime;
					continue;
				}
				if (musicPlaylist == null || musicPlaylist.Length==0) continue;
				if (musicAudioSource.clip != null)
				{
					int currentIndex = System.Array.IndexOf(musicPlaylist, musicAudioSource.clip);
					currentIndex++;
					if (currentIndex >= musicPlaylist.Length) currentIndex = 0;
					musicAudioSource.clip = musicPlaylist[currentIndex];
					musicAudioSource.Play();
				}
				else
				{
					musicAudioSource.clip = musicPlaylist[0];
					musicAudioSource.Play();
				}
				waitForNextTrack = Random.Range(60, 180);
			}
			musicAudioSource.volume = 1;
		}
	}

	private void LoadClips()
	{
		List<AudioClip> game = Resources.LoadAll<AudioClip>("Audio/music/game").ToList<AudioClip>();
		List<AudioClip> creative = Resources.LoadAll<AudioClip>("Audio/music/game/creative").ToList<AudioClip>();
		List<AudioClip> menu = Resources.LoadAll<AudioClip>("Audio/music/menu").ToList<AudioClip>();

		foreach (AudioClip ac in creative)
		{
			game.Remove(ac);
		}
		music.game.clips = Shuffle(game).ToArray();
		music.game.creative.clips = Shuffle(creative).ToArray();
		music.menu.clips = Shuffle(menu).ToArray();
	}

	[System.Serializable]
	public class Music
	{
		public Game game = new Game();
		public Menu menu = new Menu();
		[System.Serializable]
		public class Game
		{
			public AudioClip[] clips;
			public Creative creative;
			[System.Serializable]
			public class Creative
			{
				public AudioClip[] clips;
			}
		}
		[System.Serializable]
		public class Menu
		{
			public AudioClip[] clips;
		}
	}

	//https://stackoverflow.com/questions/273313/randomize-a-listt
	private System.Random rnd = new System.Random();
	private IList<T> Shuffle<T>(IList<T> list)
	{
		int n = list.Count;
		while (n > 1)
		{
			n--;
			int k = rnd.Next(n + 1);
			T value = list[k];
			list[k] = list[n];
			list[n] = value;
		}
		return list;
	}

}
