using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
public class AudioManager : MonoBehaviour
{
	public static AudioManager instance { get; private set; }

	public Music music = new Music();
	public Dig dig = new Dig();

	private AudioSource musicAudioSource, digAudioSource;
	public AudioClip[] musicPlaylist;
	private bool forceMusicRestart;
	public bool ready;

	private void Awake()
	{
		if (instance != null)
		{
			gameObject.SetActive(false);
			return;
		}
	}

	public void Initialize()
	{
		if (ready) return;
		instance = this;
		StartCoroutine(Load());
		StartCoroutine(MusicPlayer());
		DontDestroyOnLoad(this);
	}

	public void PlayNewPlaylist(AudioClip[] playlist)
	{
		Debug.Log("Force Start Music");
		musicPlaylist = playlist;
		forceMusicRestart = true;
	}

	public bool IsPlayingMusic()
	{
		return (musicPlaylist != null && musicPlaylist.Length>0);
	}

	private IEnumerator MusicPlayer()
	{
		float waitForNextTrack = 0;
		while (true)
		{
			yield return null;
			if (!ready) continue;
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

	private IEnumerator Load()
	{
		yield return null;

		musicAudioSource = gameObject.AddComponent<AudioSource>();
		musicAudioSource.playOnAwake = false;
		musicAudioSource.loop = false;

		yield return null;

		List<AudioClip> game = Resources.LoadAll<AudioClip>("Audio/music/game").ToList();
		List<AudioClip> creative = Resources.LoadAll<AudioClip>("Audio/music/game/creative").ToList();
		List<AudioClip> menu = Resources.LoadAll<AudioClip>("Audio/music/menu").ToList();

		foreach (AudioClip ac in creative)
		{
			game.Remove(ac);
		}
		music.game.clips = Shuffle(game).ToArray();
		music.game.creative.clips = Shuffle(creative).ToArray();
		music.menu.clips = Shuffle(menu).ToArray();

		Debug.Log("Music started loading");

		yield return null;

		AudioClip[] digSounds = Resources.LoadAll<AudioClip>("Audio/dig");
		List<AudioClip> grassSounds = new List<AudioClip>();
		List<AudioClip> stoneSounds = new List<AudioClip>();
		List<AudioClip> woodSounds = new List<AudioClip>();
		List<AudioClip> gravelSounds = new List<AudioClip>();

		for (int i = 0; i < digSounds.Length; ++i)
		{
			if (digSounds[i].name.Contains("wet")) continue;
			if (digSounds[i].name.Contains("grass")) grassSounds.Add(digSounds[i]);
			if (digSounds[i].name.Contains("stone")) stoneSounds.Add(digSounds[i]);
			if (digSounds[i].name.Contains("gravel")) gravelSounds.Add(digSounds[i]);
			if (digSounds[i].name.Contains("wood")) woodSounds.Add(digSounds[i]);
		}

		dig.grass = grassSounds.ToArray();
		dig.gravel = gravelSounds.ToArray();
		dig.stone = stoneSounds.ToArray();
		dig.wood = woodSounds.ToArray();

		Debug.Log("Dig sounds started loading");


		ready = true;
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

	[System.Serializable]
	public class Dig
	{
		public void Play(Type type, Vector3 position)
		{
			Debug.Log("Playing sound of type " + type);
			AudioClip[] clips = GetClips(type);
			if (clips == null) return;
			AudioClip clip = clips[Random.Range(0,clips.Length)];
			//Debug.Log("Playing sound " + clip.name);
			AudioSource.PlayClipAtPoint(clip, position);
		}

		public enum Type
		{
			Silent,
			Stone,
			Wood,
			Gravel,
			Grass
		}
		public AudioClip[] GetClips(Type type)
		{
			switch (type)
			{
				case Type.Stone: return stone;
				case Type.Wood: return wood;
				case Type.Gravel: return gravel;
				case Type.Grass: return grass;
			}
			return null;
		}
		public AudioClip[] stone, wood, gravel, grass;
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
