#pragma once

#include "../../Common/Common.h"
#include "Block.h"

namespace ham
{
	class Chunk
	{
	public:
		Chunk();
		~Chunk();

		bool Initialize(int xIdx, int yIdx);
		void Finalize();

		void Update(float dt);

		inline void AddBlock(const Block& block, size_t x, size_t y)
		{
			mData[y * WIDTH + x] = block;
		}
		inline void DeleteBlock(size_t x, size_t y)
		{
			mData[y * WIDTH + x] = EMPTY_BLOCK;
		}

		static constexpr size_t GetWidth() { return WIDTH; }
		static constexpr size_t GetHeight() { return HEIGHT; }

	private:
		int mXIdx;
		int mYIdx;
		Block* mData;

		static constexpr size_t WIDTH = 64;
		static constexpr size_t HEIGHT = 64;

	};
}