#pragma once

namespace ham
{
	struct Vec2i; 
	
	struct Vec2
	{
		float X;
		float Y;

		explicit operator Vec2i() const;
	};

	inline Vec2 operator+(const Vec2& lhs, const Vec2& rhs)
	{
		return Vec2{ lhs.X + rhs.X, lhs.Y + rhs.Y };
	}

	inline Vec2 operator-(const Vec2& lhs, const Vec2& rhs)
	{
		return Vec2{ lhs.X - rhs.X, lhs.Y - rhs.Y };
	}

	inline Vec2 operator*(float v, const Vec2& rhs)
	{
		return Vec2{ v * rhs.X, v * rhs.Y };
	}

	inline Vec2 operator*(const Vec2& lhs, float v)
	{
		return Vec2{ lhs.X * v, lhs.Y * v };
	}

	inline Vec2 operator*(const Vec2& lhs, const Vec2& rhs)
	{
		return Vec2{ lhs.X * rhs.X, lhs.Y * rhs.Y };
	}

	inline Vec2 operator/(const Vec2& lhs, float rhs)
	{
		return Vec2{ lhs.X / rhs, lhs.Y / rhs };
	}

	inline bool operator==(const Vec2& lhs, const Vec2& rhs)
	{
		return (lhs.X == rhs.X) && (lhs.Y == rhs.Y);
	}
}