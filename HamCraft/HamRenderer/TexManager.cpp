#include "TexManager.h"

TexManager::TexManager()
	: mTargetRenderer(nullptr)
{

}

TexManager::~TexManager()
{

}

bool TexManager::Initialize(SDL_Renderer** targetRenderer)
{
	ASSERT(*targetRenderer)
	mTargetRenderer = targetRenderer;

	Tileset tileset;
	mTilesetMap.insert(std::make_pair("terr_1", tileset));
	mTilesetMap["terr_1"].Load("Resource/Image/Tile/terr_1.png");

	SDL_Texture* tex = SDL_CreateTextureFromSurface(*mTargetRenderer, mTilesetMap["terr_1"].GetSDLSurface());
	// SDL_Texture* tex = SDL_CreateTextureFromSurface(*mTargetRenderer, tileset.GetSDLSurface());
	mTextureMap.insert(std::make_pair("terr_1", tex));

	return true;
}

void TexManager::Finalize()
{
	for (auto tilesetElem : mTilesetMap)
		tilesetElem.second.Free();

	for (auto texutreElem : mTextureMap)
		SDL_DestroyTexture(texutreElem.second);
}

void TexManager::GetTileTex(SDL_Texture** outTex, SDL_Rect* outRect, std::string texName, int col, int row)
{
	*outTex = mTextureMap[texName];
	*outRect = mTilesetMap[texName].GetSrcRect(col, row);
}