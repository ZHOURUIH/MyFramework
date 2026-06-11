#pragma once

#include "FrameMacro.h"

struct Vector3Int
{
public:
	int x = 0;
	int y = 0;
	int z = 0;
public:
	Vector3Int() = default;
	Vector3Int(const int xx, const int yy, const int zz):
		x(xx),
		y(yy),
		z(zz)
	{}
	void clear()
	{
		x = 0;
		y = 0;
		z = 0;
	}
	Vector3Int operator*(const int that) const { return { x * that, y * that, z * that }; }
	Vector3Int operator+(const Vector3Int& that) const { return { x + that.x, y + that.y, z + that.z }; }
	Vector3Int operator-(const Vector3Int& that) const { return { x - that.x, y - that.y, z - that.z }; }
	Vector3Int& operator+=(const Vector3Int& that)
	{
		x += that.x;
		y += that.y;
		z += that.z;
		return *this;
	}
	Vector3Int& operator-=(const Vector3Int& that)
	{
		x -= that.x;
		y -= that.y;
		z -= that.z;
		return *this;
	}
	Vector3Int operator-() const { return { -x, -y, -z }; }
	bool operator<(const Vector3Int& other) const { return x + y + z < other.x + other.y + other.z; }
	bool operator==(const Vector3Int& other) const { return x == other.x && y == other.y && z == other.z; }
	bool operator!=(const Vector3Int& other) const { return x != other.x || y != other.y || z != other.z; }
};