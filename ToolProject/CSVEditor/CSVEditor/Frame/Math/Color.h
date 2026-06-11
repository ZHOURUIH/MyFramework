#pragma once

#include "FrameMacro.h"

struct Color
{
public:
	byte r = 0;
	byte g = 0;
	byte b = 0;
	byte a = 0;
	static Color WHITE;
	static Color BLACK;
	static Color RED;
	static Color GREEN;
	static Color BLUE;
	static Color GREY;
public:
	Color() = default;
	Color(const byte rr, const byte gg, const byte bb, const byte aa):
		r(rr),
		g(gg),
		b(bb),
		a(aa)
	{}
	Color operator+(const Color& that) { return Color(r + that.r, g + that.g, b + that.b, a + that.a); }
	Color operator-(const Color& that) { return Color(r - that.r, g - that.g, b - that.b, a - that.a); }
	Color& operator+=(const Color& that)
	{
		r -= that.r;
		g -= that.g;
		b -= that.b;
		a -= that.a;
		return *this;
	}
	Color& operator-=(const Color& that)
	{
		r -= that.r;
		g -= that.g;
		b -= that.b;
		a -= that.a;
		return *this;
	}
};