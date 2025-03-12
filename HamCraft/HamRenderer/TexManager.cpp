#include "TexManager.h"

#include "SDL2/SDL_image.h"

namespace ham
{
	TexManager* sTexManager = nullptr;

	TexManager* GetTexManager()
	{
		return sTexManager;
	}

	TexManager::TexManager()
		: mTargetRenderer(nullptr)
	{
		ASSERT(sTexManager == nullptr);
		sTexManager = this;
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
		for (auto& elem : mTextureTable)
			FreeTexture(elem.first);
		ASSERT(mTextureTable.size() == 0);

		mTextureTable.clear();
		mTargetRenderer = nullptr;
	}

	bool TexManager::LoadTexture(TEXTURE_ID_TYPE id, const std::string& path, bool bLoadDevice)
	{
		SDL_Surface* surface = IMG_Load(path.c_str());
		if (!surface)
			return false;

		mTextureTable.insert({ id, {surface, nullptr} });

		if (bLoadDevice)
			LoadDevice(id);

		return true;
	}

	void TexManager::FreeTexture(TEXTURE_ID_TYPE id)
	{
		ASSERT(mTextureTable.count(id) > 0);	// 존재여부 확인
		auto data = mTextureTable[id];
		SDL_Surface* surface = data.first;
		SDL_Texture* texture = data.second;

		SDL_FreeSurface(surface);
		if (texture != nullptr)
			SDL_DestroyTexture(texture);

		mTextureTable.erase(id);
	}

	bool TexManager::LoadDevice(TEXTURE_ID_TYPE id)
	{
		ASSERT(mTargetRenderer != nullptr);
		ASSERT(mTextureTable[id].second == nullptr);
		SDL_Texture* texture = SDL_CreateTextureFromSurface(mTargetRenderer, mTextureTable[id].first);
		if (texture == nullptr)
			return false;

		mTextureTable[id].second = texture;

		return true;
	}

	void TexManager::FreeDevice(TEXTURE_ID_TYPE id)
	{
		ASSERT(mTargetRenderer != nullptr);
		ASSERT(mTextureTable[id].second != nullptr);

		SDL_DestroyTexture(mTextureTable[id].second);
		mTextureTable[id].second = nullptr;
	}
}