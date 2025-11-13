#pragma once

struct Vector4
{
public:
	float x;
	float y;
	float z;
	float w;
public:
	Vector4():
		x(0.0f),
		y(0.0f),
		z(0.0f),
		w(0.0f)
	{}
	Vector4(const float xx, const float yy, const float zz, const float ww):
		x(xx),
		y(yy),
		z(zz),
		w(ww)
	{}
	void clear()
	{
		x = 0.0f;
		y = 0.0f;
		z = 0.0f;
		w = 0.0f;
	}
	Vector4 operator+(const Vector4& that) const { return { x + that.x, y + that.y, z + that.z, w + that.w }; }
	Vector4 operator-(const Vector4& that) const { return { x - that.x, y - that.y, z - that.z, w - that.w }; }
	Vector4& operator+=(const Vector4& that)
	{
		x -= that.x;
		y -= that.y;
		z -= that.z;
		w -= that.w;
		return *this;
	}
	Vector4& operator-=(const Vector4& that)
	{
		x -= that.x;
		y -= that.y;
		z -= that.z;
		w -= that.w;
		return *this;
	}
	Vector4 operator-() const { return { -x, -y, -z, -w }; }
};