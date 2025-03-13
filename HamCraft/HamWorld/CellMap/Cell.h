#pragma once
#include "../../Common/Common.h"
#include "../SizeInfo.h"
#include "../Tile/Tile.h"

namespace ham
{
	class Cell
	{
	public:
		Tile ForeTile;
		Tile RearTile;
	public:
		Cell();
		Cell(TILE_ID_TYPE foreTileId, TILE_ID_TYPE rearTileId);

		inline bool IsForeEmpty() const;
		inline bool IsRearEmpty() const;
		inline bool IsWholeEmpty() const;
		inline bool IsForeFilled() const;
		inline bool IsRearFilled() const;
	};

	inline bool Cell::IsForeEmpty() const
	{
		return ForeTile.IsEmpty();
	}

	inline bool Cell::IsRearEmpty() const
	{
		return RearTile.IsEmpty();
	}

	inline bool Cell::IsWholeEmpty() const
	{
		return IsForeEmpty() && IsRearEmpty();
	}

	inline bool Cell::IsForeFilled() const
	{
		return !ForeTile.IsEmpty();
	}
	inline bool Cell::IsRearFilled() const
	{
		return !RearTile.IsEmpty();
	}
}