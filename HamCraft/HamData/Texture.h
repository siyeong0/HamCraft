#pragma once
#include <iostream>
#include <SDL2/SDL.h>

#include "../Common/Common.h"

namespace ham
{
	using TEXTURE_ID_TYPE = int;

	class Texture
	{
	public:
		Texture();
		~Texture();

		bool Load(const char* path);
		bool Load(const std::string& path);
		void Free();

		SDL_Surface* GetSDLSurface() { ASSERT(mSDLSurface != nullptr);  return mSDLSurface; };

	private:
		SDL_Surface* mSDLSurface;
	};

}