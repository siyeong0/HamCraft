#pragma once
#include <iostream>
#include <unordered_map>
#include <SDL2/SDL.h>

#include"../Common/Common.h"
#include "../HamData/Texture.h"

class TexManager
{
public:
	TexManager();
	~TexManager();

	bool Initialize(SDL_Renderer* targetRenderer);
	void Finalize();

	void AddTexure(TEXTURE_ID_TYPE id, SDL_Surface* surface);
	void DeleteTexture(TEXTURE_ID_TYPE id);

	SDL_Texture* GetTexture(TEXTURE_ID_TYPE id);

private:
	SDL_Renderer* mTargetRenderer;
	std::unordered_map<TEXTURE_ID_TYPE, SDL_Texture*> mTextureTable;

	static constexpr size_t DEFAULT_RESERVE_SIZE = 1024;
};