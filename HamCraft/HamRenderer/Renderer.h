#pragma once
#include <SDL2//SDL.h>
#include <numeric>

#include "../Common/Common.h"
#include "Camera.h"
#include "TexManager.h"

namespace ham
{
	class Renderer;
	extern "C" Renderer* GetRenderer();
	extern "C" SDL_Renderer* GetSDLRenderer();

	class Renderer
	{
	public:
		static constexpr Vec2i DEFAULT_RESOLUTION = { 640, 360 };
	public:
		Renderer();
		virtual ~Renderer();

		bool Initialize();
		void Finalize();

		void Update(float dt);
		void Render();

		inline SDL_Renderer* GetSDLRenderer() const;
		inline Vec2i GetAspectRatio() const;
		inline Vec2i GetRTSize() const;	// RT: Render Target
		inline Rect GetRTPos() const;
		inline Vec2i GetCameraPos() const;

	protected:
		virtual bool initSDL();

	protected:
		SDL_Window* mWindow;
		SDL_Renderer* mSDLRenderer;
		Camera mCamera;
		Vec2i mResolution;
	};

	inline SDL_Renderer* Renderer::GetSDLRenderer() const
	{
		ASSERT(mSDLRenderer != nullptr);
		return mSDLRenderer;
	}

	inline Vec2i Renderer::GetAspectRatio() const
	{
		int gcd = std::gcd(mResolution.X, mResolution.Y);
		ASSERT(gcd > 1);
		return mResolution / gcd;
	}

	inline Vec2i Renderer::GetRTSize() const
	{
		// TODO: Zoom Factor Ãß°¡
		return DEFAULT_RESOLUTION;
	}

	inline Rect Renderer::GetRTPos() const
	{
		Vec2i cameraPos = mCamera.GetPos();
		Vec2i rtSize = GetRTSize();
		ASSERT(rtSize.X % 2 == 0 && rtSize.Y % 2 == 0);
		Vec2i start = cameraPos - rtSize / 2;
		return Rect{ start.X, start.Y, rtSize.X, rtSize.Y };
	}

	inline Vec2i Renderer::GetCameraPos() const
	{
		return mCamera.GetPos();
	}
}