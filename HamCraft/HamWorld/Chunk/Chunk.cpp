#include "Chunk.h"

#include <algorithm>

namespace ham
{
	Chunk::Chunk()
		: mCellMap(nullptr)
	{

	}

	Chunk::~Chunk()
	{
		ASSERT(mCellMap == nullptr);
	}

	bool Chunk::Initialize()
	{
		mCellMap = Alloc<CellMap>();
		ASSERT(mCellMap != nullptr);

		// EMPTY_BLOCK의 비트패턴이 모두 0이라고 가정
		ASSERT(std::all_of(reinterpret_cast<const unsigned char*>(&EMPTY_CELL), reinterpret_cast<const unsigned char*>(&EMPTY_CELL) + sizeof(Cell), [](unsigned char byte) {return byte == 0; }));

		mCellMap->Initilize();

		return true;
	}

	void Chunk::Finalize()
	{
		Free<CellMap>(mCellMap);
		mCellMap = nullptr;
	}

	void Chunk::Update(float dt)
	{

	}

	void Chunk::Load(const Vec2i& idx)
	{
		// TODO: 파일에서 로드
		mCellMap->Clear();
		// 디버그용 초기화
		for (int y = 0; y < CellMap::HEIGHT; ++y)
		{
			for (int x = 0; x < CellMap::WIDTH; ++x)
			{
				CellMap& cellMap = mCellMap[0];
				cellMap[Vec2i{x,y}] = Cell{ std::rand() % 2};
			}
		}
	}
}