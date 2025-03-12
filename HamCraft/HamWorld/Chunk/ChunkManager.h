#pragma once
#include <vector>

#include "../../Common/Common.h"
#include "../../Container/FixedQueue.hpp"
#include "Chunk.h"

namespace ham
{
	// 피봇 위치(유저 위치)와 일정 거리 안에 있는 청크를 관리
	class ChunkManager
	{
	public:
		static constexpr Vec2i MANAGE_CHUNK_DEPTH = { 32, 16 };
		static constexpr int WIDTH = 1 + 2 * MANAGE_CHUNK_DEPTH.X; // 루트와 직교하는 청크 1, 양옆/위아래로 Depth만큼
		static constexpr int HEIGHT = 1 + 2 * MANAGE_CHUNK_DEPTH.Y;
		static constexpr int NUM_CHUNKS = WIDTH * HEIGHT;
		static constexpr Vec2i CHUNK_PX_SIZE = { CellMap::PX_SIZE.X, CellMap::PX_SIZE.Y };
	private:

	public:
		ChunkManager();
		~ChunkManager();

		ChunkManager(ChunkManager& rhs) = delete;
		ChunkManager(ChunkManager&& rhs) = delete;

		bool Initialize(const Vec2i& initPos);
		void Finalize();

		void Update(float dt);
		
		// TODO: std::vector 대신 Custom Vector 사용
		std::vector<std::pair<Chunk*, Vec2i>> GetIntersectChunks(const Rect& rt) const;

	private:
		inline Vec2i calcChunkOffset(const Vec2i& pos) const;
		inline Rect calcChunkRect(const Vec2i& offset) const;
	private:
		Chunk** mChunkMap;
		int* mIdxTable;	// 노드 인덱스 테이블
		// Update 함수에서 가장자리 노드를 추가/삭제할 때 코너에서 탐색을 시작해 최적화
		Vec2i mCurrUserPos;
	};

	// private
	inline Vec2i ChunkManager::calcChunkOffset(const Vec2i& pos) const
	{
		Vec2i halfSize = CHUNK_PX_SIZE / 2;
		static_assert(CHUNK_PX_SIZE.X % 2 == 0 && CHUNK_PX_SIZE.Y % 2 == 0, "CHUNK_SIZE must be even number");
		return Vec2i{
			(pos.X / halfSize.X + 1) / 2,
			(pos.Y / halfSize.Y + 1) / 2
		};
	}

	inline Rect ChunkManager::calcChunkRect(const Vec2i& offset) const
	{
		Vec2i halfSize = CHUNK_PX_SIZE / 2;
		Vec2i start = -halfSize + offset * CHUNK_PX_SIZE;
		return Rect{ start.X, start.Y, CHUNK_PX_SIZE.X, CHUNK_PX_SIZE.Y };
	}
}