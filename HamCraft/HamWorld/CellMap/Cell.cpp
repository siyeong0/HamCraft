#include "Cell.h"

namespace ham
{
	Cell::Cell() 
		: TexId(0) 
	{

	}

	Cell::Cell(TEXTURE_ID_TYPE id) 
		: TexId(id) 
	{

	}

	void Cell::Draw(SDL_Renderer* sdlRenderTarget, const Vec2i cellPos) const
	{
		SDL_Texture* tex = GetTexManager()->GetTexture(TexId);
		ASSERT(tex != nullptr);

		const Rect rtRect = GetRenderer()->GetRTPos();
		const Vec2i camPos = GetRenderer()->GetCameraPos();
		const Rect cellRect = { cellPos.X, cellPos.Y, CELL_PX_SIZE, CELL_PX_SIZE };

		Rect intersectRect = rtRect.Intersect(cellRect);
		if (!intersectRect.IsValid())
			return;

		Rect dstRect =
		{
			intersectRect.X - rtRect.X,
			intersectRect.Y - rtRect.Y,
			intersectRect.W, intersectRect.H
		};
		// To Screen Space
		const Vec2i SCREEN_SIZE = GetRenderer()->GetRTSize();
		SDL_Rect sdlDstRect =
		{
			dstRect.X,
			SCREEN_SIZE.Y - dstRect.Y - dstRect.H,
			dstRect.W, dstRect.H
		};

		bool bSrcRectXFlag = intersectRect.X == cellPos.X;
		bool bSrcRectYFlag = intersectRect.Y == cellPos.Y;

		SDL_Rect sdlSrcRect =
		{
			bSrcRectXFlag ? 0 : CELL_PX_SIZE - intersectRect.W,
			bSrcRectYFlag ? CELL_PX_SIZE - intersectRect.H : 0,	// Screen Space y flip
			intersectRect.W, intersectRect.H
		};

		SDL_RenderCopy(sdlRenderTarget, tex, &sdlSrcRect, &sdlDstRect);
	}
}