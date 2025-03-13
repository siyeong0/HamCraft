#include "Camera.h"

namespace ham
{
	Camera::Camera()
		: mPos(Vec2i{0,0})
	{

	}

	Camera::~Camera()
	{

	}

	bool Camera::Initialize(const Vec2i& pos)
	{
		mPos = pos;

		return true;
	}

	void Camera::Finalize()
	{

	}

	void Camera::Update(float dt)
	{
		// TODO: 디버그를 위한 임시코드. 이벤트 형식으로 변경필요
		mPos = gDbgPos;
	}
}