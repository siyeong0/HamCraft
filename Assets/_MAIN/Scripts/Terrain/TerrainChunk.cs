using UnityEngine;
using UnityEngine.Tilemaps;

namespace Terrain
{
	public class Chunk
	{
		Vector2Int mChunkIdx;
		GameObject mFrontTilemapObject;
		GameObject mBackTilemapObject;
		Tilemap mFrontTilemap;
		Tilemap mBackTilemap;

		public Chunk(Vector2Int chunkIdx, GameObject chunkPrefab, TerrainGenerator terrainGenerator)
		{
			mChunkIdx = chunkIdx;
			Vector2 chunkPosition = ChunkManager.Instance.CvtChunk2WorldCoord(chunkIdx);
			GameObject chunk = Object.Instantiate(chunkPrefab, chunkPosition, Quaternion.identity);

			mFrontTilemapObject = chunk.transform.GetChild(1).gameObject;
			mBackTilemapObject = chunk.transform.GetChild(0).gameObject;
			mFrontTilemap = mFrontTilemapObject.GetComponent<Tilemap>();
			mBackTilemap = mBackTilemapObject.GetComponent<Tilemap>();

			// set parent
			mFrontTilemapObject.transform.SetParent(ChunkManager.Instance.transform);
			mBackTilemapObject.transform.SetParent(ChunkManager.Instance.transform);
			Object.Destroy(chunk);

			// generate terrain
			Vector2 chunkBasePosition = ChunkManager.Instance.CvtChunk2WorldBaseCoord(chunkIdx);
			terrainGenerator.Generate(chunkBasePosition, mFrontTilemap, mBackTilemap);
		}

		public void Destroy()
		{
			Object.Destroy(mFrontTilemapObject);
			Object.Destroy(mBackTilemapObject);
		}
		public void RefreshTile()
		{
			mFrontTilemap.RefreshAllTiles();
			mBackTilemap.RefreshAllTiles();
		}
	}
}
