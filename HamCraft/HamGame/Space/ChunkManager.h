#pragma once
#include <vector>

#include "../../Common/Common.h"
#include "Chunk.h"

// 설명은 cpp파일 확인
// 컴파일러 에러, 워닝 0개 유지
namespace ham
{
	class ChunkManager
	{
	public:
		struct Node;
		enum class EDirection;
		enum class EDistType;
	public:
		ChunkManager();
		~ChunkManager();

		ChunkManager(ChunkManager& rhs) = delete;
		ChunkManager(ChunkManager&& rhs) = delete;

		void UpdateRoot(EDirection eDir);
		std::vector<Chunk*> GetChunksInCircleDist(float dist);
		std::vector<Chunk*> GetChunksInRectDist(float distX, float distY);

		void Reserve(size_t size);

		size_t GetSize() { return mSize; }
		size_t GetCapacity() { return mCapacity; }

		// Getter&Setter

	private:
		void freeChunksOutDist();

	private:
		Node* mCurrRoot;

		float mManageDistance;
		// EDistType mManageType; 구현해도 되고 안해도됨

		size_t mCapacity;
		size_t mSize;
	};
}