#pragma once
#include "../../Common/Common.h"

namespace ham
{
	class MapGenerator
	{
	public:
		MapGenerator();
		~MapGenerator();

		MapGenerator(const MapGenerator& rhs) = delete;
		MapGenerator(const MapGenerator&& rhs) = delete;

		void GenerateHeight();

	};
}