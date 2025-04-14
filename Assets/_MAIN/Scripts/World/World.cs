using NUnit.Framework;
using Terrain;
using UnityEngine;

public class World
{
	World()
	{
		Assert.IsTrue(false);
	}

	public static int GetBlockAt(Vector2 position, ETerrainLayer layer)
	{
		Vector2Int chunkIdx = ChunkManager.Instance.CvtWorld2ChunkCoord(position);
		Terrain.Chunk chunk = ChunkManager.Instance.GetChunk(chunkIdx);
		Vector2Int blockIdx = chunk.GetBlockIdx(position);
		int block = chunk.GetBlockAt(blockIdx.x, blockIdx.y, layer);

		return block;
	}

	public static bool AddBlockAt(int block, Vector2 position, ETerrainLayer layer)
	{
		Vector2Int chunkIdx = ChunkManager.Instance.CvtWorld2ChunkCoord(position);
		Terrain.Chunk chunk = ChunkManager.Instance.GetChunk(chunkIdx);
		Vector2Int blockIdx = chunk.GetBlockIdx(position);
		bool bAdded = chunk.AddBlockAt(block, blockIdx.x, blockIdx.y, layer);

		return bAdded;
	}

	public static bool RemoveBlockAt(Vector2 position, ETerrainLayer layer)
	{
		Vector2Int chunkIdx = ChunkManager.Instance.CvtWorld2ChunkCoord(position);
		Terrain.Chunk chunk = ChunkManager.Instance.GetChunk(chunkIdx);
		Vector2Int blockIdx = chunk.GetBlockIdx(position);
		bool bRemoved = chunk.RemoveBlockAt(blockIdx.x, blockIdx.y, layer);

		return bRemoved;
	}

	public static void EmplaceBlockAt(int block, Vector2 position, ETerrainLayer layer)
	{
		Vector2Int chunkIdx = ChunkManager.Instance.CvtWorld2ChunkCoord(position);
		Terrain.Chunk chunk = ChunkManager.Instance.GetChunk(chunkIdx);
		Vector2Int blockIdx = chunk.GetBlockIdx(position);
		chunk.EmplaceBlockAt(block, blockIdx.x, blockIdx.y, layer);
	}
}
