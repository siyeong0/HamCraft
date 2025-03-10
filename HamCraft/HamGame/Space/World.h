#pragma once
#include <vector>

#include "../../Common/Common.h"
#include "Chunk.h"
#include "ChunkManager.h"

namespace ham
{
	class World
	{
	public:
		World();
		~World();

		bool Initialize();
		void Finalize();

		void Update(float dt);

	private:
		ChunkManager mChunkManager;
		Vec2 mUserPos;
		static constexpr size_t DEFAULT_RESERVE_SIZE = 64;
	};
}