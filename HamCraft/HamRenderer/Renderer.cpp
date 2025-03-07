#include "Renderer.h"

#include "../HamData/Tileset.h"

Renderer::Renderer()
	: mWindow(nullptr)
	, mRenderer(nullptr)
	, mTexManager()
{

}

Renderer::~Renderer()
{

}

bool Renderer::Initialize()
{
	if (!initSDL())
	{
		std::cout << "SDL Initialization Fail";
		return false;
	}

	mRenderer = SDL_CreateRenderer(mWindow, -1, SDL_RENDERER_ACCELERATED);
	
	if (!mTexManager.Initialize(&mRenderer))
	{
		std::cout << "TextureManager Initialization Fail";
		return false;
	}

	return true;
}

void Renderer::Update(float dt)
{

}

void Renderer::Render()
{
	SDL_RenderPresent(mRenderer);
}

void Renderer::Finalize()
{
	SDL_DestroyRenderer(mRenderer);
	mRenderer = nullptr;
	SDL_DestroyWindow(mWindow);
	mWindow = nullptr;
	SDL_Quit();
}

void Renderer::DrawTileMap(int tileMap[][16])
{
	int screenWidth = mRenderOption.Resolution.Width;
	int screenHeight = mRenderOption.Resolution.Height;
	int centerX = int(screenWidth / 2);
	int centerY = int(screenHeight / 2);
	int tileSize = int(screenHeight / 16);

	float tileMapPivX = -8;
	float tileMapPivY = -8;

	for (int y = 0; y < 16; y++)
	{
		for (int x = 0; x < 16; x++)
		{
			SDL_Texture* tex;
			SDL_Rect srcRect;
			int idx = tileMap[y][x];
			if (idx > 0)
			{
				mTexManager.GetTileTex(&tex, &srcRect, "terr_1", int((idx - 1) / 16), idx - 1);

				SDL_Rect dstRect = { centerX + tileMapPivX * tileSize + x * tileSize, centerY + tileMapPivY * tileSize + y * tileSize, tileSize, tileSize };
				SDL_RenderCopy(mRenderer, tex, &srcRect, &dstRect);
			}
		}
	}
}

// Protected
bool Renderer::initSDL()
{
	if (SDL_Init(SDL_INIT_VIDEO) < 0) 
	{
		std::cout << "SDL Initialization Fail:\n" << SDL_GetError();
		return false;
	}

	mWindow = SDL_CreateWindow("SDL2 Window",
		SDL_WINDOWPOS_UNDEFINED,
		SDL_WINDOWPOS_UNDEFINED,
		static_cast<int>(mRenderOption.Resolution.Width), static_cast<int>(mRenderOption.Resolution.Height),
		SDL_WINDOW_SHOWN);
	if (!mWindow) 
	{
		std::cout << "SDL Initialization Fail:\n" << SDL_GetError();
		SDL_Quit();
		return false;
	}

	return true;
}