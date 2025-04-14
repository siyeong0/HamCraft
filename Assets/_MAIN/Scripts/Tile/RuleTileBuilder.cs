using UnityEngine;
using UnityEditor;
using UnityEngine.Tilemaps;
using UnityEditor.Tilemaps;
using System.IO;
using System.Collections.Generic;
using UnityEditor.U2D.Sprites;
using Unity.VisualScripting;
using System.Linq;

public class RuleTileBuilder
{
	[MenuItem("Tools/Build HamRuleTile from PNG")]
	public static void BuildHamRuleTile()
	{
		string path = EditorUtility.OpenFilePanel("Select PNG", "", "png");
		string fileName = Path.GetFileNameWithoutExtension(path);
		if (string.IsNullOrEmpty(path))
			return;
		string assetPath = FileUtil.GetProjectRelativePath(path);
		// load texture
		TextureImporter textureImporter = AssetImporter.GetAtPath(assetPath) as TextureImporter;
		if (textureImporter == null)
		{
			Debug.LogError("Selected file is not a valid texture.");
			return;
		}

		textureImporter.spriteImportMode = SpriteImportMode.Multiple;
		textureImporter.isReadable = true;
		textureImporter.textureType = TextureImporterType.Sprite;
		textureImporter.spritePixelsPerUnit = 16;
		textureImporter.filterMode = FilterMode.Point;
		textureImporter.textureCompression = TextureImporterCompression.Uncompressed;
		AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);

		Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
		int textureWidth = texture.width;
		int textureHeight = texture.height;

		// slice to tiles
		int tileWidth = 16;
		int tileHeight = 16;

		var factory = new SpriteDataProviderFactories();
		factory.Init();

		var dataProvider = factory.GetSpriteEditorDataProviderFromObject(texture);
		dataProvider.InitSpriteEditorDataProvider();

		var spriteRects = new List<SpriteRect>();
		for (int y = 0; y < textureHeight; y += tileHeight)
		{
			for (int x = 0; x < textureWidth; x += tileWidth)
			{
				SpriteRect spriteData = new SpriteRect
				{
					name = fileName + "_" + "(" + (y / tileHeight) * (textureWidth / tileWidth) + ","+ (x / tileWidth) + ")",
					spriteID = GUID.Generate(),
					rect = new Rect(x, y, tileWidth, tileHeight),
				};
				spriteRects.Add(spriteData);
			}
		}
		dataProvider.SetSpriteRects(spriteRects.ToArray());
		dataProvider.Apply();

		// refresh the asset database
		var assetImporter = dataProvider.targetObject as AssetImporter;
		assetImporter.SaveAndReimport();
		AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);

		// build rule tile
		Object[] assets = AssetDatabase.LoadAllAssetsAtPath(assetPath);

		List<Sprite> sprites = new List<Sprite>();
		foreach (var asset in assets)
		{
			if (asset is Sprite sprite)
			{
				sprites.Add(sprite);
			}
		}

		if (sprites.Count == 0)
		{
			Debug.LogError("No sprites found in PNG.");
			return;
		}

		HamRuleTile ruleTile = ScriptableObject.CreateInstance<HamRuleTile>();
		ruleTile.m_DefaultSprite = sprites[0];

		int THIS = 1;
		int EMTPY = 2;
		for (int i = 0; i < RULE_LIST.GetLength(0); i++)
		{
			RuleTile.TilingRule rule = new RuleTile.TilingRule();
			rule.m_Sprites = new Sprite[] { sprites[i] };
			rule.m_NeighborPositions = new List<Vector3Int>();
			rule.m_Neighbors = new List<int>();
			for (int j = 0; j < RULE_LIST.GetLength(1); j++)
			{
				switch (RULE_LIST[i, j])
				{
					case 0: // empty
						rule.m_NeighborPositions.Add(RULE_POSITIONS[j]);
						rule.m_Neighbors.Add(EMTPY);
						break;
					case 1: // this
						rule.m_NeighborPositions.Add(RULE_POSITIONS[j]);
						rule.m_Neighbors.Add(THIS);
						break;
					case 2: // any
						break;
					default:
						break;
				}
			}

			ruleTile.m_TilingRules.Add(rule);
		}

		string savePath = $"Assets/_MAIN/Resources/TileGraphic/{fileName}.asset";
		AssetDatabase.CreateAsset(ruleTile, savePath);
		AssetDatabase.SaveAssets();
		AssetDatabase.Refresh();
	}

	static Vector3Int[] RULE_POSITIONS = new Vector3Int[]
	{
		new(-1, 1, 0),
		new(0, 1, 0),
		new(1, 1, 0),
		new(-1, 0, 0),
		new(1, 0, 0),
		new(-1, -1, 0),
		new(0, -1, 0),
		new(1, -1, 0)
	};

	static int[,] RULE_LIST = new int[,]
	{
		// row 0							
		{ 0,0,0, 0,0, 0,0,0 }, // (0, 0) 
		{ 2,0,2, 0,1, 2,0,2 }, // (0, 1) 
		{ 2,0,2, 1,1, 2,0,2 }, // (0, 2) 
		{ 2,0,2, 1,0, 2,0,2 }, // (0, 3) 

		{ 0,1,0, 1,1, 1,1,0 }, // (0, 4) 
		{ 0,1,1, 1,1, 2,0,2 }, // (0, 5) 
		{ 1,1,0, 1,1, 2,0,2 }, // (0, 6) 
		{ 0,1,0, 1,1, 0,1,1 }, // (0, 7) 

		{ 2,1,1, 0,1, 2,0,2 }, // (0, 8) 
		{ 1,1,1, 1,1, 2,0,2 }, // (0, 9) 
		{ 1,1,1, 1,1, 0,1,0 }, // (0,10) 
		{ 1,1,2, 1,0, 2,0,2 }, // (0,11) 
		// row 1
		{ 2,1,2, 0,0, 2,0,2 }, // (1, 0) 
		{ 2,1,0, 0,1, 2,0,2 }, // (1, 1) 
		{ 0,1,0, 1,1, 2,0,2 }, // (1, 2) 
		{ 0,1,2, 1,0, 2,0,2 }, // (1, 3) 
		
		{ 2,1,1, 0,1, 2,1,0 }, // (1, 4) 
		{ 1,1,1, 1,1, 0,1,1 }, // (1, 5) 
		{ 1,1,1, 1,1, 1,1,0 }, // (1, 6) 
		{ 1,1,2, 1,0, 0,1,2 }, // (1, 7) 

		{ 0,1,1, 1,1, 0,1,1 }, // (1, 8) 
		{ 1,1,1, 1,1, 1,1,1 }, // (1, 9) 
		{ 1,1,0, 1,1, 0,1,1 }, // (1,10) 
		{ 1,1,2, 1,0, 1,1,2 }, // (1,11) 
		// row 2
		{ 2,1,2, 0,0, 2,1,2 }, // (2, 0) 
		{ 2,1,0, 0,1, 2,1,0 }, // (2, 1) 
		{ 0,1,0, 1,1, 0,1,0 }, // (2, 2) 
		{ 0,1,2, 1,0, 0,1,2 }, // (2, 3) 

		{ 2,1,0, 0,1, 2,1,1 }, // (2, 4) 
		{ 0,1,1, 1,1, 1,1,1 }, // (2, 5) 
		{ 1,1,0, 1,1, 1,1,1 }, // (2, 6) 
		{ 0,1,2, 1,0, 1,1,2 }, // (2, 7) 
		
		{ 2,1,1, 0,1, 2,1,1 }, // (2, 8) 
		{ 0,1,1, 1,1, 1,1,0 }, // (2, 9) 
		{ 0,0,0, 0,0, 0,0,0 }, // (2, 10) 
		{ 1,1,0, 1,1, 1,1,0 }, // (2, 11) 
		// row 3
		{ 2,0,2, 0,0, 2,1,2 }, // (3, 0) 
		{ 2,0,2, 0,1, 2,1,0 }, // (3, 1)
		{ 2,0,2, 1,1, 0,1,0 }, // (3, 2)
		{ 2,0,2, 1,0, 0,1,2 }, // (3, 3)

		{ 1,1,0, 1,1, 0,1,0 }, // (3, 4)
		{ 2,0,2, 1,1, 0,1,1 }, // (3, 5)
		{ 2,0,2, 1,1, 1,1,0 }, // (3, 6)
		{ 0,1,1, 1,1, 0,1,0 }, // (3, 7)

		{ 2,0,2, 0,1, 2,1,1 }, // (3, 8)
		{ 0,1,0, 1,1, 1,1,1 }, // (3, 9)
		{ 2,0,2, 1,1, 1,1,1 }, // (3,10)
		{ 2,0,2, 1,0, 1,1,2 }, // (3,11)
	};
}
