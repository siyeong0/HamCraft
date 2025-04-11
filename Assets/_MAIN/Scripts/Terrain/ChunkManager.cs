using UnityEngine;

using Terrain;
using System.Collections.Generic;
using Unity.VisualScripting;
using System.Linq;
using UnityEngine.Tilemaps;

public class ChunkManager : MonoBehaviour
{
	public static ChunkManager Instance { get; private set; }

	[SerializeField] GameObject chunkPrefab;
	[SerializeField] public Vector2Int chunkSize;

	[SerializeField] Transform pivot;
    [SerializeField] Vector2Int simulateDepth;

	[Header("Terrain Generation")]
	[SerializeField] public TileBase[] tiles;

	[SerializeField] public float smoothness = 64;
	[SerializeField] public int avgGrassHeight = 5;
	[SerializeField] public float seed;

	[Header("Cave Generation")]
	[Range(0, 1)]
	[SerializeField] public float modifier;
	[SerializeField] public int smoothCount;

	TerrainGenerator mTerrainGenerator;
	Dictionary<Vector2Int, Terrain.Chunk> mChunks;
	Vector2Int mCurrPivotChunk;
	bool mbUpdateChunk;
	Vector2Int CENTER_INDEX;

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
		mTerrainGenerator = new TerrainGenerator();
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
						mChunks[coord] = new Chunk(coord, chunkPrefab, mTerrainGenerator);
					}
				}
			}

			// unload old chunk
			var keys = mChunks.Keys.ToList();
			foreach (var coord in keys)
			{
				if (!newChunkSet.Contains(coord))
				{
					mChunks[coord].Destroy();
					mChunks.Remove(coord);
				}
			}

			// refresh tiles
			foreach (var chunk in mChunks)
			{
				chunk.Value.RefreshTile();
			}
		}
		Vector2Int pivotChunk = CvtWorld2ChunkCoord(pivot.position);
		mbUpdateChunk = pivotChunk != mCurrPivotChunk;
		mCurrPivotChunk = pivotChunk;
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
