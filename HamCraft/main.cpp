#include <iostream>
#include "Common/Common.h"
#include "HamGame/HamCraft.h"

#undef main

using namespace ham;

int main(void)
{	
	HamCraft game;
	game.Initialize();

	game.Run();

	return 0;
}
