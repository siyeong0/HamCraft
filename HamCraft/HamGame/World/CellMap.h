#pragma once
#include "../../Common/Common.h"
#include "Cell.h"

namespace ham
{
	class CellMap
	{
	public:
		static constexpr int WIDTH = 256;
		static constexpr int HEIGHT = 256;
		static constexpr int NUM_CELLS = WIDTH * HEIGHT;
		static constexpr Vec2i PX_SIZE = { WIDTH * CELL_PX_SIZE, HEIGHT * CELL_PX_SIZE };
	public:
		CellMap();
		~CellMap();

		CellMap(const CellMap& rhs) = delete;
		CellMap(const CellMap&& rhs) = delete;

		bool Initilize();
		void Finalize();
		void Update(float dt);
		void Draw(SDL_Renderer* sdlRenderTarget, const Vec2i& startPos);

		inline const Cell& GetCell(const Vec2i& idx);
		inline Cell& operator[](const Vec2i& idx);
		inline void Clear();
	private:
		Cell* mCells;
	};

	inline const Cell& CellMap::GetCell(const Vec2i& idx)
	{ 
		ASSERT(idx.X >= 0 && idx.X < WIDTH && idx.Y >= 0 && idx.Y < HEIGHT);
		return mCells[idx.Y * WIDTH + idx.X]; 
	}

	inline Cell& CellMap::operator[](const Vec2i& idx)
	{
		ASSERT(idx.X >= 0 && idx.X < WIDTH && idx.Y >= 0 && idx.Y < HEIGHT);
		return mCells[WIDTH * idx.Y + idx.X];
	}

	inline void CellMap::Clear()
	{
		std::fill(mCells, mCells + NUM_CELLS, EMPTY_CELL);
	}
}