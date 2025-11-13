#ifndef _COLOR_H_
#define _COLOR_H_

struct Color
{
public:
	byte r;
	byte g;
	byte b;
	byte a;
	static Color WHITE;
	static Color BLACK;
	static Color RED;
	static Color GREEN;
	static Color BLUE;
	static Color GREY;
public:
	Color()
	{
		r = 0;
		g = 0;
		b = 0;
		a = 0;
	}
	Color(unsigned char xx, unsigned char yy, unsigned char zz, unsigned char ww)
	{
		r = xx;
		g = yy;
		b = zz;
		a = ww;
	}
	Color operator+(const Color& that)
	{
		return Color(r + that.r, g + that.g, b + that.b, a + that.a);
	}
	Color operator-(const Color& that)
	{
		return Color(r - that.r, g - that.g, b - that.b, a - that.a);
	}
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

#endif