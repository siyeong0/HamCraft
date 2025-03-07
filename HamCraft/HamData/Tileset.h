#pragma once
#include "SDL2/SDL.h"
#include "../Common/Common.h"

class Tileset
{
public:
	Tileset();
	~Tileset();


	bool Load(const char* path);
	bool Load(const std::string path);
	void Free();

	SDL_Surface* GetSDLSurface();
	SDL_Rect GetSrcRect(int col, int row);
	
	int GetTileWidth() { return mTileWidth; }
	int GetTileHeight() { return mTileHeight; }

private:
	SDL_Surface* mSDLSurface;
	int mNumTiles;
	int mTileWidth;
	int mTileHeight;
	int mHorShift;
	int mVerShift;
	int mTilesPerRow;
	int mTilesPerCol;
};