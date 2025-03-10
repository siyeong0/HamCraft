#include "TexManager.h"

namespace ham
{
	TexManager::TexManager()
		: mTargetRenderer(nullptr)
	{

	}

	TexManager::~TexManager()
	{
		ASSERT(mTargetRenderer == nullptr);
	}

	bool TexManager::Initialize(SDL_Renderer* targetRenderer)
	{
		ASSERT(targetRenderer != nullptr);
		mTargetRenderer = targetRenderer;

		ASSERT(mTextureTable.size() == 0);
		mTextureTable.reserve(DEFAULT_RESERVE_SIZE);

		return true;
	}

	void TexManager::Finalize()
	{
		for (auto& texElem : mTextureTable)
			SDL_DestroyTexture(texElem.second);
		mTextureTable.clear();

		mTargetRenderer = nullptr;
	}

	void TexManager::AddTexure(TEXTURE_ID_TYPE id, SDL_Surface* surface)
	{
		SDL_Texture* sdlTexture = SDL_CreateTextureFromSurface(mTargetRenderer, surface);
		ASSERT(sdlTexture != nullptr);

		mTextureTable.insert({ id, sdlTexture });
	}

	void TexManager::DeleteTexture(TEXTURE_ID_TYPE id)
	{
		ASSERT(mTextureTable.count(id) > 0);	// 존재여부 확인

		SDL_DestroyTexture(mTextureTable[id]);
		mTextureTable.erase(id);
	}

	SDL_Texture* TexManager::GetTexture(TEXTURE_ID_TYPE id)
	{
		ASSERT(mTextureTable.count(id) > 0);	// 존재여부 확인
		return mTextureTable[id];
	}
}