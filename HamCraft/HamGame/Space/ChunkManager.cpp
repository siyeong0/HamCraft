#include "ChunkManager.h"

// ASSERT 잘 넣기.
// 메모리 할당 해제 할 땐 Alloc, Free 함수 사용하기.
// Alloc(size) : malloc이랑 똑같음
// Alloc<TYPE>(생성자 매개변수) : new 랑 똑같음
// Alloc <TYPE, size>(생성자 매개변수) : new [size]랑 똑같음
// 대응되는 Free 잘 사용할것. 모르겠으면 Commom/Memory.h ㄱㄱ

// 그래프 노드
namespace ham
{
	struct ChunkManager::Node
	{
		Chunk* Data;
		Node* Childs[4];
	};

	// 청크 탐색할 때 위 아래 양옆 방향을 나타냄
	enum class ChunkManager::EDirection
	{
		// NSWE?
	};

	// 생성자. 초기화 잘할것
	ChunkManager::ChunkManager()
		: mCurrRoot(nullptr)
		, mManageDistance(0)
		, mCapacity(-1)
		, mSize(-1)
	{

	}

	// 파괴자. 메모리 누수 없는지 확인
	ChunkManager::~ChunkManager()
	{

	}

	// 현재 루트에서 eDir 방향으로 루트를 옮김
	void ChunkManager::UpdateRoot(EDirection eDir)
	{

	}

	// 거리 내에 있는 청크를 벡터로 반환
	// reserve 잊지 말것
	// RVO위해 return은 chunkArr로만
	std::vector<Chunk*> ChunkManager::GetChunksInCircleDist(float dist)
	{
		std::vector<Chunk*> chunkArr;

		return chunkArr;
	}

	std::vector<Chunk*> ChunkManager::GetChunksInRectDist(float distX, float distY)
	{
		std::vector<Chunk*> chunkArr;

		return chunkArr;
	}

	void ChunkManager::Reserve(size_t size)
	{

	}

	// 거리 밖의 청크 삭제
	void ChunkManager::freeChunksOutDist()
	{

	}
}