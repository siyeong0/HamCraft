using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TerrainGenerator : MonoBehaviour
{
	[SerializeField] int width, height;
	// [SerializeField] int interval;
	int interval;
	[SerializeField] float smoothness = 64;
	[SerializeField] int avgGrassHeight = 5;
	[SerializeField] float seed;


	[SerializeField] TileBase[] tiles;
	[SerializeField] Tilemap tilemap;


	const int GRASS_DIFF = 2;
	const int GRASS_SECTION_WIDTH = 5;
	// Start is called once before the first execution of Update after the MonoBehaviour is created
	void Start()
	{
		Generate();
	}

	// Update is called once per frame
	void Update()
	{
		if (Input.GetKeyDown(KeyCode.F5))
		{
			Generate();
		}
	}

	void Generate()
	{
		if (seed == 0) seed = Random.Range(1, 10000);
		tilemap.ClearAllTiles();
		int[,] map = new int[width, height];
		generateTerrainSmoothPerlin(map);
		renderMap(map);
	}

	void generateTerrainSmoothPerlin(int[,] map)
	{
		// height map
		int[] heightMap = new int[width];
		for (int x = 0; x < width; ++x)
		{
			int perlineHeight = Mathf.RoundToInt(Mathf.PerlinNoise((float)x / smoothness, seed) * height);
			heightMap[x] = perlineHeight;
		}
		// grass height map
		int[] grassHeightMap = new int[width];
		System.Random grassRand = new System.Random(seed.GetHashCode());
		int randomWalkHeight = avgGrassHeight;
		int sectionWidth = 0;
		for (int x = 0; x < width; ++x)
		{
			int nextMove = grassRand.Next(0, 2);
			if (nextMove == 0 && randomWalkHeight > avgGrassHeight - GRASS_DIFF && sectionWidth > GRASS_SECTION_WIDTH)
			{
				--randomWalkHeight;
				sectionWidth = 0;
			}
			else if (nextMove == 1 && randomWalkHeight < avgGrassHeight + GRASS_DIFF && sectionWidth > GRASS_SECTION_WIDTH)
			{
				++randomWalkHeight;
				sectionWidth = 0;
			}
			++sectionWidth;

			grassHeightMap[x] = randomWalkHeight;
		}

		// fill buffer map
		for (int x = 0; x < width; ++x)
		{
			int height = heightMap[x];
			int grassHeight = grassHeightMap[x];
			int y = 0;
			for (; y < height - grassHeight; ++y)
			{
				map[x, y] = 2;	// stone
			}
			for (int gh = 0; gh < grassHeight; ++gh)
			{
				map[x, y + gh] = 1;  // grass
			}
		}
	}

	void generateTerrainSmoothIntervalPerlin(int[,] map)
	{
		float reduction = 0.5f;

		List<int> noiseX = new List<int>();
		List<int> noiseY = new List<int>();

		for (int x = 0; x < width; x += interval)
		{
			int perlinPoint = Mathf.FloorToInt(Mathf.PerlinNoise((float)x / smoothness, seed * reduction) * height);
			noiseX.Add(x);
			noiseY.Add(perlinPoint);
		}

		for (int i = 1; i < noiseY.Count; ++i)
		{
			Vector2Int currPos = new Vector2Int(noiseX[i], noiseY[i]);
			Vector2Int lastPos = new Vector2Int(noiseX[i - 1], noiseY[i - 1]);
			Vector2 diff = currPos - lastPos;

			float heightDiff = diff.y / interval;
			float currHeight = lastPos.y;
			for (int x = lastPos.x; x < currPos.x; ++x)
			{
				for (int y = Mathf.FloorToInt(currHeight); y > 0; --y)
				{
					map[x, y] = 1;
				}
				currHeight += heightDiff;
			}
		}
	}

	void renderMap(int[,] map)
	{
		for (int x = 0; x < width; x++)
		{
			for (int y = 0; y < height; y++)
			{
				// if (map[x, y] == 0) continue;	// TODO: 0번 타일을 투명 타일로 설정
				TileBase tile = tiles[map[x, y]];
				tilemap.SetTile(new Vector3Int(x - width / 2, y - height, 0), tile);
			}
		}
	}
}
