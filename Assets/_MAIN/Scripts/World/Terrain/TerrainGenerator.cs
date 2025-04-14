using System;
using System.Collections.Generic;
using UnityEngine;

namespace Terrain
{
	public class TerrainGenerator : MonoBehaviour
	{
		[Header("Terrain Generation")]
		[SerializeField] float smoothness;
		[SerializeField] float maxHeight;
		[SerializeField] int avgGrassHeight;
		[SerializeField] float seed;

		[Header("Cave Generation")]
		[SerializeField][Range(0, 1)] float modifier;
		[SerializeField] int smoothCount;

		int width;
		int height;

		const int GRASS_DIFF = 2;
		const int GRASS_SECTION_WIDTH = 5;
		const float PERLINE_OFFSET = 5000f; // to avoid repeating patterns
		void Start()
		{
			width = ChunkManager.Instance.chunkSize.x;
			height = ChunkManager.Instance.chunkSize.y;
		}

		public List<int[,]> Generate(Vector2 positionOffset)
		{
			// alloc buffers
			int numLayers = Enum.GetValues(typeof(ETerrainLayer)).Length;
			var mapBuffers = new List<int[,]>();
			for (int i = 0; i < numLayers; ++i) mapBuffers.Add(null);

			// generate terrain
			int[,] terrain = generateTerrainSmoothPerlin(positionOffset);
			int[,] cave = generateCavePerlin(positionOffset);

			int[,] frontMap = new int[width, height];
			for (int x = 0; x < width; ++x)
			{
				for (int y = 0; y < height; ++y)
				{
					frontMap[x, y] = cave[x, y] == 0 ? 0 : terrain[x, y];
				}
			}

			mapBuffers[(int)ETerrainLayer.Front] = frontMap;
			mapBuffers[(int)ETerrainLayer.Back] = terrain;
			return mapBuffers;
		}

		int[,] generateTerrainSmoothPerlin(Vector2 positionOffset)
		{
			int[,] map = new int[width, height];
			// height map
			int[] heightMap = new int[width];
			for (int x = 0; x < width; ++x)
			{
				int perlineHeight = Mathf.RoundToInt(Mathf.PerlinNoise((positionOffset.x + (float)(x + PERLINE_OFFSET)) / smoothness, seed) * maxHeight);
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
				int y = 0;
				for (; y < height; ++y)
				{
					if (positionOffset.y + y > heightMap[x] - grassHeightMap[x])
						break;

					map[x, y] = 2;  // stone
				}

				for (int gh = 0; y + gh < height; ++gh)
				{
					if (positionOffset.y + y + gh > heightMap[x])
						break;
					map[x, y + gh] = 1;  // grass
				}
			}

			return map;
		}

		int[,] generateCavePerlin(Vector2 positionOffset)
		{
			int[,] map = new int[width, height];
			for (int x = 0; x < width; ++x)
			{
				for (int y = 0; y < height; ++y)
				{
					int caveValue = Mathf.RoundToInt(Mathf.PerlinNoise(
						((float)(x + PERLINE_OFFSET) + positionOffset.x) * modifier + seed,
						((float)(y + PERLINE_OFFSET) + positionOffset.y) * modifier + seed));
					map[x, y] = caveValue == 1 ? 0 : 1;
				}
			}
			return map;
		}

		int[,] generateCaveCelluarAutomata(Vector2 positionOffset)
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
						map[x, y] = UnityEngine.Random.Range(0f, 1f) < modifier ? 0 : 1;
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
	}
}