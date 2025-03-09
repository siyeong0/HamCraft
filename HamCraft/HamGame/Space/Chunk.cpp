#include "Chunk.h"

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
	mData = Alloc<Cell, WIDTH* HEIGHT>();
	
	ASSERT(mData != nullptr);
	return true;
}

void Chunk::Finalize()
{
	Free<Cell, WIDTH* HEIGHT>(mData);
	mData = nullptr;
}

void Chunk::Update(float dt)
{

}