#pragma once

#include "../../Common/Common.h"
#include "../../HamObject/IGameObject.h"
#include "../CellMap/CellMap.h"

namespace ham
{
	class Chunk
	{
	public:
		Chunk();
		~Chunk();

		bool Initialize();
		void Finalize();

		void Update(float dt);

		void Load(const Vec2i& idx);

		inline Cell& Map(const Vec2i& idx);

	private:
		CellMap mCellMap;
		PerlinNoise mPerlinNoise;
	};

	inline Cell& Chunk::Map(const Vec2i& idx)
	{
		return mCellMap[idx];
	}
}