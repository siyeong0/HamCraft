#pragma once
#include "../../Common/Common.h"
#include "../../HamData/Texture.h"

namespace ham
{
	struct Block
	{
		TEXTURE_ID_TYPE TexId;
	};

	const Block EMPTY_BLOCK = { 0 };
}