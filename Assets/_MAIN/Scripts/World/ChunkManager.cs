using UnityEngine;

using Terrain;
using System.Collections.Generic;
using UnityEngine.Assertions;
using System.Linq;
using UnityEngine.Tilemaps;

public class ChunkManager : MonoBehaviour
{
	public static ChunkManager Instance { get; private set; }

	[SerializeField] GameObject chunkPrefab;
	[SerializeField] public Vector2Int chunkSize;

	[SerializeField] Transform pivot;
	[SerializeField] Vector2Int simulateDepth;

	[SerializeField] TileBase[] tiles;

	Grid mGrid;
	TerrainGenerator mTerrainGenerator;

	GameObject mFrontTilemapObject;
	GameObject mBackTilemapObject;
	Tilemap mFrontTilemap;
	Tilemap mBackTilemap;

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
		mTerrainGenerator = GetComponent<TerrainGenerator>();

		GameObject chunk = Instantiate(chunkPrefab);
		mFrontTilemapObject = chunk.transform.GetChild(1).gameObject;
		mBackTilemapObject = chunk.transform.GetChild(0).gameObject;
		mFrontTilemapObject.transform.SetParent(transform);
		mBackTilemapObject.transform.SetParent(transform);
		Destroy(chunk);

		mFrontTilemap = mFrontTilemapObject.GetComponent<Tilemap>();
		mBackTilemap = mBackTilemapObject.GetComponent<Tilemap>();

		mChunks = new Dictionary<Vector2Int, Chunk>();
		mCurrPivotChunk = CvtWorld2ChunkCoord(pivot.position);
		mbUpdateChunk = true;
	}

	void Update()
	{
		if (mbUpdateChunk)
		{
			// update chunks
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
						mChunks[coord] = new Chunk(coord, mTerrainGenerator);
						mChunks[coord].Draw(mFrontTilemap, mBackTilemap, tiles);
					}
				}
			}

			// unload old chunk
			var keys = mChunks.Keys.ToList();
			foreach (var coord in keys)
			{
				if (!newChunkSet.Contains(coord))
				{
					mChunks[coord].Erase(mFrontTilemap, mBackTilemap);
					mChunks.Remove(coord);
				}
			}

			// resize tilemaps
			mFrontTilemap.CompressBounds();
			mBackTilemap.CompressBounds();

			//BoundsInt defaultBound = mFrontTilemap.cellBounds;
			//BoundsInt compressBound = mBackTilemap.cellBounds;
			//Debug.Log($"Default Bound: {defaultBound} Compress Bound :  {compressBound}");
		}

		for (int i = 0; i < mChunks.Count; ++i)
		{
			Chunk chunk = mChunks.ElementAt(i).Value;
			if (chunk.IsDirty())
			{
				chunk.Draw(mFrontTilemap, mBackTilemap, tiles);
			}
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