#include "CellMap.h"
#include "../../HamRenderer/Renderer.h"

namespace ham
{
	CellMap::CellMap()
		: mCells(nullptr)
	{

	}

	CellMap::~CellMap()
	{

	}

	bool CellMap::Initilize()
	{
		mCells = Alloc<Cell, NUM_CELLS>();
		if (mCells == nullptr)
			return false;

		return true;
	}

	void CellMap::Finalize()
	{
		ASSERT(mCells != nullptr);
		Free<Cell, NUM_CELLS>(mCells);
		mCells = nullptr;
	}

	void CellMap::Update(float dt)
	{

	}

	void CellMap::Draw(SDL_Renderer* sdlRenderTarget, const Vec2i& startPos)
	{
		ASSERT(mCells != nullptr);
		for (int y = 0; y < HEIGHT; ++y)
		{
			for (int x = 0; x < WIDTH; ++x)
			{
				const Cell& cell = mCells[y * WIDTH + x];

				if (cell != EMPTY_CELL)
					cell.Draw(sdlRenderTarget, startPos + Vec2i{ x, y } *CELL_PX_SIZE);
			}
		}
	}
}