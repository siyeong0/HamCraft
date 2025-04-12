using UnityEngine;
using UnityEngine.Tilemaps;

namespace Terrain
{
	public class Chunk
	{
		Vector2Int mChunkIdx;
		Vector2 mChunkBasePosition;
		int[,] mFrontBuffer;
		int[,] mBackBuffer;

		public Chunk(Vector2Int chunkIdx, TerrainGenerator terrainGenerator)
		{
			// chunk info
			mChunkIdx = chunkIdx;
			mChunkBasePosition = ChunkManager.Instance.CvtChunk2WorldBaseCoord(chunkIdx);
			// allocate buffers
			mFrontBuffer = new int[ChunkManager.Instance.chunkSize.x, ChunkManager.Instance.chunkSize.y];
			mBackBuffer = new int[ChunkManager.Instance.chunkSize.x, ChunkManager.Instance.chunkSize.y];
			// generate terrain
			Vector2 chunkBasePosition = ChunkManager.Instance.CvtChunk2WorldBaseCoord(chunkIdx);
			terrainGenerator.Generate(chunkBasePosition, out mFrontBuffer, out mBackBuffer);
		}

		public void Draw(Tilemap frontMap, Tilemap backMap)
		{
			for (int x = 0; x < ChunkManager.Instance.chunkSize.x; ++x)
			{
				for (int y = 0; y < ChunkManager.Instance.chunkSize.y; ++y)
				{
					Vector3Int tilemapIdx = new Vector3Int(Mathf.FloorToInt(mChunkBasePosition.x + x), Mathf.FloorToInt(mChunkBasePosition.y + y), 0);
					if (mFrontBuffer[x, y] != 0)
					{
						frontMap.SetTile(tilemapIdx, ChunkManager.Instance.tiles[mFrontBuffer[x, y]]);
					}
					if (mBackBuffer[x, y] != 0)
					{
						backMap.SetTile(tilemapIdx, ChunkManager.Instance.tiles[mBackBuffer[x, y]]);
					}
				}
			}
		}

		public void Erase(Tilemap frontMap, Tilemap backMap)
		{
			for (int x = 0; x < ChunkManager.Instance.chunkSize.x; ++x)
			{
				for (int y = 0; y < ChunkManager.Instance.chunkSize.y; ++y)
				{
					Vector3Int tilemapIdx = new Vector3Int(Mathf.FloorToInt(mChunkBasePosition.x + x), Mathf.FloorToInt(mChunkBasePosition.y + y), 0);
					frontMap.SetTile(tilemapIdx, null);
					backMap.SetTile(tilemapIdx, null);
				}
			}
		}
	}
}
