#pragma once
#include <iostream>
#include <unordered_map>
#include <SDL2/SDL.h>

#include"../Common/Common.h"

namespace ham
{
	using TEXTURE_ID_TYPE = int;

	class TexManager;
	extern "C" TexManager* GetTexManager();

	class TexManager
	{
	public:
		TexManager();
		~TexManager();

		bool Initialize(SDL_Renderer* targetRenderer);
		void Finalize();

		bool LoadTexture(TEXTURE_ID_TYPE id, const std::string& path, bool bLoadDevice = false);
		void FreeTexture(TEXTURE_ID_TYPE id);
		
		bool LoadDevice(TEXTURE_ID_TYPE id);
		void FreeDevice(TEXTURE_ID_TYPE id);

		inline bool Exists(TEXTURE_ID_TYPE id) const;

		inline SDL_Surface* GetSurface(TEXTURE_ID_TYPE id);
		inline SDL_Texture* GetTexture(TEXTURE_ID_TYPE id);

	private:
		SDL_Renderer* mTargetRenderer;
		std::unordered_map<TEXTURE_ID_TYPE, std::pair<SDL_Surface*, SDL_Texture*>> mTextureTable;

		static constexpr size_t DEFAULT_RESERVE_SIZE = 1024;
	};

	inline bool TexManager::Exists(TEXTURE_ID_TYPE id) const
	{
		return mTextureTable.count(id) > 0;
	}

	inline SDL_Surface* TexManager::GetSurface(TEXTURE_ID_TYPE id)
	{
		ASSERT(Exists(id));	// 존재여부 확인

		return mTextureTable[id].first;
	}

	inline SDL_Texture* TexManager::GetTexture(TEXTURE_ID_TYPE id)
	{
		ASSERT(Exists(id));	// 존재여부 확인
		
		if (mTextureTable[id].second == nullptr)
			LoadDevice(id);

		return mTextureTable[id].second;
	}
}