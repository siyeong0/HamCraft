using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Tilemaps;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using HamCraft.Container;

namespace HamCraft
{
	public class ChunkManager : MonoBehaviour
	{
		public static ChunkManager Instance { get; private set; }

		[SerializeField] Transform pivot;
		[SerializeField] GameObject chunkPrefab;
		[SerializeField] public Vector2Int chunkSize;
		[SerializeField] Vector2Int simulateDepth;
		[SerializeField] public int updateChunkBatchSize;
		[SerializeField] public int updateBlockBatchSize;
		[SerializeField] public TileBase[] tiles;

		Grid mGrid;
		public TerrainGenerator terrainGenerator { get; private set; }
		public List<Tilemap> tilemapList { get; private set; }

		Dictionary<Vector2Int, Chunk> mChunks = new Dictionary<Vector2Int, Chunk>();
		Vector2Int mPrevPivotChunk;

		ModifiableQueue<(Vector2Int, Chunk)> mLoadChunkQueue = new ModifiableQueue<(Vector2Int, Chunk)>();
		ModifiableQueue<(Vector2Int, Chunk)> mFreeChunkQueue = new ModifiableQueue<(Vector2Int, Chunk)>();
		bool mbCompressBound;

		void Awake()
		{
			if (Instance == null)
			{
				Instance = this;
			}
			else
			{
				Destroy(gameObject);
				return;
			}

			mGrid = GetComponent<Grid>();
			terrainGenerator = GetComponent<TerrainGenerator>();

			// init tilemaps
			GameObject chunk = Instantiate(chunkPrefab);
			GameObject frontTilemapObject = chunk.transform.GetChild(1).gameObject;
			GameObject backTilemapObject = chunk.transform.GetChild(0).gameObject;
			frontTilemapObject.transform.SetParent(transform);
			backTilemapObject.transform.SetParent(transform);
			Destroy(chunk);

			int numLayers = Enum.GetValues(typeof(ETerrainLayer)).Length;
			tilemapList = new List<Tilemap>();
			for (int i = 0; i < numLayers; ++i) tilemapList.Add(null);
			tilemapList[(int)ETerrainLayer.Front] = frontTilemapObject.GetComponent<Tilemap>();
			tilemapList[(int)ETerrainLayer.Back] = backTilemapObject.GetComponent<Tilemap>();

			Assert.IsTrue(tiles[0] == null);
		}

		void Start()
		{
			// init world
			mPrevPivotChunk = CvtWorld2ChunkCoord(pivot.position);
			for (int y = -simulateDepth.y; y <= simulateDepth.y; ++y)
			{
				for (int x = -simulateDepth.x; x <= simulateDepth.x; ++x)
				{
					Vector2Int coord = mPrevPivotChunk + new Vector2Int(x, y);
					mChunks[coord] = new Chunk(coord);
					IEnumerator routine = mChunks[coord].DrawCoroutine();
					while (routine.MoveNext()) { }
				}
			}
		}

		void Update()
		{
			// update chunks : add/remove
			foreach (var chunk in mChunks.Values)
			{
				chunk.Update();
			}

			Vector2Int pivotChunk = CvtWorld2ChunkCoord(pivot.position);
			if (pivotChunk != mPrevPivotChunk)
			{
				mPrevPivotChunk = pivotChunk;

				HashSet<Vector2Int> newChunkSet = new HashSet<Vector2Int>();
				// load new chunk
				for (int y = -simulateDepth.y; y <= simulateDepth.y; ++y)
				{
					for (int x = -simulateDepth.x; x <= simulateDepth.x; ++x)
					{
						Vector2Int coord = pivotChunk + new Vector2Int(x, y);
						newChunkSet.Add(coord);
					}
				}

				// add load chunk
				foreach (var coord in newChunkSet.ToList())
				{
					if (!mChunks.ContainsKey(coord))
					{
						int removeIndex = mFreeChunkQueue.FindIndex(x => x.Item1 == coord);
						if (removeIndex != -1)
						{
							mFreeChunkQueue.RemoveAt(removeIndex);
						}
						else if (mLoadChunkQueue.Exist(x => x.Item1 == coord))
						{
							goto LOAD_EXIT;
						}
						mChunks[coord] = new Chunk(coord);
						mLoadChunkQueue.Enqueue((coord, mChunks[coord]));
					LOAD_EXIT:;
					}
				}
				// add free chunk
				foreach (var coord in mChunks.Keys.ToList())
				{
					if (!newChunkSet.Contains(coord))
					{
						int removeIndex = mLoadChunkQueue.FindIndex(x => x.Item1 == coord);
						if (removeIndex != -1)
						{
							mLoadChunkQueue.RemoveAt(removeIndex);
						}
						else if (mFreeChunkQueue.Exist(x => x.Item1 == coord))
						{
							goto FREE_EXIT;
						}
						mFreeChunkQueue.Enqueue((coord, mChunks[coord]));
						mChunks.Remove(coord);
					FREE_EXIT:;
					}
				}

				return;
			}

			// process queries
			if (mLoadChunkQueue.Count + mFreeChunkQueue.Count > 0)
			{
				int batchCount = 0;
				while (mLoadChunkQueue.Count > 0 && batchCount < updateChunkBatchSize)
				{
					var (dummy, chunk) = mLoadChunkQueue.Dequeue();
					StartCoroutine(chunk.DrawCoroutine());
					++batchCount;
				}
				while (mFreeChunkQueue.Count > 0 && batchCount < updateChunkBatchSize)
				{
					var (dummy, chunk) = mFreeChunkQueue.Dequeue();
					StartCoroutine(chunk.EraseCoroutine());
					++batchCount;
				}

				mbCompressBound = ((mLoadChunkQueue.Count + mFreeChunkQueue.Count) == 0);
				return;
			}

			// compress tilemap bounds
			if (mbCompressBound)
			{
				foreach (var tilemap in tilemapList)
				{
					tilemap.CompressBounds();
				}
				mbCompressBound = false;
			}
			return;
		}

		public Chunk GetChunk(Vector2Int chunkIdx)
		{
			Assert.IsTrue(mChunks.ContainsKey(chunkIdx));
			return mChunks[chunkIdx];
		}

		public Vector2 GetGridCellSize()
		{
			return mGrid.cellSize;
		}

		public Vector2Int CvtWorld2ChunkCoord(Vector2 position)
		{
			return new Vector2Int(
				Mathf.FloorToInt(position.x / chunkSize.x + 0.5f), // (x / (SIZE / 2) + 1) / 2
				Mathf.FloorToInt(position.y / chunkSize.y + 0.5f));
		}

		public Vector2 CvtChunk2WorldCoord(Vector2Int chunkIdx)
		{
			return chunkSize * (chunkIdx - new Vector2(0.5f, 0.5f)) + chunkSize / 2;
		}

		public Vector2 CvtChunk2WorldBaseCoord(Vector2Int chunkIdx)
		{
			return chunkSize * (chunkIdx - new Vector2(0.5f, 0.5f));
		}

		public Vector3Int CvtChunk2TilemapBaseCoord(Vector2Int chunkIdx)
		{
			Vector2 worldBaseCoord = CvtChunk2WorldBaseCoord(chunkIdx);
			Vector3Int tilemapBaseCoord = new Vector3Int(
				Mathf.FloorToInt(worldBaseCoord.x / mGrid.cellSize.x),
				Mathf.FloorToInt(worldBaseCoord.y / mGrid.cellSize.y),
				0);
			return tilemapBaseCoord;
		}
	}
}