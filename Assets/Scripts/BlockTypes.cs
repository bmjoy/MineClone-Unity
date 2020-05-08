using System.Collections.Generic;
public class BlockTypes
{
	//SOLID
	public const byte BEDROCK = 0;
	public const byte GRASS =1;
	public const byte DIRT =2;
	public const byte STONE = 3;
	public const byte COBBLESTONE = 4;
	public const byte COAL =5;
	public const byte IRON = 6;
	public const byte GOLD = 7;
	public const byte DIAMOND = 8;
	public const byte LOG_OAK = 9;
	public const byte PLANKS_OAK = 10;
	public const byte GLOWSTONE = 11;
	public const byte DIORITE = 12;
	public const byte GRANITE = 13;
	public const byte ANDESITE = 14;

	//TRANSPARENT
	public const byte LEAVES_OAK = 128;
	public const byte AIR = 255;

	public static Dictionary<byte, byte> lightLevel;
	public static Dictionary<byte, byte> density;

	public static void Initialize()
	{
		lightLevel = new Dictionary<byte, byte>();
		lightLevel.Add(BEDROCK, 0);
		lightLevel.Add(GRASS, 0);
		lightLevel.Add(DIRT, 0);
		lightLevel.Add(STONE, 0);
		lightLevel.Add(COAL, 0);
		lightLevel.Add(IRON, 0);
		lightLevel.Add(GOLD, 0);
		lightLevel.Add(DIAMOND, 0);
		lightLevel.Add(LOG_OAK, 0);
		lightLevel.Add(PLANKS_OAK, 0);
		lightLevel.Add(GLOWSTONE, 14);
		lightLevel.Add(LEAVES_OAK, 0);
		lightLevel.Add(AIR, 0);
		lightLevel.Add(ANDESITE, 0);
		lightLevel.Add(DIORITE, 0);
		lightLevel.Add(GRANITE, 0);
		lightLevel.Add(COBBLESTONE, 0);

		density = new Dictionary<byte, byte>();
		density.Add(BEDROCK, 255);
		density.Add(GRASS, 255);
		density.Add(DIRT, 255);
		density.Add(STONE, 255);
		density.Add(COAL, 255);
		density.Add(IRON, 255);
		density.Add(GOLD, 255);
		density.Add(DIAMOND, 255);
		density.Add(LOG_OAK, 255);
		density.Add(PLANKS_OAK, 255);
		density.Add(GLOWSTONE, 255);
		density.Add(LEAVES_OAK, 63);
		density.Add(AIR, 0);
		density.Add(ANDESITE, 255);
		density.Add(DIORITE, 255);
		density.Add(GRANITE, 255);
		density.Add(COBBLESTONE, 255);
	}

}
