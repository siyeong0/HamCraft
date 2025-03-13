#pragma once
#pragma once
#include "../Common/Common.h"
#include "../HamRenderer/Renderer.h"
#include "../HamEvent/Event.h"
#include "../HamWorld/Chunk/ChunkManager.h"

namespace ham
{
	class HamCraft
	{
	public:
		HamCraft();
		~HamCraft();

		bool Initialize();
		void Finalize();
		void Run();

	private:
		void drawTiles(const std::vector<std::pair<Chunk*, Vec2i>>& targetChunks);

	private:
		Renderer mRenderer;
		Event mEvent;
		ChunkManager mChunkManager;
		TexManager mTexManager;
	};
}