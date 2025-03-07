#include "Tileset.h"
#include "SDL2/SDL_image.h"

Tileset::Tileset()
	: mSDLSurface(nullptr)
	, mNumTiles(-1)
	, mTileWidth(-1)
	, mTileHeight(-1)
	, mHorShift(-1)
	, mVerShift(-1)
	, mTilesPerRow(-1)
	, mTilesPerCol(-1)
{

}

Tileset::~Tileset()
{
	if (mSDLSurface)
		this->Free();
}

bool Tileset::Load(const char* path)
{
	mSDLSurface = IMG_Load(path);
	if (!mSDLSurface)
		return false;
	// TODO: metadata에 따라 초기화
	mNumTiles	= 16 * 16;
	mTileWidth	= 8;
	mTileHeight	= 8;
	mHorShift	= 9;
	mVerShift	= 9;
	mTilesPerRow = 16;
	mTilesPerCol = 16;

	return true;
}

bool Tileset::Load(const std::string path)
{
	return Load(path.c_str());
}

void Tileset::Free()
{
	SDL_FreeSurface(mSDLSurface);
	mSDLSurface = nullptr;
}

SDL_Surface* Tileset::GetSDLSurface()
{
	ASSERT(mSDLSurface != nullptr);
	return mSDLSurface;
}

SDL_Rect Tileset::GetSrcRect(int col, int row)
{
	SDL_Rect rect;
	rect.x = row * mHorShift;
	rect.y = col * mVerShift;
	rect.w = mTileWidth;
	rect.h = mTileHeight;

	return rect;
}