using UnityEngine;
using UnityEngine.Tilemaps;

namespace Terrain
{
	public class ProceduralTerrainGenerator : MonoBehaviour
	{
		[SerializeField] int seed;
		[SerializeField] Vector2Int chunkSize;
		[SerializeField] Vector2Int simulationChunkDepth;
		[SerializeField] Transform pivotTransform;

		[SerializeField] GameObject tilemapPrefab;
		[SerializeField] TileBase[] tiles;

		[SerializeField] float smoothness;

		private struct Chunk
		{
			public GameObject Tilemap;
			public Vector2Int Index;
		}
		Chunk[,] mChunks;
		Vector2Int mChunkArrSize;
		Vector2 mCellSize;
		Vector2 mChunkAbsSize;
		Vector2Int mPivotGlobalIdx;
		Vector2Int mPivotLocalIdx;
		// Start is called once before the first execution of Update after the MonoBehaviour is created
		void Start()
		{
			mCellSize = GetComponent<Grid>().cellSize;
			mChunkAbsSize = mCellSize * chunkSize;
			updatePivotGlobalIdx();

			mChunkArrSize = new Vector2Int(1 + simulationChunkDepth.x * 2, 1 + simulationChunkDepth.y * 2);
			mChunks = new Chunk[mChunkArrSize.x, mChunkArrSize.y];
			for (int x = 0; x < mChunkArrSize.x; ++x)
			{
				for (int y = 0; y < mChunkArrSize.y; ++y)
				{
					mChunks[x, y].Index = mPivotGlobalIdx + new Vector2Int(x - simulationChunkDepth.x, y - simulationChunkDepth.y);
					mChunks[x, y].Tilemap = Instantiate(tilemapPrefab, 
						new Vector3(mChunks[x, y].Index.x * mChunkAbsSize.x, mChunks[x, y].Index.y * mChunkAbsSize.y), 
						Quaternion.identity);
					updateChunk(new Vector2Int(x,y), mChunks[x, y].Index);
				}
			}
			mPivotLocalIdx = new Vector2Int(simulationChunkDepth.x, simulationChunkDepth.y);
		}

		// Update is called once per frame
		void Update()
		{
			Vector2Int prevPivotGlobalIdx = mPivotGlobalIdx;
			updatePivotGlobalIdx();

			if (prevPivotGlobalIdx != mPivotGlobalIdx)
			{
				Vector2Int diff = prevPivotGlobalIdx - mPivotGlobalIdx;
				mPivotLocalIdx.x += diff.x;
				mPivotLocalIdx.y += diff.x;

				int[] signs = { -1, 1 };
				foreach (int sign in signs)
				{
					for (int x = 0; x < Mathf.Abs(diff.x); ++x)
					{
						for (int y = 0; y < mChunkArrSize.y; ++y)
						{
							Vector2Int localIdx = new Vector2Int(
								cellularIndex(mPivotLocalIdx.x + sign * simulationChunkDepth.x + x, mChunkArrSize.x),
								cellularIndex(mPivotLocalIdx.y + y, mChunkArrSize.y));
							Vector2Int globalIdx = new Vector2Int(
								mPivotGlobalIdx.x + sign * simulationChunkDepth.x + x, 
								mPivotGlobalIdx.y + y);
							updateChunk(localIdx, globalIdx);
						}
					}
					for (int y = 0; y < Mathf.Abs(diff.y); ++y)
					{
						for (int x = 0; x < mChunkArrSize.x; ++x)
						{
							Vector2Int localIdx = new Vector2Int(
								cellularIndex(mPivotLocalIdx.x + x, mChunkArrSize.x),
								cellularIndex(mPivotLocalIdx.y + sign * simulationChunkDepth.y + y, mChunkArrSize.y));
							Vector2Int globalIdx = new Vector2Int(
								mPivotGlobalIdx.x + x,
								mPivotGlobalIdx.y + sign * simulationChunkDepth.y + y);
							updateChunk(localIdx, globalIdx);
						}
					}
				}
			}
		}

		void updateChunk(Vector2Int chunkLocalIdx, Vector2Int chunkGlobalIdx)
		{
			mChunks[chunkLocalIdx.x, chunkLocalIdx.y].Index = chunkGlobalIdx;
			Transform transform = mChunks[chunkLocalIdx.x, chunkLocalIdx.y].Tilemap.GetComponent<Transform>();
			transform.position = new Vector3(chunkGlobalIdx.x * mChunkAbsSize.x,chunkGlobalIdx.y * mChunkAbsSize.y, 0);
			Tilemap targetTilemap = mChunks[chunkLocalIdx.x, chunkLocalIdx.y].Tilemap.GetComponent<Tilemap>();
			targetTilemap.ClearAllTiles();

			int beginXIdx = (int)(chunkGlobalIdx.x * chunkSize.x);
			for (int x = 0; x < chunkSize.x; x++)
			{
				float perlin = Mathf.PerlinNoise((float)(beginXIdx + x) / smoothness, seed) - 0.1f;
				int perlinHeight = Mathf.RoundToInt(perlin);
				for (int y = 0; transform.position.y + y * mCellSize.y < perlinHeight; y++)
				{
					targetTilemap.SetTile(new Vector3Int(x, y, 0), tiles[Random.Range(0, tiles.Length)]);
				}
			}
		}

		void updatePivotGlobalIdx()
		{
			mPivotGlobalIdx.x = Mathf.RoundToInt(pivotTransform.position.x / mChunkAbsSize.x);
			mPivotGlobalIdx.y = Mathf.RoundToInt(pivotTransform.position.y / mChunkAbsSize.y);
		}

		int cellularIndex(int idx, int size)
		{
			return (idx + size) % size;
		}
	}
}
