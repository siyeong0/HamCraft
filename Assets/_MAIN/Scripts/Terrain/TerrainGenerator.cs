using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TerrainGenerator : MonoBehaviour
{
	[SerializeField] int width, height;
	[SerializeField] Tilemap frontTilemap;
	[SerializeField] Tilemap rearTilemap;
	[SerializeField] TileBase[] tiles;

	[Header("Terrain Generation")]
	[SerializeField] float smoothness = 64;
	[SerializeField] int avgGrassHeight = 5;
	[SerializeField] float seed;
	// [SerializeField] int interval;
	int interval;

	[Header("Cave Generation")]
	[Range(0, 1)]
	[SerializeField] float modifier;
	[SerializeField] int smoothCount;


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
		seed = Random.Range(1, 10000);
		frontTilemap.ClearAllTiles();
		rearTilemap.ClearAllTiles();

		int[,] terrain = generateTerrainSmoothPerlin();
		// generateCavePerlin(map);
		int[,] cave = generateCaveCelluarAutomata();

		int[,] map = new int[width, height];
		for (int x = 0; x < width; ++x)
		{
			for (int y = 0; y < height; ++y)
			{
				map[x, y] = cave[x, y] == 0 ? 0 : terrain[x, y];
			}
		}

		renderMap(frontTilemap, map);
		renderMap(rearTilemap, terrain);
	}

	int[,] generateTerrainSmoothPerlin()
	{
		int[,] map = new int[width, height];
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
				map[x, y] = 2;  // stone
			}
			for (int gh = 0; gh < grassHeight; ++gh)
			{
				map[x, y + gh] = 1;  // grass
			}
		}

		return map;
	}

	void generateCavePerlin(int[,] map)
	{
		for (int x = 0; x < width; ++x)
		{
			for (int y = 0; y < height; ++y)
			{
				int caveValue = Mathf.RoundToInt(Mathf.PerlinNoise((float)x * modifier + seed, (float)y * modifier + seed));
				map[x, y] = caveValue == 1 ? 0 : map[x, y];
			}
		}
	}

	int[,] generateCaveCelluarAutomata()
	{
		int[,] map = new int[width, height];
		for (int x = 0; x < width; ++x)
		{
			for (int y = 0; y < height; ++y)
			{
				if (x == 0 || x == width - 1 || y == 0 || y == height - 1)
				{
					map[x, y] = 1;
				}
				else
				{
					map[x, y] = Random.Range(0f, 1f) < modifier ? 0 : 1;
				}
			}
		}

		for (int i = 0; i < smoothCount; ++i)
		{
			int[,] bufferMap = new int[width, height];
			for (int x = 0; x < width; ++x)
			{
				for (int y = 0; y < height; ++y)
				{
					if (x == 0 || x == width - 1 || y == 0 || y == height - 1)
					{
						bufferMap[x, y] = 1;
					}
					else
					{
						if (countAdjacentWalls(map, x, y) >= 5 || countNearbyWalls(map, x, y) <= 2)
						{
							bufferMap[x, y] = 1;
						}
					}
				}
			}
			map = bufferMap;
		}

		return map;
	}

	int countAdjacentWalls(int[,] map, int x, int y)
	{
		int count = 0;
		for (int neighborX = x - 1; neighborX <= x + 1; ++neighborX)
		{
			for (int neighborY = y - 1; neighborY <= y + 1; ++neighborY)
			{
				if (neighborX >= 0 && neighborX < width && neighborY >= 0 && neighborY < height)
				{
					if (map[neighborX, neighborY] != 0)
					{
						count++;
					}
				}
			}
		}
		return count;
	}

	int countNearbyWalls(int[,] map, int x, int y)
	{
		int count = 0;
		for (int neighborX = x - 2; neighborX <= x + 2; ++neighborX)
		{
			for (int neighborY = y - 2; neighborY <= y + 2; ++neighborY)
			{
				if (Mathf.Abs(neighborX - x) == 2 && Mathf.Abs(neighborY - y) == 2)
					continue;

				if (neighborX < 0 || neighborY < 0 || neighborX >= width || neighborY >= height)
					continue;

				if (map[neighborX, neighborY] != 0)
					++count;
			}
		}
		return count;
	}

	void renderMap(Tilemap tilemap, int[,] map)
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
