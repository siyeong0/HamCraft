using NUnit.Framework;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using System;
using static Unity.Collections.AllocatorManager;

namespace Terrain
{
	public class Chunk
	{
		Vector2Int mChunkIdx;
		Vector2 mChunkBasePosition;
		List<int[,]> mMapBuffers;
		bool mbDirty = false;

		public Chunk(Vector2Int chunkIdx, TerrainGenerator terrainGenerator)
		{
			// chunk info
			mChunkIdx = chunkIdx;
			mChunkBasePosition = ChunkManager.Instance.CvtChunk2WorldBaseCoord(chunkIdx);
			// allocate buffers
			// generate terrain
			Vector2 chunkBasePosition = ChunkManager.Instance.CvtChunk2WorldBaseCoord(chunkIdx);
			mMapBuffers = terrainGenerator.Generate(chunkBasePosition);
		}

		public void Draw(Tilemap frontMap, Tilemap backMap, TileBase[] tiles)
		{
			Assert.IsTrue(tiles[0] == null);
			for (int x = 0; x < ChunkManager.Instance.chunkSize.x; ++x)
			{
				for (int y = 0; y < ChunkManager.Instance.chunkSize.y; ++y)
				{
					Vector3Int tilemapIdx = new Vector3Int(Mathf.FloorToInt(mChunkBasePosition.x + x), Mathf.FloorToInt(mChunkBasePosition.y + y), 0);
					int[,] frontBuffer = mMapBuffers[(int)ETerrainLayer.Front];
					frontMap.SetTile(tilemapIdx, tiles[frontBuffer[x, y]]);
					int[,] backBuffer = mMapBuffers[(int)ETerrainLayer.Back];
					backMap.SetTile(tilemapIdx, tiles[backBuffer[x, y]]);
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

		public bool IsDirty()
		{
			return mbDirty;
		}

		public int GetBlockAt(int x, int y, ETerrainLayer layer)
		{
			Assert.IsTrue(x >= 0 && x < ChunkManager.Instance.chunkSize.x && y >= 0 && y < ChunkManager.Instance.chunkSize.y);
			return mMapBuffers[(int)layer][x, y];
		}

		public bool AddBlockAt(int block, int x, int y, ETerrainLayer layer)
		{
			Assert.IsTrue(x >= 0 && x < ChunkManager.Instance.chunkSize.x && y >= 0 && y < ChunkManager.Instance.chunkSize.y);
			bool bAdded = mMapBuffers[(int)layer][x, y] == 0;
			if (bAdded)
			{
				mMapBuffers[(int)layer][x, y] = block;
				mbDirty = true;
			}
			return bAdded;
		}

		public bool RemoveBlockAt(int x, int y, ETerrainLayer layer)
		{
			Assert.IsTrue(x >= 0 && x < ChunkManager.Instance.chunkSize.x && y >= 0 && y < ChunkManager.Instance.chunkSize.y);
			bool bRemoved = mMapBuffers[(int)layer][x, y] != 0;
			if (bRemoved)
			{
				mMapBuffers[(int)layer][x, y] = 0;
				mbDirty = true;
			}
			return bRemoved;
		}

		public void EmplaceBlockAt(int block, int x, int y, ETerrainLayer layer)
		{
			Assert.IsTrue(x >= 0 && x < ChunkManager.Instance.chunkSize.x && y >= 0 && y < ChunkManager.Instance.chunkSize.y);
			mMapBuffers[(int)layer][x, y] = block;
			mbDirty = true;
		}

		public Vector2Int GetBlockIdx(Vector2 position)
		{
			Vector2 cellSize = ChunkManager.Instance.GetGridCellSize();
			Vector2Int blockIdx = new Vector2Int(
				Mathf.FloorToInt((position.x - mChunkBasePosition.x) / cellSize.x), 
				Mathf.FloorToInt((position.y - mChunkBasePosition.y) / cellSize.x));
			return blockIdx;
		}
	}
}
