using UnityEngine;

using Terrain;
using System.Collections.Generic;
using UnityEngine.Assertions;
using System.Linq;
using UnityEngine.Tilemaps;
using System.Diagnostics;
using System;

public class ChunkManager : MonoBehaviour
{
	public static ChunkManager Instance { get; private set; }

	[SerializeField] Transform pivot;
	[SerializeField] GameObject chunkPrefab;
	[SerializeField] public Vector2Int chunkSize;
	[SerializeField] Vector2Int simulateDepth;
	[SerializeField] public int updateBatchSize;
	[SerializeField] public TileBase[] tiles;

	Grid mGrid;
	public TerrainGenerator terrainGenerator { get; private set; }

	public List<Tilemap> tilemapList { get; private set; }
	public List<TilemapRenderer> tilemapRendererList { get; private set; }

	Dictionary<Vector2Int, Chunk> mChunks;
	Vector2Int mCurrPivotChunk;
	bool mbUpdateChunk;

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
	}
	void Start()
	{
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
		tilemapRendererList = new List<TilemapRenderer>();
		for (int i = 0; i < numLayers; ++i) tilemapRendererList.Add(null);
		tilemapRendererList[(int)ETerrainLayer.Front] = frontTilemapObject.GetComponent<TilemapRenderer>();
		tilemapRendererList[(int)ETerrainLayer.Back] = backTilemapObject.GetComponent<TilemapRenderer>();

		mChunks = new Dictionary<Vector2Int, Chunk>();
		mCurrPivotChunk = CvtWorld2ChunkCoord(pivot.position);
		mbUpdateChunk = true;

		Assert.IsTrue(updateBatchSize >= 1);
		Assert.IsTrue(chunkSize.y % updateBatchSize == 0);
		Assert.IsTrue(tiles[0] == null);
	}

	void Update()
	{
		if (mbUpdateChunk) // load/unload chunks
		{
			HashSet<Vector2Int> newChunkSet = new HashSet<Vector2Int>();

			// load new chunk
			for (int y = -simulateDepth.y; y <= simulateDepth.y; ++y)
			{
				for (int x = -simulateDepth.x; x <= simulateDepth.x; ++x)
				{
					Vector2Int coord = mCurrPivotChunk + new Vector2Int(x, y);
					newChunkSet.Add(coord);

					if (!mChunks.ContainsKey(coord))
					{
						mChunks[coord] = new Chunk(coord);
						StartCoroutine(mChunks[coord].DrawCoroutine());
					}
				}
			}

			// unload old chunk
			var keys = mChunks.Keys.ToList();
			foreach (var coord in keys)
			{
				if (!newChunkSet.Contains(coord))
				{
					StartCoroutine(mChunks[coord].EraseCoroutine());
					mChunks.Remove(coord);
				}
			}

			// resize tilemaps
			foreach(var tilemap in tilemapList)
			{
				tilemap.CompressBounds();
			}
		}

		// update chunks : add/remove
		for (int i = 0; i < mChunks.Count; ++i)
		{
			Chunk chunk = mChunks.ElementAt(i).Value;
			chunk.Update();
		}

		Vector2Int pivotChunk = CvtWorld2ChunkCoord(pivot.position);
		mbUpdateChunk = pivotChunk != mCurrPivotChunk;
		mCurrPivotChunk = pivotChunk;
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
}