#pragma once

struct Vector2
{
public:
	float x;
	float y;
	static Vector2 ONE;
	static Vector2 ZERO;
	static Vector2 UP;
	static Vector2 LEFT;
	static Vector2 RIGHT;
	static Vector2 DOWN;
public:
	Vector2():
		x(0.0f),
		y(0.0f)
	{}
	Vector2(const float xx, const float yy):
		x(xx),
		y(yy)
	{}
	void clear()
	{
		x = 0.0f;
		y = 0.0f;
	}
	Vector2 operator*(const float that) const { return { x * that, y * that }; }
	Vector2 operator/(const float that) const
	{
		const float inverse = 1.0f / that;
		return { x * inverse, y * inverse };
	}
	Vector2 operator+(const Vector2& that) const { return { x + that.x, y + that.y }; }
	Vector2 operator-(const Vector2& that) const { return { x - that.x, y - that.y }; }
	Vector2& operator+=(const Vector2& that)
	{
		x += that.x;
		y += that.y;
		return *this;
	}
	Vector2& operator-=(const Vector2& that)
	{
		x -= that.x;
		y -= that.y;
		return *this;
	}
	Vector2 operator-() const { return { -x, -y }; }
};