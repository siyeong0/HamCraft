#include "Chunk.h"

namespace ham
{
	Chunk::Chunk()
		: mCellMap(nullptr)
		, mPerlinNoise(103, PerlinNoise::EInterp::Cosine, 1, 1, 2)
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
		const float HEIGHT_SCALE = 1.5f;
		CellMap& cellMap = mCellMap[0];

		if (idx.X == 0 && idx.Y == 0)
		{
			int a = 3;
		}

		for (int x = 0; x < CellMap::WIDTH; ++x)
		{
			int currX = idx.X * CellMap::WIDTH - CellMap::WIDTH / 2 + x;
			if (currX == 0)
			{
				int a = 3;
			}

			float pn = mPerlinNoise.Get(static_cast<float>(currX));
			int height = pn * (pn >= 0 ? HEIGHT_SCALE : 1);
			int yStart = idx.Y * CellMap::HEIGHT - CellMap::HEIGHT / 2;
			int yEnd = yStart + CellMap::HEIGHT - 1;
			if (height < yStart)
			{
				continue;
			}
			else if (height > yEnd)
			{
				for (int y = 0; y < CellMap::HEIGHT; ++y)
				{
					cellMap[Vec2i{ x,y }] = Cell{ 1 };
				}
			}
			else
			{
				for (int y = 0; y < height - yStart; ++y)
				{
					cellMap[Vec2i{ x,y }] = Cell{ 1 };
				}
			}
		}
	}
}