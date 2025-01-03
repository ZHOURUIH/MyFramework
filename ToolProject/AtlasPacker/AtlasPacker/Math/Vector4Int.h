#ifndef _VECTOR4_INT_H_
#define _VECTOR4_INT_H_

struct Vector4Int
{
public:
	int x;
	int y;
	int z;
	int w;
public:
	Vector4Int()
	{
		x = 0;
		y = 0;
		z = 0;
		w = 0;
	}
	Vector4Int(int xx, int yy, int zz, int ww)
	{
		x = xx;
		y = yy;
		z = zz;
		w = ww;
	}
	void clear()
	{
		x = 0;
		y = 0;
		z = 0;
		w = 0;
	}
	Vector4Int operator+(const Vector4Int& that)
	{
		return Vector4Int(x + that.x, y + that.y, z + that.z, w + that.w);
	}
	Vector4Int operator-(const Vector4Int& that)
	{
		return Vector4Int(x - that.x, y - that.y, z - that.z, w - that.w);
	}
	Vector4Int& operator+=(const Vector4Int& that)
	{
		x += that.x;
		y += that.y;
		z += that.z;
		w += that.w;
		return *this;
	}
	Vector4Int& operator-=(const Vector4Int& that)
	{
		x -= that.x;
		y -= that.y;
		z -= that.z;
		w -= that.w;
		return *this;
	}
	Vector4Int operator-()
	{
		return Vector4Int(-x, -y, -z, -w);
	}
};

#endif