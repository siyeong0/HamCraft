#pragma once
#include <SDL2/SDL.h>

#include"../Common/Common.h"

namespace ham
{
	class Event
	{
	public:
		Event();
		virtual ~Event();

		bool Initialize();
		void Update(float dt);
		void Finalize();

	protected:
		SDL_Event mEvent;
	};
}