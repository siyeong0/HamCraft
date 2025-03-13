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

		Clear();
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
}