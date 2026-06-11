#pragma once

#include "FrameMacro.h"

struct Vector2UInt
{
public:
	uint x = 0;
	uint y = 0;
public:
	Vector2UInt() = default;
	Vector2UInt(const uint xx, const uint yy):
		x(xx),
		y(yy)
	{}
	void clear()
	{
		x = 0;
		y = 0;
	}
	Vector2UInt operator*(const short that) const { return { (uint)(x * that), (uint)(y * that) }; }
	Vector2UInt operator+(const Vector2UInt& that) const { return { (uint)(x + that.x), (uint)(y + that.y) }; }
	Vector2UInt operator-(const Vector2UInt& that) const { return { (uint)(x - that.x), (uint)(y - that.y) }; }
	Vector2UInt& operator+=(const Vector2UInt& that)
	{
		x += that.x;
		y += that.y;
		return *this;
	}
	Vector2UInt& operator-=(const Vector2UInt& that)
	{
		x -= that.x;
		y -= that.y;
		return *this;
	}
	bool operator<(const Vector2UInt& other) const { return x + y < other.x + other.y; }
	bool operator==(const Vector2UInt& other) const { return x == other.x && y == other.y; }
	bool operator!=(const Vector2UInt& other) const { return x != other.x || y != other.y; }
};