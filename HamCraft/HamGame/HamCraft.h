#pragma once
#include "../Common/Common.h"
#include "../HamRenderer/Renderer.h"
#include "../HamEvent/Event.h"

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