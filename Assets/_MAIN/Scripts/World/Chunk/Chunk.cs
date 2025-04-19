using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Tilemaps;

using System;
using System.Collections;
using System.Collections.Generic;

namespace HamCraft
{
	public class Chunk
	{
		Vector2Int mChunkIdx;
		Vector2 mChunkBasePosition;
		TileBase[] mTileBuffer;
		TileBase[] mNullTileBuffer;

		List<int[,]> mMapBuffers;
		List<(int, Vector2Int, ETerrainLayer)> mUpdateList;

		bool mbCoroutineErase;

		public Chunk(Vector2Int chunkIdx)
		{
			// chunk info
			mChunkIdx = chunkIdx;
			mChunkBasePosition = ChunkManager.Instance.CvtChunk2WorldBaseCoord(chunkIdx);

			// init buffers
			int width = ChunkManager.Instance.chunkSize.x;
			int height = ChunkManager.Instance.chunkSize.y;

			int batchSize = ChunkManager.Instance.updateBlockBatchSize;
			mTileBuffer = new TileBase[width * batchSize];
			mNullTileBuffer = new TileBase[width * batchSize];

			mUpdateList = new List<(int, Vector2Int, ETerrainLayer)>();

			// generate terrain
			mMapBuffers = ChunkManager.Instance.terrainGenerator.Generate(mChunkBasePosition);
		}

		public void Draw()
		{
			int width = ChunkManager.Instance.chunkSize.x;
			int height = ChunkManager.Instance.chunkSize.y;
			List<Tilemap> tilemapList = ChunkManager.Instance.tilemapList;
			TileBase[] tiles = ChunkManager.Instance.tiles;

			TileBase[] tileBuffer = new TileBase[width * height];
			for (int layer = 0; layer < Enum.GetValues(typeof(ETerrainLayer)).Length; ++layer)
			{
				int[,] buffer = mMapBuffers[layer];
				Tilemap tilemap = tilemapList[layer];
				for (int x = 0; x < width; ++x)
				{
					for (int y = 0; y < height; ++y)
					{
						tileBuffer[y * width + x] = tiles[buffer[x, y]];
					}
				}
				Vector3Int baseIdx = tilemap.WorldToCell(mChunkBasePosition);
				BoundsInt bound = new BoundsInt(baseIdx.x, baseIdx.y, baseIdx.z, width, height, 1);
				tilemap.SetTilesBlock(bound, tileBuffer);
			}
		}

		public void Erase()
		{
			int width = ChunkManager.Instance.chunkSize.x;
			int height = ChunkManager.Instance.chunkSize.y;
			List<Tilemap> tilemapList = ChunkManager.Instance.tilemapList;

			TileBase[] tileBuffer = new TileBase[width * height];
			for (int layer = 0; layer < Enum.GetValues(typeof(ETerrainLayer)).Length; ++layer)
			{
				Tilemap tilemap = tilemapList[layer];
				Vector3Int baseIdx = tilemap.WorldToCell(mChunkBasePosition);
				BoundsInt bound = new BoundsInt(baseIdx.x, baseIdx.y, baseIdx.z, width, height, 1);
				tilemap.SetTilesBlock(bound, tileBuffer);
			}
		}

		public IEnumerator DrawCoroutine()
		{
			yield return null;
			if (mbCoroutineErase) yield break;

			int width = ChunkManager.Instance.chunkSize.x;
			int height = ChunkManager.Instance.chunkSize.y;
			int batchSize = ChunkManager.Instance.updateBlockBatchSize;
			List<Tilemap> tilemapList = ChunkManager.Instance.tilemapList;
			TileBase[] tiles = ChunkManager.Instance.tiles;

			for (int layer = 0; layer < Enum.GetValues(typeof(ETerrainLayer)).Length; ++layer)
			{
				int[,] buffer = mMapBuffers[layer];
				Tilemap tilemap = tilemapList[layer];
				for (int e = 0; e < height / batchSize; ++e)
				{
					for (int b = 0; b < batchSize; ++b)
					{
						int y = e * batchSize + b;
						for (int x = 0; x < width; ++x)
						{
							mTileBuffer[b * width + x] = tiles[buffer[x, y]];
						}
					}
					BoundsInt bound = new BoundsInt((int)mChunkBasePosition.x, (int)mChunkBasePosition.y + e * batchSize, 0, width, batchSize, 1);
					tilemap.SetTilesBlock(bound, mTileBuffer);
					
					yield return null;
					if (mbCoroutineErase) yield break;
				}
			}
		}

		public IEnumerator EraseCoroutine()
		{
			mbCoroutineErase = true;
			yield return null;

			int width = ChunkManager.Instance.chunkSize.x;
			int height = ChunkManager.Instance.chunkSize.y;
			int batchSize = ChunkManager.Instance.updateBlockBatchSize;
			List<Tilemap> tilemapList = ChunkManager.Instance.tilemapList;

			for (int layer = 0; layer < Enum.GetValues(typeof(ETerrainLayer)).Length; ++layer)
			{
				Tilemap tilemap = tilemapList[layer];
				for (int e = 0; e < height / batchSize; ++e)
				{
					BoundsInt bound = new BoundsInt((int)mChunkBasePosition.x, (int)mChunkBasePosition.y + e * batchSize, 0, width, batchSize, 1);
					tilemap.SetTilesBlock(bound, mNullTileBuffer);

					yield return null;
				}
			}
		}

		public void Update()
		{
			List<Tilemap> tilemapList = ChunkManager.Instance.tilemapList;
			TileBase[] tiles = ChunkManager.Instance.tiles;

			foreach ((int block, Vector2Int idx, ETerrainLayer layer) in mUpdateList)
			{ 
				Vector3Int tilemapIdx = new Vector3Int((int)(mChunkBasePosition.x) + idx.x, (int)(mChunkBasePosition.y) + idx.y, 0);
				tilemapList[(int)layer].SetTile(tilemapIdx, tiles[block]);
			}

			mUpdateList.Clear();
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
				mUpdateList.Add((block, new Vector2Int(x, y), layer));
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
				mUpdateList.Add((0, new Vector2Int(x, y), layer));
			}
			return bRemoved;
		}

		public void EmplaceBlockAt(int block, int x, int y, ETerrainLayer layer)
		{
			Assert.IsTrue(x >= 0 && x < ChunkManager.Instance.chunkSize.x && y >= 0 && y < ChunkManager.Instance.chunkSize.y);
			mMapBuffers[(int)layer][x, y] = block;
			mUpdateList.Add((block, new Vector2Int(x, y), layer));
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
