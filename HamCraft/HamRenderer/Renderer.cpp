#include "Renderer.h"

#include "../HamData/Tileset.h"

namespace ham
{
	Renderer* sRenderer = nullptr;

	Renderer* GetRenderer()
	{
		return sRenderer;
	}

	SDL_Renderer* GetSDLRenderer()
	{
		return sRenderer->GetSDLRenderer();
	}

	Renderer::Renderer()
		: mWindow(nullptr)
		, mSDLRenderer(nullptr)
		, mCamera()
		, mResolution(DEFAULT_RESOLUTION)
	{
		ASSERT(sRenderer == nullptr);
		sRenderer = this;
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

		mSDLRenderer = SDL_CreateRenderer(mWindow, -1, SDL_RENDERER_ACCELERATED);

		// TODO: Camera 위치 초기화
		if (!mCamera.Initialize(Vec2i{ 0,0 }))
		{
			std::cout << "Camera Initialization Fail";
			return false;
		}

		return true;
	}

	void Renderer::Finalize()
	{
		SDL_DestroyRenderer(mSDLRenderer);
		mSDLRenderer = nullptr;
		SDL_DestroyWindow(mWindow);
		mWindow = nullptr;
		SDL_Quit();
	}

	void Renderer::Update(float dt)
	{
		mCamera.Update(dt);
	}

	void Renderer::Render()
	{
		SDL_RenderPresent(mSDLRenderer);
		SDL_RenderClear(mSDLRenderer);
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
			GetRTSize().X, GetRTSize().Y,
			SDL_WINDOW_SHOWN);
		if (!mWindow)
		{
			std::cout << "SDL Initialization Fail:\n" << SDL_GetError();
			SDL_Quit();
			return false;
		}

		return true;
	}
}