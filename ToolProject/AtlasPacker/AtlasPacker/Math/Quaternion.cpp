#include "Quaternion.h"
#include "MathUtility.h"

Quaternion::Quaternion()
{
	x = 0.0f;
	y = 0.0f;
	z = 0.0f;
	w = 1.0f;
}

Quaternion::Quaternion(const Quaternion& other)
{
	x = other.x;
	y = other.y;
	z = other.z;
	w = other.w;
}

Quaternion::Quaternion(float xx, float yy, float zz, float ww)
{
	x = xx;
	y = yy;
	z = zz;
	w = ww;
}

Quaternion::Quaternion(float angleRadian, const Vector3& v)
{
	angleRadian *= 0.5f;
	float sinValue = sin(angleRadian);
	w = cos(angleRadian);
	if (MathUtility::isFloatEqual(MathUtility::getSquaredLength(v), 1.0f))
	{
		x = v.x * sinValue;
		y = v.y * sinValue;
		z = v.z * sinValue;
	}
	else
	{
		Vector3 newV = MathUtility::normalize(v);
		x = newV.x * sinValue;
		y = newV.y * sinValue;
		z = newV.z * sinValue;
	}
}

void Quaternion::clear()
{
	x = 0.0f;
	y = 0.0f;
	z = 0.0f;
	w = 1.0f;
}

Quaternion Quaternion::conjugate() const
{
	return Quaternion(-x, -y, -z, w);
}

Quaternion Quaternion::inverse() const
{
	return conjugate() / dot(*this, *this);
}

float Quaternion::length() const
{
	return sqrt(dot(*this, *this));
}

Quaternion Quaternion::normalize() const
{
	float len = length();
	if (len <= 0.0f)
	{
		return Quaternion();
	}
	float oneOverLen = 1.0f / len;
	return Quaternion(w * oneOverLen, x * oneOverLen, y * oneOverLen, z * oneOverLen);
}

Vector3 Quaternion::eulerAngles()
{
	return Vector3(pitch(), yaw(), roll());
}

float Quaternion::roll()
{
	return atan2(2 * (x * y + w * z), w * w + x * x - y * y - z * z);
}

float Quaternion::pitch()
{
	return atan2(2.0f * (y * z + w * x), w * w - x * x - y * y + z * z);
}

float Quaternion::yaw()
{
	return asin(-2.0f * (x * z - w * y));
}

float Quaternion::angle()
{
	return acos(w) * 2.0f;
}

Vector3 Quaternion::axis()
{
	float tmp1 = 1.0f - w * w;
	if (tmp1 <= 0.0f)
	{
		return Vector3::FORWARD;
	}
	float tmp2 = 1.0f / sqrt(tmp1);
	return Vector3(x * tmp2, y * tmp2, z * tmp2);
}

Quaternion Quaternion::operator*(const Quaternion& that) const
{
	Quaternion p(*this);
	Quaternion q(that);
	return Quaternion(p.w * q.x + p.x * q.w + p.y * q.z - p.z * q.y,
					  p.w * q.y + p.y * q.w + p.z * q.x - p.x * q.z,
					  p.w * q.z + p.z * q.w + p.x * q.y - p.y * q.x,
					  p.w * q.w - p.x * q.x - p.y * q.y - p.z * q.z);
}

Vector3 Quaternion::operator*(const Vector3& vec)
{
	float x0 = x * 2.0f;
	float y0 = y * 2.0f;
	float z0 = z * 2.0f;
	float xx = x * x0;
	float yy = y * y0;
	float zz = z * z0;
	float xy = x * y0;
	float xz = x * z0;
	float yz = y * z0;
	float wx = w * x0;
	float wy = w * y0;
	float wz = w * z0;
	return Vector3((1.0f - (yy + zz)) * vec.x + (xy - wz) * vec.y + (xz + wy) * vec.z,
				   (xy + wz) * vec.x + (1.0f - (xx + zz)) * vec.y + (yz - wx) * vec.z,
				   (xz - wy) * vec.x + (yz + wx) * vec.y + (1.0f - (xx + yy)) * vec.z);
}

Quaternion Quaternion::operator*(float scale) const
{
	return Quaternion(x * scale, y * scale, z * scale, w * scale);
}

Quaternion Quaternion::operator/(float that) const
{
	float inverse = 1.0f / that;
	return Quaternion(x * inverse, y * inverse, z * inverse, w * inverse);
}

Quaternion Quaternion::operator+(const Quaternion& that) const
{
	return Quaternion(x + that.x, y + that.y, z + that.z, w + that.w);
}

Quaternion& Quaternion::operator+=(const Quaternion& that)
{
	x += that.x;
	y += that.y;
	z += that.z;
	w += that.w;
	return *this;
}

Quaternion& Quaternion::operator*=(float scale)
{
	x *= scale;
	y *= scale;
	z *= scale;
	w *= scale;
	return *this;
}

Quaternion& Quaternion::operator/=(float scale)
{
	float inverseScale = 1.0f / scale;
	x *= inverseScale;
	y *= inverseScale;
	z *= inverseScale;
	w *= inverseScale;
	return *this;
}

Quaternion& Quaternion::operator*=(const Quaternion& that)
{
	Quaternion p(*this);
	Quaternion q(that);

	this->w = p.w * q.w - p.x * q.x - p.y * q.y - p.z * q.z;
	this->x = p.w * q.x + p.x * q.w + p.y * q.z - p.z * q.y;
	this->y = p.w * q.y + p.y * q.w + p.z * q.x - p.x * q.z;
	this->z = p.w * q.z + p.z * q.w + p.x * q.y - p.y * q.x;
	return *this;
}

Quaternion Quaternion::operator-() const
{
	return Quaternion(-x, -y, -z, -w);
}

Quaternion Quaternion::eulerAnglesToQuaterion(const Vector3& eulerAngles)
{
	Vector3 c(cos(eulerAngles.x * 0.5f), cos(eulerAngles.y * 0.5f), cos(eulerAngles.z * 0.5f));
	Vector3 s(sin(eulerAngles.x * 0.5f), sin(eulerAngles.y * 0.5f), sin(eulerAngles.z * 0.5f));
	return Quaternion(s.x * c.y * c.z - c.x * s.y * s.z,
					  c.x * s.y * c.z + s.x * c.y * s.z,
					  c.x * c.y * s.z - s.x * s.y * c.z,
					  c.x * c.y * c.z + s.x * s.y * s.z);
}

float Quaternion::dot(const Quaternion& q1, const Quaternion& q2)
{
	return q1.x * q2.x + q1.y * q2.y + q1.z * q2.z + q1.w * q2.w;
}

Quaternion Quaternion::cross(const Quaternion& q1, const Quaternion& q2)
{
	return Quaternion(q1.w * q2.w - q1.x * q2.x - q1.y * q2.y - q1.z * q2.z,
					  q1.w * q2.x + q1.x * q2.w + q1.y * q2.z - q1.z * q2.y,
					  q1.w * q2.y + q1.y * q2.w + q1.z * q2.x - q1.x * q2.z,
					  q1.w * q2.z + q1.z * q2.w + q1.x * q2.y - q1.y * q2.x);
}

Quaternion Quaternion::lerp(const Quaternion& x, const Quaternion& y, float a)
{
	return x * (1.0f - a) + (y * a);
}