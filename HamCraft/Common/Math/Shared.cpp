#include "Vec2.hpp"
#include "Vec2i.hpp"

namespace ham
{
	Vec2i::operator Vec2() const
	{
		return Vec2{ static_cast<float>(X), static_cast<float>(Y) };
	}

	Vec2::operator Vec2i() const
	{
		return Vec2i{ static_cast<int>(X), static_cast<int>(Y) };
	}

	Vec2 operator/(const Vec2i& lhs, float rhs)
	{
		return Vec2{ static_cast<float>(lhs.X) / rhs, static_cast<float>(lhs.Y) / rhs };
	}
}