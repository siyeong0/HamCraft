#pragma once

#include "../../Common/Common.h"
#include "Cell.h"

class Chunk
{
public:
	Chunk();
	~Chunk();

	bool Initialize(int xIdx, int yIdx);
	void Finalize();

	void Update(float dt);

private:
	int mXIdx;
	int mYIdx;
	Cell* mData;

	static constexpr size_t WIDTH = 64;
	static constexpr size_t HEIGHT = 64;

};