using UnityEngine;

// Quaternion扩展方法
public static class QuaternionExtension
{
	public static bool isEqual(this Quaternion value0, Quaternion value1, float precision = 0.0001f)
	{
		return value0.x.isEqual(value1.x, precision) &&
			   value0.y.isEqual(value1.y, precision) &&
			   value0.z.isEqual(value1.z, precision) &&
			   value0.w.isEqual(value1.w, precision);
	}
	public static float getQuaternionYaw(this Quaternion q) { return q.eulerAngles.y; }
	public static float getQuaternionPitch(this Quaternion q) { return q.eulerAngles.z; }
	public static float getQuaternionRoll(this Quaternion q) { return q.eulerAngles.x; }
}