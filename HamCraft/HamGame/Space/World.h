#pragma once
#include <vector>

#include "../../Common/Common.h"
#include "Chunk.h"

class World
{
public:
	World();
	~World();

	bool Initialize();
	void Finalize();

	void Update(float dt);

private:
	std::vector<Chunk*> mChunkArr;

	static constexpr size_t DEFAULT_RESERVE_SIZE = 64;
};