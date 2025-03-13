#pragma once
#include "../HamCraft/Common/Common.h"
#include "../HamCraft/HamRenderer/Renderer.h"

namespace ham
{
	class TileEditor
	{
	public:
		TileEditor();
		~TileEditor();

		bool Initialize();
		void Finalize();

		void Update(float dt);

	private:
		Renderer mRenderer;
	};
}