#include "HamCraft.h"

namespace ham
{
	HamCraft::HamCraft()
		: mRenderer()
		, mEvent()
		, mChunkManager()
	{

	}

	HamCraft::~HamCraft()
	{

	}

	bool HamCraft::Initialize()
	{
		if (!mRenderer.Initialize())
			return false;

		if (!mEvent.Initialize())
			return false;

		if (!mChunkManager.Initialize(Vec2i{ 0,0 }))
			return false;

		if (!mTexManager.Initialize(GetSDLRenderer()))
			return false;

		mTexManager.LoadTexture(1, "C:\\Dev\\HamCraft\\HamCraft\\Resource\\Image\\Tile\\dirt.png", true);
		return true;
	}

	void HamCraft::Run()
	{
		bool quit = false;
		while (!quit)
		{
			mRenderer.Update(0.16f);
			mEvent.Update(0.16f);

			const Rect rtRect = mRenderer.GetRTPos();
			std::vector<std::pair<Chunk*, Vec2i>> targetChunks = mChunkManager.GetIntersectChunks(rtRect);
			for (auto& chunk : targetChunks)
			{
				Chunk* pChunk = chunk.first;
				Vec2i pos = chunk.second;
				pChunk->mCellMap->Draw(GetSDLRenderer(), pos);
			}
			
			mRenderer.Render();
		}
	}

	void HamCraft::Finalize()
	{
		mRenderer.Finalize();
		mEvent.Finalize();
	}
}