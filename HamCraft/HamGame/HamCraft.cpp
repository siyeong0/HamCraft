#include "HamCraft.h"

namespace ham
{
	int gWorldMap[16][16] =
	{
		{0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
		{0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
		{0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
		{0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
		{0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
		{0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
		{0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
		{0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
		{0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
		{0,0,4,4,0,0,0,0,0,0,0,0,0,11,13,3},
		{1,1,34,1,1,1,3,1,1,1,3,1,1,1,1,1},
		{1,1,1,1,1,1,1,1,16,1,1,1,1,1,1,1},
		{1,1,1,1,2,1,1,1,1,1,1,1,1,12,1,1},
		{1,1,1,1,33,1,1,1,1,1,2,1,1,1,1,1},
		{1,1,34,1,1,1,3,1,1,1,3,1,1,1,1,1},
		{1,1,1,1,1,1,1,1,16,1,1,1,1,1,1,1},
	};

	HamCraft::HamCraft()
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

		return true;
	}

	void HamCraft::Run()
	{
		bool quit = false;
		while (!quit)
		{
			mRenderer.Update(0.16f);
			mRenderer.DrawTileMap(gWorldMap);
			mRenderer.Render();

			mEvent.Update(0.16f);
		}
	}

	void HamCraft::Finalize()
	{
		mRenderer.Finalize();
		mEvent.Finalize();
	}
}