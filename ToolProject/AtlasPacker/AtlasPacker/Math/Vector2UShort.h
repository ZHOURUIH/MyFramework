#ifndef _VECTOR2_USHORT_H_
#define _VECTOR2_USHORT_H_

struct Vector2UShort
{
public:
	ushort x;
	ushort y;
public:
	Vector2UShort()
	{
		x = 0;
		y = 0;
	}
	Vector2UShort(ushort xx, ushort yy)
	{
		x = xx;
		y = yy;
	}
	void clear()
	{
		x = 0;
		y = 0;
	}
	Vector2UShort operator*(ushort that) const
	{
		return Vector2UShort(x * that, y * that);
	}
	Vector2UShort operator+(const Vector2UShort& that) const
	{
		return Vector2UShort(x + that.x, y + that.y);
	}
	Vector2UShort operator-(const Vector2UShort& that) const
	{
		return Vector2UShort(x - that.x, y - that.y);
	}
	Vector2UShort& operator+=(const Vector2UShort& that)
	{
		x += that.x;
		y += that.y;
		return *this;
	}
	Vector2UShort& operator-=(const Vector2UShort& that)
	{
		x -= that.x;
		y -= that.y;
		return *this;
	}
	Vector2UShort operator-()const
	{
		return Vector2UShort(-x, -y);
	}
	bool operator<(const Vector2UShort& other)const
	{
		return x + y < other.x + other.y;
	}
};

#endif