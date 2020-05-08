using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextureMapper
{
	public Dictionary<byte, TextureMap> map;
	public TextureMapper()
	{
		map = new Dictionary<byte, TextureMap>();

		map.Add(BlockTypes.GRASS, new TextureMap(
			new TextureMap.Face(new Vector2(192, 64)),
			new TextureMap.Face(new Vector2(192, 64)),
			new TextureMap.Face(new Vector2(192, 64)),
			new TextureMap.Face(new Vector2(192, 64)),
			new TextureMap.Face(new Vector2(384, 288)),
			new TextureMap.Face(new Vector2(176, 160)),
			new Color32(255,255,255,255)
			)
		);

		map.Add(BlockTypes.DIRT, new TextureMap(
			new TextureMap.Face(new Vector2(176, 160)),
			new TextureMap.Face(new Vector2(176, 160)),
			new TextureMap.Face(new Vector2(176, 160)),
			new TextureMap.Face(new Vector2(176, 160)),
			new TextureMap.Face(new Vector2(176, 160)),
			new TextureMap.Face(new Vector2(176, 160)),
			new Color32(255, 255, 255, 255)
			)
		);

		map.Add(BlockTypes.STONE, new TextureMap(
			new TextureMap.Face(new Vector2(496, 144)),
			new TextureMap.Face(new Vector2(496, 144)),
			new TextureMap.Face(new Vector2(496, 144)),
			new TextureMap.Face(new Vector2(496, 144)),
			new TextureMap.Face(new Vector2(496, 144)),
			new TextureMap.Face(new Vector2(496, 144)),
			new Color32(255, 255, 255, 255)
			)
		);

		map.Add(BlockTypes.BEDROCK, new TextureMap(
			new TextureMap.Face(new Vector2(352, 416)),
			new TextureMap.Face(new Vector2(352, 416)),
			new TextureMap.Face(new Vector2(352, 416)),
			new TextureMap.Face(new Vector2(352, 416)),
			new TextureMap.Face(new Vector2(352, 416)),
			new TextureMap.Face(new Vector2(352, 416)),
			new Color32(255, 255, 255, 255)
			)
		);

		map.Add(BlockTypes.COAL, new TextureMap(
			new TextureMap.Face(new Vector2(432, 368)),
			new TextureMap.Face(new Vector2(432, 368)),
			new TextureMap.Face(new Vector2(432, 368)),
			new TextureMap.Face(new Vector2(432, 368)),
			new TextureMap.Face(new Vector2(432, 368)),
			new TextureMap.Face(new Vector2(432, 368)),
			new Color32(255, 255, 255, 255)
			)
		);

		map.Add(BlockTypes.IRON, new TextureMap(
			new TextureMap.Face(new Vector2(208, 32)),
			new TextureMap.Face(new Vector2(208, 32)),
			new TextureMap.Face(new Vector2(208, 32)),
			new TextureMap.Face(new Vector2(208, 32)),
			new TextureMap.Face(new Vector2(208, 32)),
			new TextureMap.Face(new Vector2(208, 32)),
			new Color32(255, 255, 255, 255)
			)
		);

		map.Add(BlockTypes.GOLD, new TextureMap(
			new TextureMap.Face(new Vector2(192, 112)),
			new TextureMap.Face(new Vector2(192, 112)),
			new TextureMap.Face(new Vector2(192, 112)),
			new TextureMap.Face(new Vector2(192, 112)),
			new TextureMap.Face(new Vector2(192, 112)),
			new TextureMap.Face(new Vector2(192, 112)),
			new Color32(255, 255, 255, 255)

			)
		);

		map.Add(BlockTypes.DIAMOND, new TextureMap(
			new TextureMap.Face(new Vector2(176, 192)),
			new TextureMap.Face(new Vector2(176, 192)),
			new TextureMap.Face(new Vector2(176, 192)),
			new TextureMap.Face(new Vector2(176, 192)),
			new TextureMap.Face(new Vector2(176, 192)),
			new TextureMap.Face(new Vector2(176, 192)),
			new Color32(255, 255, 255, 255)
			)
		);

		map.Add(BlockTypes.LOG_OAK, new TextureMap(
			new TextureMap.Face(new Vector2(256, 112)),
			new TextureMap.Face(new Vector2(256, 112)),
			new TextureMap.Face(new Vector2(256, 112)),
			new TextureMap.Face(new Vector2(256, 112)),
			new TextureMap.Face(new Vector2(256, 96)),
			new TextureMap.Face(new Vector2(256, 96)),
			new Color32(255, 255, 255, 255)
			)
		);

		map.Add(BlockTypes.PLANKS_OAK, new TextureMap(
			new TextureMap.Face(new Vector2(256, 80)),
			new TextureMap.Face(new Vector2(256, 80)),
			new TextureMap.Face(new Vector2(256, 80)),
			new TextureMap.Face(new Vector2(256, 80)),
			new TextureMap.Face(new Vector2(256, 80)),
			new TextureMap.Face(new Vector2(256, 80)),
			new Color32(255, 255, 255, 255)
			)
		);
		map.Add(BlockTypes.LEAVES_OAK, new TextureMap(
			new TextureMap.Face(new Vector2(256, 128)),
			new TextureMap.Face(new Vector2(256, 128)),
			new TextureMap.Face(new Vector2(256, 128)),
			new TextureMap.Face(new Vector2(256, 128)),
			new TextureMap.Face(new Vector2(256, 128)),
			new TextureMap.Face(new Vector2(256, 128)),
			new Color32(168, 255, 68, 255)
			)
		);

		map.Add(BlockTypes.GLOWSTONE, new TextureMap(
			new TextureMap.Face(new Vector2(192, 144)),
			new TextureMap.Face(new Vector2(192, 144)),
			new TextureMap.Face(new Vector2(192, 144)),
			new TextureMap.Face(new Vector2(192, 144)),
			new TextureMap.Face(new Vector2(192, 144)),
			new TextureMap.Face(new Vector2(192, 144)),
			new Color32(255, 255, 255, 255)
			)
		);

		map.Add(BlockTypes.ANDESITE, new TextureMap(
			new TextureMap.Face(new Vector2(320, 128)),
			new TextureMap.Face(new Vector2(320, 128)),
			new TextureMap.Face(new Vector2(320, 128)),
			new TextureMap.Face(new Vector2(320, 128)),
			new TextureMap.Face(new Vector2(320, 128)),
			new TextureMap.Face(new Vector2(320, 128)),
			new Color32(255, 255, 255, 255)
			)
		);

		map.Add(BlockTypes.DIORITE, new TextureMap(
			new TextureMap.Face(new Vector2(176, 176)),
			new TextureMap.Face(new Vector2(176, 176)),
			new TextureMap.Face(new Vector2(176, 176)),
			new TextureMap.Face(new Vector2(176, 176)),
			new TextureMap.Face(new Vector2(176, 176)),
			new TextureMap.Face(new Vector2(176, 176)),
			new Color32(255, 255, 255, 255)
			)
		);

		map.Add(BlockTypes.GRANITE, new TextureMap(
			new TextureMap.Face(new Vector2(192, 96)),
			new TextureMap.Face(new Vector2(192, 96)),
			new TextureMap.Face(new Vector2(192, 96)),
			new TextureMap.Face(new Vector2(192, 96)),
			new TextureMap.Face(new Vector2(192, 96)),
			new TextureMap.Face(new Vector2(192, 96)),
			new Color32(255, 255, 255, 255)
			)
		);

		map.Add(BlockTypes.COBBLESTONE, new TextureMap(
			new TextureMap.Face(new Vector2(464, 368)),
			new TextureMap.Face(new Vector2(464, 368)),
			new TextureMap.Face(new Vector2(464, 368)),
			new TextureMap.Face(new Vector2(464, 368)),
			new TextureMap.Face(new Vector2(464, 368)),
			new TextureMap.Face(new Vector2(464, 368)),
			new Color32(255, 255, 255, 255)
			)
		);
	}

	public class TextureMap
	{
		public TextureMap(Face front, Face back, Face left, Face right, Face top, Face bottom, Color defaultColor)
		{
			this.front = front;
			this.back = back;
			this.left = left;
			this.right = right;
			this.top = top;
			this.bottom = bottom;
			this.defaultColor = defaultColor;
		}
		public Face front, back, left, right, top, bottom;
		public Color32 defaultColor;
		public class Face
		{
			public Face(Vector2 tl)
			{
				this.tl = tl;
				tr = tl + new Vector2(16, 0);
				bl = tl + new Vector2(0, 16);
				br = tl + new Vector2(16, 16);
			}
			public Vector2 tl, tr, bl, br;
		}
	}
}
