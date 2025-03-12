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

		CellMap* mCellMap;

	//private:
	//	// TODO: 오브젝트 리스트로 변경
	//	CellMap* mCellMap;
	};
}