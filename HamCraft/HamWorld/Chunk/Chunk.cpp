#include "Chunk.h"

namespace ham
{
	Chunk::Chunk()
		: mCellMap()
		, mPerlinNoise(103, PerlinNoise::EInterp::Cosine, 1, 1, 2)
	{

	}

	Chunk::~Chunk()
	{
	}

	bool Chunk::Initialize()
	{
		return mCellMap.Initilize();
	}

	void Chunk::Finalize()
	{
		mCellMap.Finalize();
	}

	void Chunk::Update(float dt)
	{

	}

	void Chunk::Load(const Vec2i& idx)
	{
		// TODO: 파일에서 로드
		// 디버그용 초기화
		const float HEIGHT_SCALE = 1.5f;

		for (int x = 0; x < CellMap::WIDTH; ++x)
		{
			int currX = idx.X * CellMap::WIDTH - CellMap::WIDTH / 2 + x;

			float pn = mPerlinNoise.Get(static_cast<float>(currX));
			int height = static_cast<int>(pn * (pn >= 0 ? HEIGHT_SCALE : 1));
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
					mCellMap[Vec2i{ x,y }] = Cell(1, 0);
				}
			}
			else
			{
				for (int y = 0; y < height - yStart; ++y)
				{
					mCellMap[Vec2i{ x,y }] = Cell(1, 0);
				}
			}
		}
	}
}