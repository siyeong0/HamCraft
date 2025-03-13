#include "Tile.h"
#include "../SizeInfo.h"
#include "../../HamRenderer/Renderer.h"

namespace ham
{
	Tile::Tile()
		: mTileId(0)	// Empty
	{

	}

	Tile::Tile(TILE_ID_TYPE id)
		: mTileId(id)
	{

	}
}