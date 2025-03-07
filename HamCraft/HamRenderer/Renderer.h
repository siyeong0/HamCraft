#pragma once
#include <SDL2//SDL.h>

#include "../Common/Common.h"
#include "TexManager.h"
#include "RenderOption.h"

class Renderer
{
public:
	Renderer();
	virtual ~Renderer();

	bool Initialize();
	void Update(float dt);
	void Render();
	void Finalize();

	void DrawTileMap(int tileMap[][16]);

protected:
	virtual bool initSDL();

protected:
	SDL_Window* mWindow;
	SDL_Renderer* mRenderer;

	TexManager mTexManager;

	RenderOption mRenderOption;
};