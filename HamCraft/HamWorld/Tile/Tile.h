#pragma once
#include <cstdint>
#include "SDL2/SDL.h"

#include "../../Common/Common.h"

namespace ham
{
	class Tile
	{
	public:
		static constexpr int NUM_TILE_TEX_ROW = 16; // Tile Texture 한 바리에이션은 총 16
	public:
		Tile();
		Tile(TILE_ID_TYPE id);

		inline TILE_ID_TYPE GetId() const;
		inline void SetId(TILE_ID_TYPE id);

		inline bool IsEmpty() const;

		inline static TEXTURE_ID_TYPE CvtIdTile2Tex(TILE_ID_TYPE id);
		inline static Vec2i ExtractTexOffset(TILE_FLAG_TYPE flag);

	private:
		TILE_ID_TYPE mTileId;
	};

	inline TILE_ID_TYPE Tile::GetId() const
	{
		return mTileId;
	}

	inline void Tile::SetId(TILE_ID_TYPE id)
	{
		mTileId = id;
	}

	inline bool Tile::IsEmpty() const
	{ 
		return mTileId == 0; 
	}

	inline TEXTURE_ID_TYPE Tile::CvtIdTile2Tex(TILE_ID_TYPE id)
	{
		return id;
	}

	inline Vec2i Tile::ExtractTexOffset(TILE_FLAG_TYPE flag)
	{
		TILE_FLAG_TYPE variationFlag = (flag & 0xF0) >> 4;
		TILE_FLAG_TYPE fillInfoFlag = flag & 0x0F;
		return Vec2i{ static_cast<int>(fillInfoFlag), static_cast<int>(variationFlag) };
	}
}