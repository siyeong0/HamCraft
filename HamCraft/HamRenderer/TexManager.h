#pragma once
#include <iostream>
#include <unordered_map>
#include <SDL2/SDL.h>

#include"../Common/Common.h"
#include "../HamData/Tileset.h"

class TexManager
{
public:
	TexManager();
	~TexManager();

	bool Initialize(SDL_Renderer** targetRenderer);
	void Finalize();

	void GetTileTex(SDL_Texture** outTex, SDL_Rect* outRect, std::string texName, int col, int row);

private:
	SDL_Renderer** mTargetRenderer;
	std::unordered_map<std::string, Tileset> mTilesetMap;
	std::unordered_map<std::string, SDL_Texture*> mTextureMap;
};