#pragma once

struct Vector2Int
{
public:
	int x;
	int y;
public:
	Vector2Int():
		x(0),
		y(0)
	{}
	Vector2Int(const int xx, const int yy):
		x(xx),
		y(yy)
	{}
	void clear()
	{
		x = 0;
		y = 0;
	}
	Vector2Int operator*(const int that) const { return { x * that, y * that }; }
	Vector2Int operator+(const Vector2Int& that) const { return { x + that.x, y + that.y }; }
	Vector2Int operator-(const Vector2Int& that) const { return { x - that.x, y - that.y }; }
	Vector2Int& operator+=(const Vector2Int& that)
	{
		x += that.x;
		y += that.y;
		return *this;
	}
	Vector2Int& operator-=(const Vector2Int& that)
	{
		x -= that.x;
		y -= that.y;
		return *this;
	}
	Vector2Int operator-() const { return { -x, -y }; }
	bool operator<(const Vector2Int& other) const { return x + y < other.x + other.y; }
	bool operator==(const Vector2Int& other) const { return x == other.x && y == other.y; }
	bool operator!=(const Vector2Int& other) const { return x != other.x || y != other.y; }
};