using UnityEngine;
using System.Collections.Generic;
public class Structure
{
	public enum Type
	{
		OAK_TREE,
		WELL
	}

	private static Dictionary<Type, List<Change>> templates;

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

	public static void Initialize()
	{
		templates = new Dictionary<Type, List<Change>>();
		foreach (Type type in System.Enum.GetValues(typeof(Type)))
		{
			GenerateTemplate(type);
		}
	}

	public static bool OverwritesEverything(Type type)
	{
		switch (type)
		{
			case Type.OAK_TREE: return false;
			case Type.WELL: return true;
		}
		return false;
	}



	public static List<Change> Generate(Type type, int seed)
	{
		System.Random rnd = new System.Random(seed);
		
		List<Change> template = templates[type];
		List<Change> result=template;
		switch (type)
		{
			case Type.OAK_TREE:
				result = new List<Change>(); // new list because there are variants
				result.Add(new Change(0, -1, 0, BlockTypes.DIRT));
				bool cutOff = rnd.Next(100) == 0;
				if (cutOff)
				{
					result.Add(new Change(0, 0, 0, BlockTypes.LOG_OAK));
					return result;
				}
				int height = (byte)rnd.Next(4, 7);
				bool superHigh = rnd.Next(100) == 0;
				if (superHigh) height = 10;

				for (int i = 0; i < height; ++i)
				{
					result.Add(new Change(0, i, 0, BlockTypes.LOG_OAK));
				}
				result.Add(new Change(0, height, 0, BlockTypes.LEAVES_OAK));

				for (int i = 0; i < 4; ++i)
				{
					result.Add(new Change(1, height - i, 0, BlockTypes.LEAVES_OAK));
					result.Add(new Change(0, height - i, 1, BlockTypes.LEAVES_OAK));
					result.Add(new Change(-1, height - i, 0, BlockTypes.LEAVES_OAK));
					result.Add(new Change(0, height - i, -1, BlockTypes.LEAVES_OAK));
				}


				if (rnd.Next(0, 2) == 0) result.Add(new Change(1, height - 1, 1, BlockTypes.LEAVES_OAK));
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
				break;
			case Type.WELL:
				//no variants
				break;
		}
		return result;
	}

	private static void GenerateTemplate(Type type)
	{
		Debug.Log("Generating structure type: " + type);
		List<Change> result = new List<Change>();
		switch (type)
		{
			case Type.OAK_TREE:
				//no template
				break;
			case Type.WELL:
				for (int z = -2; z < 4; ++z)
				{
					for (int x = -2; x < 4; ++x)
					{
						result.Add(new Change(x, -1, z, BlockTypes.COBBLESTONE));
					}
				}
				for (int z = -1; z < 3; ++z)
				{
					for (int x = -1; x < 3; ++x)
					{
						result.Add(new Change(x, 0, z, BlockTypes.COBBLESTONE));
					}
				}
				for (int z = -1; z < 3; ++z)
				{
					for (int x = -1; x < 3; ++x)
					{
						result.Add(new Change(x, 3, z, BlockTypes.PLANKS_OAK));
					}
				}
				result.Add(new Change(-1, 1, -1, BlockTypes.PLANKS_OAK));
				result.Add(new Change(2, 1, -1, BlockTypes.PLANKS_OAK));
				result.Add(new Change(-1, 1, 2, BlockTypes.PLANKS_OAK));
				result.Add(new Change(2, 1, 2, BlockTypes.PLANKS_OAK));
				result.Add(new Change(-1, 2, -1, BlockTypes.PLANKS_OAK));
				result.Add(new Change(2, 2, -1, BlockTypes.PLANKS_OAK));
				result.Add(new Change(-1, 2, 2, BlockTypes.PLANKS_OAK));
				result.Add(new Change(2, 2, 2, BlockTypes.PLANKS_OAK));

				for (int i = 0; i < 16; ++i)
				{
					result.Add(new Change(0, -i, 0, BlockTypes.AIR));
					result.Add(new Change(1, -i, 0, BlockTypes.AIR));
					result.Add(new Change(0, -i, 1, BlockTypes.AIR));
					result.Add(new Change(1, -i, 1, BlockTypes.AIR));
				}
				break;
		}
		templates.Add(type, result);
		//Debug.Log("added to template " + type);
	}
}
