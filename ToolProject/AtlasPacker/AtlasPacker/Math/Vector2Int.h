#ifndef _VECTOR2_INT_H_
#define _VECTOR2_INT_H_

struct Vector2Int
{
public:
	int x;
	int y;
public:
	Vector2Int()
	{
		x = 0;
		y = 0;
	}
	Vector2Int(int xx, int yy)
	{
		x = xx;
		y = yy;
	}
	void clear()
	{
		x = 0;
		y = 0;
	}
	Vector2Int operator*(int that) const
	{
		return Vector2Int(x * that, y * that);
	}
	Vector2Int operator+(const Vector2Int& that) const
	{
		return Vector2Int(x + that.x, y + that.y);
	}
	Vector2Int operator-(const Vector2Int& that) const
	{
		return Vector2Int(x - that.x, y - that.y);
	}
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
	Vector2Int operator-() const
	{
		return Vector2Int(-x, -y);
	}
	bool operator<(const Vector2Int& other) const
	{
		return x + y < other.x + other.y;
	}
};

#endif