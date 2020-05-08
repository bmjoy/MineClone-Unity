using UnityEngine;
using System.Collections.Generic;
public class Structure
{
	public enum Type
	{
		OAK_TREE
	}

	public struct Change
	{
		public Change(int x, int y, int z, byte b)
		{
			this.x = x;
			this.y = y;
			this.z = z;
			this.b = b;
		}
		public int x, y, z;
		public byte b;
	}

	public static List<Change> Generate(Type type, int seed)
	{
		System.Random rnd = new System.Random(seed);
		List<Change> result = new List<Change>();
		switch (type)
		{
			case Type.OAK_TREE:
				int height = (byte)rnd.Next(4, 7);
				for (int i = 0; i < height; ++i)
				{
					result.Add(new Change(0, i, 0, BlockTypes.LOG_OAK));
				}
				result.Add(new Change(0, height, 0, BlockTypes.LEAVES_OAK));

				for (int i = 0; i < 4; ++i)
				{
					result.Add(new Change(1, height-i, 0, BlockTypes.LEAVES_OAK));
					result.Add(new Change(0, height-i, 1, BlockTypes.LEAVES_OAK));
					result.Add(new Change(-1, height-i, 0, BlockTypes.LEAVES_OAK));
					result.Add(new Change(0, height-i, -1, BlockTypes.LEAVES_OAK));
				}


				if (rnd.Next(0, 2)==0) result.Add(new Change(1, height-1, 1, BlockTypes.LEAVES_OAK));
				if (rnd.Next(0, 2) == 0) result.Add(new Change(-1, height - 1, 1, BlockTypes.LEAVES_OAK));
				if (rnd.Next(0, 2) == 0) result.Add(new Change(1, height - 1, -1, BlockTypes.LEAVES_OAK));
				if (rnd.Next(0, 2) == 0) result.Add(new Change(-1, height - 1, -1, BlockTypes.LEAVES_OAK));



				for (int i = 2; i < 4; ++i)
				{
					result.Add(new Change(2, height - i, -1, BlockTypes.LEAVES_OAK));
					result.Add(new Change(2, height - i, 0, BlockTypes.LEAVES_OAK));
					result.Add(new Change(2, height - i, 1, BlockTypes.LEAVES_OAK));

					result.Add(new Change(-2, height - i, -1, BlockTypes.LEAVES_OAK));
					result.Add(new Change(-2, height - i, 0, BlockTypes.LEAVES_OAK));
					result.Add(new Change(-2, height - i, 1, BlockTypes.LEAVES_OAK));

					result.Add(new Change(-1, height - i, 2, BlockTypes.LEAVES_OAK));
					result.Add(new Change(0, height - i, 2, BlockTypes.LEAVES_OAK));
					result.Add(new Change(1, height - i, 2, BlockTypes.LEAVES_OAK));

					result.Add(new Change(-1, height - i, -2, BlockTypes.LEAVES_OAK));
					result.Add(new Change(0, height - i, -2, BlockTypes.LEAVES_OAK));
					result.Add(new Change(1, height - i, -2, BlockTypes.LEAVES_OAK));

					result.Add(new Change(1, height - i, 1, BlockTypes.LEAVES_OAK));
					result.Add(new Change(-1, height - i, 1, BlockTypes.LEAVES_OAK));
					result.Add(new Change(1, height - i, -1, BlockTypes.LEAVES_OAK));
					result.Add(new Change(-1, height - i, -1, BlockTypes.LEAVES_OAK));

					if (rnd.Next(0, 2) == 0) result.Add(new Change(2, height - i, 2, BlockTypes.LEAVES_OAK));
					if (rnd.Next(0, 2) == 0) result.Add(new Change(-2, height - i, 2, BlockTypes.LEAVES_OAK));
					if (rnd.Next(0, 2) == 0) result.Add(new Change(2, height - i, -2, BlockTypes.LEAVES_OAK));
					if (rnd.Next(0, 2) == 0) result.Add(new Change(-2, height - i, -2, BlockTypes.LEAVES_OAK));

				}
				return result;
		}
		return null;
	}
}
