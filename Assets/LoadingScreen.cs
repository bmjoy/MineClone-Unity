using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class LoadingScreen : MonoBehaviour
{
	public RawImage loadingGraphic;
	public Texture2D loadingTexture;
	private Color[] colors;
	private ChunkManager chunkManager;
	private int renderDistance;
	public TextMeshProUGUI percentageText;
    public void Initialize()
    {
		renderDistance = GameManager.instance.gameSettings.RenderDistance;
		loadingTexture = new Texture2D(renderDistance, renderDistance, TextureFormat.ARGB32, false);
		loadingTexture.filterMode = FilterMode.Point;
		loadingGraphic.texture = loadingTexture;
		colors = new Color[renderDistance * renderDistance];
		for (int i = 0; i < colors.Length; ++i)
		{
			colors[i] = new Color(0, 0, 0, 1);
		}
		loadingTexture.SetPixels(colors);
		loadingTexture.Apply();
		chunkManager = World.activeWorld.chunkManager;
	}

    void Update()
    {
		Vector3 cameraPosition = World.activeWorld.mainCamera.transform.position;
		Vector2Int cameraChunkPos = new Vector2Int((int)cameraPosition.x / 16, (int)cameraPosition.z / 16);
		Vector2Int[] positions = chunkManager.GetActiveChunkPositions();
		percentageText.text = Mathf.RoundToInt( Mathf.Clamp01(1f * positions.Length / (renderDistance * renderDistance))*100f).ToString() + "%";
		//int minX = 32;
		//int maxX = 0;
		for (int i = 0; i < positions.Length; ++i)
		{
			int x = positions[i].x+(renderDistance/2)-1;
			int y = positions[i].y + (renderDistance / 2)-1;
			//minX = Mathf.Min(minX, x);
			//maxX = Mathf.Max(maxX, x);
			int index = renderDistance * y + x;
			if (index < colors.Length && index>-1)
			{
				colors[index] = new Color(1, 1, 1, 1);
			}
		}
		loadingTexture.SetPixels(colors);
		loadingTexture.Apply();
		//Debug.Log(minX + " - " + maxX);
	}
}
