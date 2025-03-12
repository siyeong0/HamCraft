#pragma once
#include "../Common/Common.h"

namespace ham
{
	class Camera
	{
	public:
		Camera();
		~Camera();

		Camera(const Camera& rhs) = delete;
		Camera(const Camera&& rhs) = delete;

		bool Initialize(const Vec2i& pos);
		void Finalize();

		void Update(float dt);

		inline const Vec2i& GetPos() const;

	private:
		Vec2i mPos;
	};

	inline const Vec2i& Camera::GetPos() const
	{
		return mPos;
	}
}