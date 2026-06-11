#pragma once

#include "FrameDefine.h"

struct Vector2
{
public:
	float x;
	float y;
	static Vector2 ZERO;
public:
	Vector2() :
		x(0.0f),
		y(0.0f)
	{}
	Vector2(const float xx, const float yy) :
		x(xx),
		y(yy)
	{}
	void clear()
	{
		x = 0.0f;
		y = 0.0f;
	}
	Vector2 operator*(const float that) const { return { x * that, y * that }; }
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
	bool operator<(const Vector2& other) const { return x + y < other.x + other.y; }
	bool operator==(const Vector2& other) const { return x == other.x && y == other.y; }
	bool operator!=(const Vector2& other) const { return x != other.x || y != other.y; }
};