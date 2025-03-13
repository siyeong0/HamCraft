#include "PerlinNoise.h"

#include <iostream>
#include <vector>
#include <cmath>
#include <cstdlib>
#include <ctime>

namespace ham
{

	PerlinNoise::PerlinNoise(int seed, EInterp interp, int amplitude, int frequency, int octavs)
		: mSeed(seed)
		, meInterp(interp)
		, mAmplitude(amplitude)
		, mFrequency(frequency)
		, mOctavs(octavs)
	{
		if (mSeed == 0)
		{
			std::srand(static_cast<unsigned int>(std::time(nullptr)));
			mSeed = std::rand();
		}
	}

	PerlinNoise::~PerlinNoise()
	{

	}
}