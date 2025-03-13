#pragma once
#include <numeric>
#include <cmath>
#include <random>
#include "Numbers.h"

namespace ham
{
	class PerlinNoise
	{
	public:
		enum class EInterp
		{
			Linear,
			Cosine,
			Cubic
		};
	public:
		PerlinNoise(
			int seed = 0, 
			EInterp interp = EInterp::Cosine, 
			int amplitude = 1, 
			int frequency = 1, 
			int octavs = 1);
		~PerlinNoise();

		inline float Get(float x);

	private:
		inline float noise(float x);
		inline float interpolatedNoise(float x);

		inline float linearInterp(float a, float b, float x);
		inline float cosineInterp(float a, float b, float x);
		inline float cubicInterp(float a, float b, float c, float d, float x);

	private:
		int mSeed;
		EInterp meInterp;
		int mAmplitude;
		int mFrequency;
		int mOctavs;
	};

	float PerlinNoise::Get(float x)
	{
		float r = 0.f;
		float freq = static_cast<float>(mFrequency);
		float ampl = static_cast<float>(mAmplitude);
		for (int o = 0; o < mOctavs; ++o)
		{
			r += interpolatedNoise(x * freq) * ampl;
			freq *= 2.f;
			ampl /= 2.f;

		}

		return r;
	}

	inline float PerlinNoise::noise(float x)
	{
		static std::normal_distribution<float> sDist(-1.f, 1.f);
		std::seed_seq seed{*reinterpret_cast<unsigned int*>(&x)};
		std::mt19937 gen(seed);
		return sDist(gen);
	}

	inline float PerlinNoise::interpolatedNoise(float x)
	{
		float r = 0.f;

		int px = static_cast<int>(std::floor(x));
		int nx = px + 1;
		float fx = x - static_cast<float>(px);

		switch (meInterp)
		{
		case EInterp::Linear:
			r = linearInterp(
				noise(static_cast<float>(px)), 
				noise(static_cast<float>(nx)),
				noise(fx));
			break;
		case EInterp::Cosine:
			r = cosineInterp(
				noise(static_cast<float>(px)),
				noise(static_cast<float>(nx)),
				noise(fx));
			break;
		case EInterp::Cubic:
			r = cubicInterp(
				noise(static_cast<float>(px - 1)),
				noise(static_cast<float>(px)),
				noise(static_cast<float>(nx)),
				noise(static_cast<float>(nx + 1)),
				noise(fx)
			);
			break;
		}

		return r;
	}

	inline float PerlinNoise::linearInterp(float a, float b, float x)
	{
		return a + x * (b - a);
	}

	inline float PerlinNoise::cosineInterp(float a, float b, float x)
	{
		float x2 = (1.f - std::cosf(x * static_cast<float>(PI))) / 2.f;
		return a * (1.f - x2) + b * x2;
	}

	inline float PerlinNoise::cubicInterp(float a, float b, float c, float d, float x)
	{
		float p = (d - c) - (a - b);
		float q = (a - b) - p;
		float r = c - a;
		float s = b;
		return p * std::powf(x, 3.f) + q * std::powf(x, 2.f) + r * x + s;
	}
}