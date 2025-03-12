#pragma once
#pragma once
#include "../Common/Common.h"
#include "../HamRenderer/Renderer.h"
#include "../HamEvent/Event.h"
#include "World/ChunkManager.h"

namespace ham
{
	class HamCraft
	{
	public:
		HamCraft();
		~HamCraft();

		bool Initialize();
		void Run();
		void Finalize();

	private:
		Renderer mRenderer;
		Event mEvent;
		ChunkManager mChunkManager;
		TexManager mTexManager;
	};
}