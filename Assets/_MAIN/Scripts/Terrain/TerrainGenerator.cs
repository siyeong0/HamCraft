using UnityEngine;
using UnityEngine.Tilemaps;

public class TerrainGenerator : MonoBehaviour
{
    [SerializeField] int width, height;
    [SerializeField] float smoothness;
    [SerializeField] float seed;

    [SerializeField] TileBase[] tiles;
    [SerializeField] Tilemap tilemap;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Generate();
    }

    // Update is called once per frame
    void Update()
    {

    }

    void Generate()
    {
		tilemap.ClearAllTiles();
        int[,] map = generateArray(width, height, true);
        generateTerrain(map);
        renderMap(map);
    }

    int[,] generateArray(int width, int height, bool empty)
    {
        int[,] map = new int[width, height];
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                map[x, y] = empty ? 0 : 1;
            }
        }
        return map;
    }

    void generateTerrain(int[,] map)
    {
        int perlinHeight;
        for (int x = 0; x < width; x++)
        {
            perlinHeight = Mathf.RoundToInt(Mathf.PerlinNoise((float)x / smoothness, seed) * height / 2);
            perlinHeight += height / 2;
			for (int y = 0; y < perlinHeight; y++)
            {
                map[x, y] = 1;
            }
        }

    }

    void renderMap(int[,] map)
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (map[x, y] == 1)
                {
                    tilemap.SetTile(new Vector3Int(x - width / 2, y - height, 0), tiles[Random.Range(0, tiles.Length)]);
                }
            }
        }
    }
}
