#include "Chunk.h"
#include <algorithm>

namespace ham
{
	Chunk::Chunk()
		:mData(nullptr)
	{

	}

	Chunk::~Chunk()
	{
		ASSERT(mData = nullptr);
	}

	bool Chunk::Initialize(int xIdx, int yIdx)
	{
		mXIdx = xIdx;
		mYIdx = yIdx;
		mData = Alloc<Block, WIDTH* HEIGHT>();
		ASSERT(mData != nullptr);

		// EMPTY_BLOCK의 비트패턴이 모두 0이라고 가정
		ASSERT(std::all_of(reinterpret_cast<const unsigned char*>(&EMPTY_BLOCK), reinterpret_cast<const unsigned char*>(&EMPTY_BLOCK) + sizeof(Block), [](unsigned char byte) {return byte == 0; }));
		std::memset(mData, 0, WIDTH * HEIGHT * sizeof(Block));
		return true;
	}

	void Chunk::Finalize()
	{
		Free<Block, WIDTH* HEIGHT>(mData);
		mData = nullptr;
	}

	void Chunk::Update(float dt)
	{

	}
}