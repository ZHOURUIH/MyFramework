#pragma once

#include "FrameMacro.h"

struct Vector2Int
{
public:
	int x = 0;
	int y = 0;
	static Vector2Int ZERO;
public:
	Vector2Int() = default;
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

namespace std
{
	template<>
	struct hash<Vector2Int>
	{
		size_t operator()(const Vector2Int& key) const
		{
			// 使用位运算合并哈希值（确保哈希分布均匀）
			return (hash<int>()(key.x) ^ (hash<int>()(key.y) << 1)) >> 1;
		}
	};
}