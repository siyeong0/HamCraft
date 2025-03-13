#pragma once
#include <SDL2\SDL.h>
#include <algorithm>

namespace ham
{
	struct Rect
	{
		int X, Y;
		int W, H;

		Rect() : X(0), Y(0), W(0), H(0) {}
		Rect(int x, int y, int w, int h) : X(x), Y(y), W(w), H(h) {}

		inline bool DoIntersect(const Rect& r2) const;
		inline Rect Intersect(const Rect& r2) const;
		inline bool IsValid() const;

		operator SDL_Rect() { return SDL_Rect{ X, Y, W, H }; }
	};

	inline Rect ToRect(const SDL_Rect& sdlRect)
	{
		return Rect(sdlRect.x, sdlRect.y, sdlRect.w, sdlRect.h);
	}

	inline bool Rect::DoIntersect(const Rect& r2) const
	{
		const Rect& r1 = *this;
		return r1.X < r2.X + r2.W && r1.X + r1.W > r2.X && r1.Y < r2.Y + r2.H && r1.Y + r1.H > r2.Y;
	}

	inline Rect Rect::Intersect(const Rect& r2) const
	{
		const Rect& r1 = *this;
		int interLeft = std::max(r1.X, r2.X);
		int interTop = std::max(r1.Y, r2.Y);
		int interRight = std::min(r1.X + r1.W, r2.X + r2.W);
		int interBottom = std::min(r1.Y + r1.H, r2.Y + r2.H);

		return Rect(interLeft, interTop, interRight - interLeft, interBottom - interTop);
	}

	inline bool Rect::IsValid() const
	{
		return W > 0 && H > 0;
	}
}