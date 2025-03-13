#pragma once

namespace ham
{
	struct Vec2;

	struct Vec2i
	{
		int X;
		int Y;

		explicit operator Vec2() const;
	};

	inline Vec2i operator+(const Vec2i& lhs, const Vec2i& rhs)
	{
		return Vec2i{ lhs.X + rhs.X, lhs.Y + rhs.Y };
	}

	inline Vec2i operator-(const Vec2i& lhs, const Vec2i& rhs)
	{
		return Vec2i{ lhs.X - rhs.X, lhs.Y - rhs.Y };
	}

	inline Vec2i operator*(int v, const Vec2i& rhs)
	{
		return Vec2i{ v * rhs.X, v * rhs.Y };
	}

	inline Vec2i operator*(const Vec2i& lhs, int v)
	{
		return Vec2i{ lhs.X * v, lhs.Y * v };
	}

	inline Vec2i operator*(const Vec2i& lhs, const Vec2i& rhs)
	{
		return Vec2i{ lhs.X * rhs.X, lhs.Y * rhs.Y };
	}

	inline Vec2i operator/(const Vec2i& lhs, int rhs)
	{
		return Vec2i{ lhs.X / rhs, lhs.Y / rhs };
	}

	inline Vec2i operator%(const Vec2i& lhs, int rhs)
	{
		return Vec2i{ lhs.X % rhs, lhs.Y % rhs };
	}

	inline Vec2i operator-(const Vec2i& rhs) 
	{
		return -1 * rhs;
	}

	inline bool operator==(const Vec2i& lhs, const Vec2i& rhs)
	{
		return (lhs.X == rhs.X) && (lhs.Y == rhs.Y);
	}
}