#include "ChunkManager.h"

#include "numeric"

#define CHUNK_MAP(x, y) mChunkMap[mIdxTable[y * WIDTH + x]]

namespace ham
{
	ChunkManager::ChunkManager()
		: mChunkMap(nullptr)
		, mIdxTable(nullptr)
		, mCurrUserPos(Vec2i{ 0, 0 })
	{

	}

	ChunkManager::~ChunkManager()
	{
		ASSERT(mChunkMap == nullptr);
	}

	// TODO: NodePtrArray를  heap으로 할당
	bool ChunkManager::Initialize(const Vec2i& initPos)
	{
		// 기본 멤버 초기화
		mCurrUserPos = initPos;
		mChunkMap = Alloc<Chunk*, NUM_CHUNKS>();
		mIdxTable = Alloc<int, NUM_CHUNKS>();
		ASSERT(mChunkMap != nullptr);
		ASSERT(mIdxTable != nullptr);

		// 인덱스 테이블 초기화
		std::iota(mIdxTable, mIdxTable + NUM_CHUNKS, 0);	// 0,1,2,3,4...로 초기화

		// 청크 초기화
		const Vec2i centerChunkOffset = calcChunkOffset(initPos);
		static_assert(WIDTH % 2 == 1 && HEIGHT % 2 == 1, "WIDTH / Height must be odd number.");
		const Vec2i baseOffset = centerChunkOffset - Vec2i{ WIDTH / 2, HEIGHT / 2 };
		for (int y = 0; y < HEIGHT; ++y)
		{
			for (int x = 0; x < WIDTH; ++x)
			{
				CHUNK_MAP(x, y) = Alloc<Chunk>();
				ASSERT(CHUNK_MAP(x, y) != nullptr);
				CHUNK_MAP(x, y)->Initialize();
				CHUNK_MAP(x, y)->Load(baseOffset + Vec2i{x, y});
			}
		}

		return true;
	}

	void ChunkManager::Finalize()
	{
		ASSERT(mChunkMap != nullptr);
		ASSERT(mIdxTable != nullptr);
		for (int y = 0; y < HEIGHT; ++y)
		{
			for (int x = 0; x < WIDTH; ++x)
			{
				CHUNK_MAP(x, y)->Finalize();
				Free(CHUNK_MAP(x, y));
				CHUNK_MAP(x, y) = nullptr;
			}
		}
		Free<Chunk*, NUM_CHUNKS>(mChunkMap);
		mChunkMap = nullptr;
		Free<int, NUM_CHUNKS>(mIdxTable);
		mIdxTable = nullptr;
	}

	void ChunkManager::Update(float dt)
	{
		Vec2i prevUserPos = mCurrUserPos;
		// TODO: 디버그용
		mCurrUserPos = gDbgPos;

		const Vec2i prevCenterOffset = calcChunkOffset(prevUserPos);
		const Vec2i currCenterOffset = calcChunkOffset(mCurrUserPos);
		const Vec2i diffOffset = currCenterOffset - prevCenterOffset;

		if (diffOffset.X == 0 && diffOffset.Y == 0)
			return;

		ASSERT(std::abs(diffOffset.X) < WIDTH && std::abs(diffOffset.Y) < HEIGHT);

		// 오프셋 만큼 청크 교체
		int signX = diffOffset.X > 0 ? 1 : -1;
		int signY = diffOffset.Y > 0 ? 1 : -1;
		const Vec2i baseOffset = prevCenterOffset - Vec2i{ WIDTH / 2, HEIGHT / 2 };
		for (int y = 0; y < std::abs(diffOffset.Y); ++y)
		{
			for (int x = 0; x < std::abs(diffOffset.X); ++x)
			{
				int repX = ((WIDTH + x * signX) + WIDTH) % WIDTH;	
				int repY = ((WIDTH + y * signY) + HEIGHT) % HEIGHT; // // 가장자리에서 시작해 원형 인덱싱
				// TODO: 캐시에 저장, 로드
				// 청크 해제
				CHUNK_MAP(x, y)->Finalize();
				Free<Chunk>(CHUNK_MAP(x, y));
				// 청크 할당
				CHUNK_MAP(x, y) = Alloc<Chunk>();
				CHUNK_MAP(x, y)->Initialize();
				CHUNK_MAP(x, y)->Load(baseOffset + Vec2i{ x, y });
			}
		}
		
		// 인덱스 테이블 갱신
		int idxTableBuf[NUM_CHUNKS];
		std::iota(idxTableBuf, idxTableBuf + NUM_CHUNKS, 0);	// 0,1,2,3,4...로 초기화
		for (int y = 0; y < HEIGHT; ++y)
		{
			for (int x = 0; x < WIDTH; ++x)
			{
				int movedX = ((x - diffOffset.X) + WIDTH) % WIDTH;
				int movedY = ((y - diffOffset.Y) + HEIGHT) % HEIGHT;
				idxTableBuf[movedY * WIDTH + movedX] = mIdxTable[y * WIDTH + x];
			}
		}
		std::copy(mIdxTable, mIdxTable + NUM_CHUNKS, idxTableBuf);
	}

	std::vector<std::pair<Chunk*, Vec2i>> ChunkManager::GetIntersectChunks(const Rect& rt) const
	{
		std::vector<std::pair<Chunk*, Vec2i>> outChunks;
		outChunks.reserve(64);

		const Vec2i currCenterOffset = calcChunkOffset(mCurrUserPos);
		const Vec2i baseOffset = currCenterOffset - Vec2i{ WIDTH / 2, HEIGHT / 2 };
		for (int y = 0; y < HEIGHT; ++y)
		{
			for (int x = 0; x < WIDTH; ++x)
			{
				Rect chunkRect = calcChunkRect(baseOffset + Vec2i{x,y});
				if (chunkRect.DoIntersect(rt))
				{
					outChunks.push_back(std::make_pair(
						CHUNK_MAP(x, y), 
						Vec2i{ chunkRect.X, chunkRect.Y }
					));
				}
			}
		}

		return outChunks;
	}
}