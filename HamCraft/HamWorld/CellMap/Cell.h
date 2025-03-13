#pragma once
#include "../../Common/Common.h"
#include "../../HamRenderer/Renderer.h"
#include "../../HamRenderer/TexManager.h"

namespace ham
{
	static constexpr int CELL_PX_SIZE = 16;

	class Cell
	{
	public:
		TEXTURE_ID_TYPE TexId;
	public:
		Cell();
		Cell(TEXTURE_ID_TYPE id);

		void Draw(SDL_Renderer* sdlRenderTarget, const Vec2i cellPos) const;
	};

	const Cell EMPTY_CELL = { 0 };

	inline bool operator==(const Cell& lhs, const Cell& rhs)
	{
		return lhs.TexId == rhs.TexId;
	}

	inline bool operator!=(const Cell& lhs, const Cell& rhs)
	{
		return lhs.TexId != rhs.TexId;
	}
}