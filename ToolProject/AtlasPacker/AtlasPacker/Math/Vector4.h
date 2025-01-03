#ifndef _VECTOR4_H_
#define _VECTOR4_H_

struct Vector4
{
public:
	float x;
	float y;
	float z;
	float w;
public:
	Vector4()
	{
		x = 0.0f;
		y = 0.0f;
		z = 0.0f;
		w = 0.0f;
	}
	Vector4(float xx, float yy, float zz, float ww)
	{
		x = xx;
		y = yy;
		z = zz;
		w = ww;
	}
	void clear()
	{
		x = 0.0f;
		y = 0.0f;
		z = 0.0f;
		w = 0.0f;
	}
	Vector4 operator+(const Vector4& that)
	{
		return Vector4(x + that.x, y + that.y, z + that.z, w + that.w);
	}
	Vector4 operator-(const Vector4& that)
	{
		return Vector4(x - that.x, y - that.y, z - that.z, w - that.w);
	}
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
	Vector4 operator-()
	{
		return Vector4(-x, -y, -z, -w);
	}
};

#endif