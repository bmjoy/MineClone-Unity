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
			new TextureMap.Face(new Vector2(176, 160))
			)
		);

		map.Add(BlockTypes.DIRT, new TextureMap(
			new TextureMap.Face(new Vector2(176, 160)),
			new TextureMap.Face(new Vector2(176, 160)),
			new TextureMap.Face(new Vector2(176, 160)),
			new TextureMap.Face(new Vector2(176, 160)),
			new TextureMap.Face(new Vector2(176, 160)),
			new TextureMap.Face(new Vector2(176, 160))
			)
		);

		map.Add(BlockTypes.STONE, new TextureMap(
			new TextureMap.Face(new Vector2(496, 144)),
			new TextureMap.Face(new Vector2(496, 144)),
			new TextureMap.Face(new Vector2(496, 144)),
			new TextureMap.Face(new Vector2(496, 144)),
			new TextureMap.Face(new Vector2(496, 144)),
			new TextureMap.Face(new Vector2(496, 144))
			)
		);

		map.Add(BlockTypes.BEDROCK, new TextureMap(
			new TextureMap.Face(new Vector2(352, 416)),
			new TextureMap.Face(new Vector2(352, 416)),
			new TextureMap.Face(new Vector2(352, 416)),
			new TextureMap.Face(new Vector2(352, 416)),
			new TextureMap.Face(new Vector2(352, 416)),
			new TextureMap.Face(new Vector2(352, 416))
			)
		);

		map.Add(BlockTypes.COAL, new TextureMap(
			new TextureMap.Face(new Vector2(432, 368)),
			new TextureMap.Face(new Vector2(432, 368)),
			new TextureMap.Face(new Vector2(432, 368)),
			new TextureMap.Face(new Vector2(432, 368)),
			new TextureMap.Face(new Vector2(432, 368)),
			new TextureMap.Face(new Vector2(432, 368))
			)
		);

		map.Add(BlockTypes.IRON, new TextureMap(
			new TextureMap.Face(new Vector2(208, 32)),
			new TextureMap.Face(new Vector2(208, 32)),
			new TextureMap.Face(new Vector2(208, 32)),
			new TextureMap.Face(new Vector2(208, 32)),
			new TextureMap.Face(new Vector2(208, 32)),
			new TextureMap.Face(new Vector2(208, 32))
			)
		);

		map.Add(BlockTypes.GOLD, new TextureMap(
			new TextureMap.Face(new Vector2(192, 112)),
			new TextureMap.Face(new Vector2(192, 112)),
			new TextureMap.Face(new Vector2(192, 112)),
			new TextureMap.Face(new Vector2(192, 112)),
			new TextureMap.Face(new Vector2(192, 112)),
			new TextureMap.Face(new Vector2(192, 112))
			)
		);

		map.Add(BlockTypes.DIAMOND, new TextureMap(
			new TextureMap.Face(new Vector2(176, 192)),
			new TextureMap.Face(new Vector2(176, 192)),
			new TextureMap.Face(new Vector2(176, 192)),
			new TextureMap.Face(new Vector2(176, 192)),
			new TextureMap.Face(new Vector2(176, 192)),
			new TextureMap.Face(new Vector2(176, 192))
			)
		);
	}

	public class TextureMap
	{
		public TextureMap(Face front, Face back, Face left, Face right, Face top, Face bottom)
		{
			this.front = front;
			this.back = back;
			this.left = left;
			this.right = right;
			this.top = top;
			this.bottom = bottom;
		}
		public Face front, back, left, right, top, bottom;
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
