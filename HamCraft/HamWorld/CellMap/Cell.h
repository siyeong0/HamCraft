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
		Cell() : TexId(0) {}
		Cell(TEXTURE_ID_TYPE id) : TexId(id) {}

		inline void Draw(SDL_Renderer* sdlRenderTarget, const Vec2i cellPos) const;
	};

	inline void Cell::Draw(SDL_Renderer* sdlRenderTarget, const Vec2i cellPos) const
	{
		SDL_Texture* tex = GetTexManager()->GetTexture(TexId);
		ASSERT(tex != nullptr);

		const Rect rtRect = GetRenderer()->GetRTPos();
		const Vec2i camPos = GetRenderer()->GetCameraPos();
		const Rect cellRect = { cellPos.X, cellPos.Y, CELL_PX_SIZE, CELL_PX_SIZE };

		Rect intersectRect = rtRect.Intersect(cellRect);
		if (!intersectRect.IsValid())
			return;

		SDL_Rect dstRect =
		{
			intersectRect.X - rtRect.X,
			intersectRect.Y - rtRect.Y,
			intersectRect.W, intersectRect.H
		};

		bool bSrcRectXFlag = intersectRect.X == cellPos.X;
		bool bSrcRectYFlag = intersectRect.Y == cellPos.Y;

		SDL_Rect srcRect =
		{
			bSrcRectXFlag ? 0 : CELL_PX_SIZE - intersectRect.W,
			bSrcRectYFlag ? 0 : CELL_PX_SIZE - intersectRect.H,
			intersectRect.W, intersectRect.H
		};
		SDL_RenderCopy(sdlRenderTarget, tex, &srcRect, &dstRect);
	}

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