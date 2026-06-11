#include "FrameHeader.h"

Quaternion::Quaternion(const Quaternion& other):
	x(other.x),
	y(other.y),
	z(other.z),
	w(other.w)
{}

Quaternion::Quaternion(const float xx, const float yy, const float zz, const float ww):
	x(xx),
	y(yy),
	z(zz),
	w(ww)
{}

Quaternion::Quaternion(const Vector3& v, float angleRadian)
{
	angleRadian *= 0.5f;
	const float sinValue = sin(angleRadian);
	w = cos(angleRadian);
	if (isEqual(getSquaredLength(v), 1.0f))
	{
		x = v.x * sinValue;
		y = v.y * sinValue;
		z = v.z * sinValue;
	}
	else
	{
		const Vector3 newV = MathUtility::normalize(v);
		x = newV.x * sinValue;
		y = newV.y * sinValue;
		z = newV.z * sinValue;
	}
}

Quaternion Quaternion::normalize() const
{
	const float len = length();
	if (len <= 0.0f)
	{
		return Quaternion();
	}
	const float oneOverLen = 1.0f / len;
	return { w * oneOverLen, x * oneOverLen, y * oneOverLen, z * oneOverLen };
}

Vector3 Quaternion::axis()
{
	const float tmp1 = 1.0f - w * w;
	if (tmp1 <= 0.0f)
	{
		return Vector3::FORWARD;
	}
	const float tmp2 = 1.0f / sqrt(tmp1);
	return { x * tmp2, y * tmp2, z * tmp2 };
}

Quaternion Quaternion::operator*(const Quaternion& that) const
{
	const Quaternion p(*this);
	const Quaternion q(that);
	return { p.w * q.x + p.x * q.w + p.y * q.z - p.z * q.y,
			 p.w * q.y + p.y * q.w + p.z * q.x - p.x * q.z,
			 p.w * q.z + p.z * q.w + p.x * q.y - p.y * q.x,
			 p.w * q.w - p.x * q.x - p.y * q.y - p.z * q.z };
}

Vector3 Quaternion::operator*(const Vector3& vec)
{
	const float x0 = x * 2.0f;
	const float y0 = y * 2.0f;
	const float z0 = z * 2.0f;
	const float xx = x * x0;
	const float yy = y * y0;
	const float zz = z * z0;
	const float xy = x * y0;
	const float xz = x * z0;
	const float yz = y * z0;
	const float wx = w * x0;
	const float wy = w * y0;
	const float wz = w * z0;
	return { (1.0f - (yy + zz)) * vec.x + (xy - wz) * vec.y + (xz + wy) * vec.z,
			 (xy + wz) * vec.x + (1.0f - (xx + zz)) * vec.y + (yz - wx) * vec.z,
			 (xz - wy) * vec.x + (yz + wx) * vec.y + (1.0f - (xx + yy)) * vec.z };
}

Quaternion& Quaternion::operator*=(const Quaternion& that)
{
	const Quaternion p(*this);
	const Quaternion q(that);
	w = p.w * q.w - p.x * q.x - p.y * q.y - p.z * q.z;
	x = p.w * q.x + p.x * q.w + p.y * q.z - p.z * q.y;
	y = p.w * q.y + p.y * q.w + p.z * q.x - p.x * q.z;
	z = p.w * q.z + p.z * q.w + p.x * q.y - p.y * q.x;
	return *this;
}

Quaternion Quaternion::eulerAnglesToQuaterion(const Vector3& eulerAngles)
{
	const Vector3 c(cos(eulerAngles.x * 0.5f), cos(eulerAngles.y * 0.5f), cos(eulerAngles.z * 0.5f));
	const Vector3 s(sin(eulerAngles.x * 0.5f), sin(eulerAngles.y * 0.5f), sin(eulerAngles.z * 0.5f));
	return { s.x * c.y * c.z - c.x * s.y * s.z,
			 c.x * s.y * c.z + s.x * c.y * s.z,
			 c.x * c.y * s.z - s.x * s.y * c.z,
			 c.x * c.y * c.z + s.x * s.y * s.z };
}

Quaternion Quaternion::cross(const Quaternion& q1, const Quaternion& q2)
{
	return { q1.w * q2.w - q1.x * q2.x - q1.y * q2.y - q1.z * q2.z,
			 q1.w * q2.x + q1.x * q2.w + q1.y * q2.z - q1.z * q2.y,
			 q1.w * q2.y + q1.y * q2.w + q1.z * q2.x - q1.x * q2.z,
			 q1.w * q2.z + q1.z * q2.w + q1.x * q2.y - q1.y * q2.x };
}