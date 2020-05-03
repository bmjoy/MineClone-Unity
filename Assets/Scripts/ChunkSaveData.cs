using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class ChunkSaveData
{
	public Vector2Int position;
	public List<C> changes = new List<C>();

	public ChunkSaveData(Vector2Int position)
	{
		this.position = position;
		changes = new List<C>();
	}

	[System.Serializable]
	public struct C //Change
	{
		public C(byte x, byte y, byte z, byte b)
		{
			this.x = x;
			this.y = y;
			this.z = z;
			this.b = b;
		}
		public byte x, y, z, b;
	
	}
}