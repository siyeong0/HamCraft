#pragma once
#include "../Common/Common.h"
#include "../HamRenderer/Renderer.h"
#include "../HamEvent/Event.h"

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
	};
}