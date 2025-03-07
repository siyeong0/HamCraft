#include "Event.h"

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

	switch (mEvent.type)
	{
	case SDL_QUIT:

		break;
    case SDL_KEYDOWN:
        std::cout << "Key pressed: " << SDL_GetKeyName(mEvent.key.keysym.sym) << std::endl;
        break;
    case SDL_KEYUP:
        std::cout << "Key released: " << SDL_GetKeyName(mEvent.key.keysym.sym) << std::endl;
        break;
    case SDL_MOUSEBUTTONDOWN:
        std::cout << "Mouse button pressed: " << static_cast<int>(mEvent.button.button) << std::endl;
        std::cout << "Mouse position: (" << mEvent.button.x << ", " << mEvent.button.y << ")" << std::endl;
        break;
    case SDL_MOUSEBUTTONUP:
        std::cout << "Mouse button released: " << static_cast<int>(mEvent.button.button) << std::endl;
        std::cout << "Mouse position: (" << mEvent.button.x << ", " << mEvent.button.y << ")" << std::endl;
        break;
    case SDL_MOUSEMOTION:
        std::cout << "Mouse moved: (" << mEvent.motion.x << ", " << mEvent.motion.y << ")" << std::endl;
        break;
	}

}

void Event::Finalize()
{

}