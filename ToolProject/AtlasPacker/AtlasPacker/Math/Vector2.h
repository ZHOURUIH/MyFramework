#ifndef _VECTOR2_H_
#define _VECTOR2_H_

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
	Vector2()
	{
		x = 0.0f;
		y = 0.0f;
	}
	Vector2(float xx, float yy)
	{
		x = xx;
		y = yy;
	}
	void clear()
	{
		x = 0.0f;
		y = 0.0f;
	}
	Vector2 operator*(float that) const
	{
		return Vector2(x * that, y * that);
	}
	Vector2 operator/(float that) const
	{
		float inverse = 1.0f / that;
		return Vector2(x * inverse, y * inverse);
	}
	Vector2 operator+(const Vector2& that) const
	{
		return Vector2(x + that.x, y + that.y);
	}
	Vector2 operator-(const Vector2& that) const
	{
		return Vector2(x - that.x, y - that.y);
	}
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
	Vector2 operator-()
	{
		return Vector2(-x, -y);
	}
};

#endif