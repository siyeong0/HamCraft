#include "Cell.h"

namespace ham
{
	Cell::Cell() 
		: ForeTile(0) 
		, RearTile(0)
	{

	}

	Cell::Cell(TILE_ID_TYPE foreTileId, TILE_ID_TYPE rearTileId)
		: ForeTile(foreTileId)
		, RearTile(rearTileId)
	{

	}
}