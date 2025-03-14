#pragma once
#include <set>

#include "../Common/Common.h"
#include "Entity.h"

namespace ham
{
	class System
	{
	public:
		std::set<Entity> mEntities;
	};
}