#include "Event.h"

namespace ham
{
	Event::Event()
		: mEvent()
	{

	}

	Event::~Event()
	{

	}

	bool Event::Initialize()
	{
		return true;
	}

	void Event::Update(float dt)
	{
		SDL_PollEvent(&mEvent);

		int mx = -1;
		int my = -1;

		switch (mEvent.type)
		{
		case SDL_QUIT:

			break;
		case SDL_KEYDOWN:
			switch (mEvent.key.keysym.sym)
			{
			case SDLK_w:
				gDbgPos.Y += static_cast<int>(100 * dt);
				break;;
			case SDLK_a:
				gDbgPos.X += static_cast<int>(-100 * dt);
				break;
			case SDLK_s:
				gDbgPos.Y += static_cast<int>(-100 * dt);
				break;
			case SDLK_d:
				gDbgPos.X += static_cast<int>(100 * dt);
				break;
			}
			break;
		case SDL_KEYUP:

			break;
		case SDL_MOUSEBUTTONDOWN:
			mx = mEvent.button.x;
			my = mEvent.button.y;
			break;
		case SDL_MOUSEBUTTONUP:
			mx = mEvent.button.x;
			my = mEvent.button.y;
			break;
		case SDL_MOUSEMOTION:
			mx = mEvent.motion.x;
			my = mEvent.motion.y;
			break;
		}

	}

	void Event::Finalize()
	{

	}
}