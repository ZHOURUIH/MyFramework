#pragma once

struct Vector2Short
{
public:
	short x;
	short y;
public:
	Vector2Short():
		x(0),
		y(0)
	{}
	Vector2Short(const short xx, const short yy):
		x(xx),
		y(yy)
	{}
	void clear()
	{
		x = 0;
		y = 0;
	}
	Vector2Short operator*(const short that) const { return { (short)(x * that), (short)(y * that) }; }
	Vector2Short operator+(const Vector2Short& that) const { return { (short)(x + that.x), (short)(y + that.y) }; }
	Vector2Short operator-(const Vector2Short& that) const { return { (short)(x - that.x), (short)(y - that.y) }; }
	Vector2Short& operator+=(const Vector2Short& that)
	{
		x += that.x;
		y += that.y;
		return *this;
	}
	Vector2Short& operator-=(const Vector2Short& that)
	{
		x -= that.x;
		y -= that.y;
		return *this;
	}
	Vector2Short operator-()const { return { (short)-x, (short)-y}; }
	bool operator<(const Vector2Short& other) const { return x + y < other.x + other.y; }
	bool operator==(const Vector2Short& other) const { return x == other.x && y == other.y; }
	bool operator!=(const Vector2Short& other) const { return x != other.x || y != other.y; }
};