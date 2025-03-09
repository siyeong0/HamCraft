#include "Texture.h"
#include "SDL2/SDL_image.h"

Texture::Texture()
	:mSDLSurface(nullptr)
{

}

Texture::~Texture()
{
	ASSERT(mSDLSurface == nullptr);
}

bool Texture::Load(const char* path)
{
	mSDLSurface = IMG_Load(path);
	if (!mSDLSurface)
		return false;

	return true;
}

bool Texture::Load(const std::string& path)
{
	return this->Load(path.c_str());
}

void Texture::Free()
{
	SDL_FreeSurface(mSDLSurface);
	mSDLSurface = nullptr;
}