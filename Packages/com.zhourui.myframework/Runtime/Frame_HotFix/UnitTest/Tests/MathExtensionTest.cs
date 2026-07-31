using UnityEngine;
using static TestAssert;

// MathExtension 纯数学扩展方法测试
public static class MathExtensionTest
{
	public static void Run()
	{
		// 原有测试
		testTrigonometry();
		testNormalize();
		testClampLength();
		testGetAngle();
		testLengthComparisons();
		testLengthComparisonsIgnoreY();
		testDivideAndPow();
		testGetGreaterPowValue();
		testQuaternionAngles();
		testComponentReplace();

		// 新增测试 — 舍入/取整
		testCeilFloorRound();
		// 新增测试 — 数学函数
		testSqrtPow();
		testFmodFracStep();
		testAcosAsinCos();
		// 新增测试 — 饱和/夹紧/周期
		testSaturate();
		testClampCycle();
		testClampMinMax();
		// 新增测试 — 精度检查
		testCheckFloatInt();
		testIsNaN();
		testIsZeroDouble();
		// 新增测试 — 范围/比较
		testInRange();
		testInRangeFixed();
		testIsGreaterLess();
		// 新增测试 — 向量运算
		testDotCross();
		testSetLength();
		testMulti();
		testGetLengthIgnoreY();
		// 新增测试 — 角度/弧度转换
		testToRadianDegree();
		testAdjustAngleRadian();
		testGetVectorFromAngle();
		// 新增测试 — 旋转
		testRotate();
		testGetQuaternionYaw();
		// 新增测试 — 单位转换
		testUnitConversion();
		// 新增测试 — 索引/坐标
		testIndexPos();
		testGenerateBatchCount();
		// 新增测试 — 位运算/杂项
		testHasMask();
		testDivideInt();
		testGetGreaterPow2();
		testReplaceZ();
		// 新增测试 — 长度比较(无IgnoreY)
		testSquaredLength();
		testLengthLess();
		testLengthGreater();
		// 新增测试 — clamp(双边界)
		testClamp();
		// 新增测试 — inverse / divide
		testInverse();
		testDivide();
		testInversePow10();
	}

	// ==================== 原有测试 ====================

	// atan / tan
	static void testTrigonometry()
	{
		assertTrue(0.0f.atan().isEqual(0.0f, 0.0001f), "atan0");
		assertTrue(1.0f.atan().isEqual(Mathf.PI * 0.25f, 0.001f), "atan1");
		assertTrue(0.0f.tan().isEqual(0.0f, 0.0001f), "tan0");
		assertTrue((Mathf.PI * 0.25f).tan().isEqual(1.0f, 0.001f), "tan45");
	}

	// normalize
	static void testNormalize()
	{
		Vector3 v3 = new Vector3(3, 4, 0).normalize();
		assertTrue(v3.getLength().isEqual(1.0f, 0.001f), "normalize V3 len1");
		Vector2 v2 = new Vector2(3, 4).normalize();
		assertTrue(v2.getLength().isEqual(1.0f, 0.001f), "normalize V2 len1");
	}

	// clampLength
	static void testClampLength()
	{
		Vector3 shortV = new Vector3(1, 0, 0).clampLength(5.0f);
		assertTrue(shortV.x.isEqual(1.0f, 0.001f), "clampLength short unchanged");
		Vector3 longV = new Vector3(10, 0, 0).clampLength(3.0f);
		assertTrue(longV.getLength().isEqual(3.0f, 0.001f), "clampLength long clamped");
	}

	// getAngle (Vector3/Vector2, 弧度制)
	static void testGetAngle()
	{
		float a3 = new Vector3(0, 0, 1).getAngle();
		assertTrue(a3.isZero(0.01f), "getAngle fwd=0");
		float a2 = new Vector2(0, 1).getAngle();
		assertTrue(a2.isZero(0.01f), "getAngle V2 fwd=0");
		float deg = new Vector3(1, 0, 0).getAngle(ANGLE.DEGREE);
		assertTrue(deg.abs().isEqual(90.0f, 1.0f), "getAngle V3 deg90");
	}

	// 长度比较 (不忽略Y)
	static void testLengthComparisons()
	{
		assertTrue(new Vector2(1, 1).lengthLessEqual(2.0f), "lenLessEqual V2 f");
		assertFalse(new Vector2(3, 3).lengthLessEqual(2.0f), "lenLessEqual V2 false");
		assertTrue(new Vector2(1, 1).lengthLessEqual(new Vector2(2, 2)), "lenLessEqual V2 V2");
		assertTrue(new Vector3(1, 1, 1).lengthLessEqual(2.0f), "lenLessEqual V3 f");
		assertTrue(new Vector3(1, 1, 1).lengthLessEqual(new Vector3(2, 2, 2)), "lenLessEqual V3 V3");
		assertTrue(new Vector3(3, 4, 0).lengthGreaterEqual(5.0f), "lenGreaterEqual eq");
		assertFalse(new Vector3(1, 1, 1).lengthGreaterEqual(5.0f), "lenGreaterEqual false");
	}

	// 长度比较 (忽略Y)
	static void testLengthComparisonsIgnoreY()
	{
		Vector3 v = new Vector3(3, 99, 4);
		assertTrue(v.getSquaredLengthIgnoreY().isEqual(25.0f, 0.001f), "sqLenIgnoreY");
		assertTrue(v.lengthLessEqualIgnoreY(6.0f), "lenLessEqualIgY");
		assertFalse(v.lengthLessEqualIgnoreY(4.0f), "lenLessEqualIgY false");
		assertTrue(v.lengthGreaterEqualIgnoreY(5.0f), "lenGreaterEqualIgY eq");
		assertTrue(v.lengthGreaterIgnoreY(4.9f), "lenGreaterIgY");
		assertTrue(v.lengthLessIgnoreY(6.0f), "lenLessIgY");
		assertFalse(v.lengthLessIgnoreY(4.0f), "lenLessIgY false");
	}

	// divideLong / inversePow10Long
	static void testDivideAndPow()
	{
		assertEqual(3L, 10L.divideLong(3L), "divideLong");
		assertEqual(0L, 10L.divideLong(0L), "divideLong by0");
		assertEqual(-1L, 10L.divideLong(0L, -1L), "divideLong by0 default");
		assertTrue(2.inversePow10Long().isEqual(0.01, 0.0001), "inversePow10Long");
	}

	// getGreaterPowValue
	static void testGetGreaterPowValue()
	{
		assertEqual(8, 7.getGreaterPowValue(2), "getGreaterPowValue 7/2");
		assertEqual(16, 10.getGreaterPowValue(2), "getGreaterPowValue 10/2");
		assertEqual(9, 5.getGreaterPowValue(3), "getGreaterPowValue 5/3");
		assertEqual(1, 1.getGreaterPowValue(2), "getGreaterPowValue 1/2");
	}

	// getQuaternionPitch / getQuaternionRoll
	static void testQuaternionAngles()
	{
		Quaternion q = Quaternion.Euler(30, 0, 45);
		assertTrue(q.getQuaternionPitch().isEqual(45.0f, 0.01f), "qPitch z=45");
		assertTrue(q.getQuaternionRoll().isEqual(30.0f, 0.01f), "qRoll x=30");
	}

	// replaceX/replaceY/resetZ
	static void testComponentReplace()
	{
		Vector3 v3 = new Vector3(1, 2, 3);
		assertEqual(new Vector3(9, 2, 3), v3.replaceX(9), "replaceX V3");
		assertEqual(new Vector3(1, 9, 3), v3.replaceY(9), "replaceY V3");
		assertEqual(new Vector3(1, 2, 0), v3.resetZ(), "resetZ V3");
		assertEqual(new Vector3(0, 2, 3), v3.resetX(), "resetX V3");
		assertEqual(new Vector3(1, 0, 3), v3.resetY(), "resetY V3");
		Vector2 v2 = new Vector2(1, 2);
		assertEqual(new Vector2(9, 2), v2.replaceX(9), "replaceX V2");
		assertEqual(new Vector2(1, 9), v2.replaceY(9), "replaceY V2");
		assertEqual(new Vector2(0, 2), v2.resetX(), "resetX V2");
		assertEqual(new Vector2(1, 0), v2.resetY(), "resetY V2");
	}

	// ==================== 新增测试 ====================

	// ---- ceil / floor / round ----
	static void testCeilFloorRound()
	{
		// float ceil
		assertEqual(4, 3.2f.ceil(), "ceil 3.2");
		assertEqual(4, 3.8f.ceil(), "ceil 3.8");
		assertEqual(-3, (-3.2f).ceil(), "ceil -3.2");
		assertEqual(-3, (-3.8f).ceil(), "ceil -3.8");
		assertEqual(0, 0.0f.ceil(), "ceil 0");
		// Vector2 ceil
		Vector2 v2Ceil = new Vector2(3.2f, -3.8f).ceil();
		assertTrue(v2Ceil.x.isEqual(4f, 0.001f) && v2Ceil.y.isEqual(-3f, 0.001f), "ceil V2");
		// Vector3 ceil
		Vector3 v3Ceil = new Vector3(3.2f, 4.7f, -1.2f).ceil();
		assertTrue(v3Ceil.x.isEqual(4f, 0.001f) && v3Ceil.y.isEqual(5f, 0.001f) && v3Ceil.z.isEqual(-1f, 0.001f), "ceil V3");

		// float floor
		assertEqual(3, 3.2f.floor(), "floor 3.2");
		assertEqual(3, 3.8f.floor(), "floor 3.8");
		assertEqual(-4, (-3.2f).floor(), "floor -3.2");
		assertEqual(-4, (-3.8f).floor(), "floor -3.8");
		assertEqual(0, 0.0f.floor(), "floor 0");
		// double floor
		assertEqual(3, 3.2.floor(), "floor double 3.2");
		assertEqual(-4, (-3.2).floor(), "floor double -3.2");
		// Vector2 floor
		Vector2 v2Floor = new Vector2(3.8f, -3.2f).floor();
		assertTrue(v2Floor.x.isEqual(3f, 0.001f) && v2Floor.y.isEqual(-4f, 0.001f), "floor V2");
		// Vector3 floor
		Vector3 v3Floor = new Vector3(3.8f, -4.2f, 0.5f).floor();
		assertTrue(v3Floor.x.isEqual(3f, 0.001f) && v3Floor.y.isEqual(-5f, 0.001f) && v3Floor.z.isEqual(0f, 0.001f), "floor V3");

		// float round
		assertEqual(3, 3.2f.round(), "round 3.2");
		assertEqual(4, 3.8f.round(), "round 3.8");
		assertEqual(4, 3.5f.round(), "round 3.5"); // Banker's rounding or AwayFromZero
		assertEqual(-3, (-3.2f).round(), "round -3.2");
		assertEqual(-4, (-3.8f).round(), "round -3.8");
		// double round
		assertEqual(3L, 3.2.round(), "round double 3.2");
		assertEqual(-4L, (-3.8).round(), "round double -3.8");
		// Vector2 round
		Vector2 v2Round = new Vector2(3.2f, -3.8f).round();
		assertTrue(v2Round.x.isEqual(3f, 0.001f) && v2Round.y.isEqual(-4f, 0.001f), "round V2");
		// Vector3 round
		Vector3 v3Round = new Vector3(3.2f, 3.8f, -3.5f).round();
		assertTrue(v3Round.x.isEqual(3f, 0.001f) && v3Round.y.isEqual(4f, 0.001f), "round V3");
	}

	// ---- sqrt / pow / pow2 / pow10 ----
	static void testSqrtPow()
	{
		// sqrt float
		assertTrue(4.0f.sqrt().isEqual(2.0f, 0.001f), "sqrt 4");
		assertTrue(2.0f.sqrt().isEqual(1.414f, 0.01f), "sqrt 2");
		assertTrue(0.0f.sqrt().isZero(), "sqrt 0");
		// sqrt int
		assertTrue(9.sqrt().isEqual(3.0f, 0.001f), "sqrt int 9");
		// pow float
		assertTrue(2.0f.pow(3.0f).isEqual(8.0f, 0.001f), "pow 2^3");
		assertTrue(2.0f.pow(0).isEqual(1.0f, 0.001f), "pow 2^0 int");
		assertTrue(3.0f.pow(2).isEqual(9.0f, 0.001f), "pow 3^2 int");
		// pow2
		assertEqual(4.0f, 2.pow2(), "pow2 2^2=4");
		assertEqual(8.0f, 3.pow2(), "pow2 2^3=8");
		assertEqual(1.0f, 0.pow2(), "pow2 2^0=1");
		// pow10
		assertEqual(100, 2.pow10(), "pow10 10^2");
		assertEqual(1, 0.pow10(), "pow10 10^0");
		assertEqual(1000, 3.pow10(), "pow10 10^3");
		// pow10Long
		assertEqual(100L, 2.pow10Long(), "pow10Long 10^2");
		assertEqual(1L, 0.pow10Long(), "pow10Long 10^0");
		// inversePow10
		float inv10 = 2.inversePow10Long().toFloat();
		assertTrue(inv10.isEqual(0.01f, 0.001f), "inversePow10 2");
	}

	// ---- fmod / frac / step ----
	static void testFmodFracStep()
	{
		// fmod
		assertTrue(10.0f.fmod(3.0f).isEqual(1.0f, 0.001f), "fmod 10%3=1");
		assertTrue(10.0f.fmod(5.0f).isEqual(0.0f, 0.001f), "fmod 10%5=0");
		assertTrue(3.5f.fmod(1.5f).isEqual(0.5f, 0.001f), "fmod 3.5%1.5=0.5");
		// frac
		assertTrue(3.14f.frac().isEqual(0.14f, 0.01f), "frac 3.14");
		assertTrue((-3.14f).frac().isEqual(-0.14f, 0.01f), "frac -3.14");
		assertTrue(5.0f.frac().isZero(0.001f), "frac 5.0=0");
		// step
		assertEqual(1, 5.0f.step(10.0f), "step 5->10 true");
		assertEqual(1, 5.0f.step(5.0f), "step eq true");
		assertEqual(0, 5.0f.step(3.0f), "step 5->3 false");
	}

	// ---- acos / asin / cos ----
	static void testAcosAsinCos()
	{
		// acos
		assertTrue(1.0f.acos().isZero(0.001f), "acos 1=0");
		assertTrue(0.0f.acos().isEqual(Mathf.PI * 0.5f, 0.001f), "acos 0=pi/2");
		// asin
		assertTrue(0.0f.asin().isZero(0.001f), "asin 0=0");
		assertTrue(1.0f.asin().isEqual(Mathf.PI * 0.5f, 0.001f), "asin 1=pi/2");
		// sin
		assertTrue(0.0f.sin().isZero(0.001f), "sin 0=0");
		assertTrue((Mathf.PI * 0.5f).sin().isEqual(1.0f, 0.001f), "sin pi/2=1");
		// cos
		assertTrue(0.0f.cos().isEqual(1.0f, 0.001f), "cos 0=1");
		assertTrue((Mathf.PI * 0.5f).cos().isZero(0.001f), "cos pi/2=0");
	}

	// ---- saturate ----
	static void testSaturate()
	{
		// float
		assertTrue(0.5f.saturate().isEqual(0.5f, 0.001f), "saturate 0.5");
		assertTrue(0.0f.saturate().isZero(), "saturate 0");
		assertTrue(1.0f.saturate().isEqual(1.0f, 0.001f), "saturate 1");
		assertTrue((-0.5f).saturate().isZero(), "saturate -0.5 -> 0");
		assertTrue(1.5f.saturate().isEqual(1.0f, 0.001f), "saturate 1.5 -> 1");
		// Vector3
		Vector3 sv = new Vector3(-0.5f, 0.5f, 1.5f).saturate();
		assertTrue(sv.x.isZero() && sv.y.isEqual(0.5f, 0.001f) && sv.z.isEqual(1.0f, 0.001f), "saturate V3");
	}

	// ---- clampCycle ----
	static void testClampCycle()
	{
		// int: clampCycle(value, min, max, cycle) — 通过加减 cycle 将 value 移入 [min, max]
		// includeMax=true (默认) 时 max 本身是合法值
		assertEqual(0, 0.clampCycle(0, 3, 1), "clampCycle int 0 in range");
		assertEqual(3, 5.clampCycle(0, 3, 1), "clampCycle int 5 -> 3 (5>3, 5-1=4>3, 4-1=3)");
		assertEqual(3, 4.clampCycle(0, 3, 1), "clampCycle int 4 -> 3 (4>3, 4-1=3)");
		assertEqual(0, (-1).clampCycle(0, 3, 1), "clampCycle int -1 -> 0 (-1<0, -1+1=0)");
		assertEqual(0, (-2).clampCycle(0, 3, 1), "clampCycle int -2 -> 0 (-2+1=-1, -1+1=0)");
		assertEqual(3, 7.clampCycle(0, 3, 2), "clampCycle int 7 step 2 -> 3 (7>3,7-2=5>3,5-2=3)");
		assertEqual(1, (-3).clampCycle(0, 3, 2), "clampCycle int -3 step 2 -> 1 (-3+2=-1+2=1)");
		// float
		assertTrue(2.5f.clampCycle(0.0f, 3.0f, 1.0f).isEqual(2.5f, 0.001f), "clampCycle f 2.5 in range");
		assertTrue(3.0f.clampCycle(0.0f, 3.0f, 1.0f).isEqual(3.0f, 0.001f), "clampCycle f 3.0 at max boundary");
		assertTrue(0.0f.clampCycle(0.0f, 3.0f, 1.0f).isEqual(0.0f, 0.001f), "clampCycle f 0 at min boundary");
		assertTrue(2.5f.clampCycle(0.0f, 3.0f, 1.5f).isEqual(2.5f, 0.001f), "clampCycle f in range step 1.5");
		assertTrue(4.0f.clampCycle(0.0f, 3.0f, 1.5f).isEqual(2.5f, 0.001f), "clampCycle f 4->2.5 (4>3,4-1.5=2.5)");
	}

	// ---- clampMin / clampMax 标量 ----
	static void testClampMinMax()
	{
		// clampMin int
		assertEqual(5, 5.clampMin(3), "clampMin int no change");
		assertEqual(3, 2.clampMin(3), "clampMin int clamped");
		assertEqual(5, 5.clampMin(), "clampMin int default 0");
		// clampMin float
		assertTrue(5.0f.clampMin(3.0f).isEqual(5.0f, 0.001f), "clampMin f no change");
		assertTrue(2.0f.clampMin(3.0f).isEqual(3.0f, 0.001f), "clampMin f clamped");
		// clampMin long
		assertEqual(5L, 5L.clampMin(3L), "clampMin long");
		assertEqual(3L, 1L.clampMin(3L), "clampMin long clamped");
		// clampMin uint
		assertEqual(5u, 5u.clampMin(3u), "clampMin uint");
		// clampMin double
		assertTrue(5.0.clampMin(3.0).isEqual(5.0, 0.001), "clampMin double");
		// clampMin short
		assertEqual((short)5, ((short)5).clampMin((short)3), "clampMin short");
		// clampMin sbyte
		assertEqual((sbyte)5, ((sbyte)5).clampMin((sbyte)3), "clampMin sbyte");
		// clampMin byte
		assertEqual((byte)5, ((byte)5).clampMin((byte)3), "clampMin byte");
		// clampMin ushort
		assertEqual((ushort)5, ((ushort)5).clampMin((ushort)3), "clampMin ushort");
		// clampMin ulong
		assertEqual(5UL, 5UL.clampMin(3UL), "clampMin ulong");

		// clampMax int
		assertEqual(5, 5.clampMax(10), "clampMax int no change");
		assertEqual(10, 15.clampMax(10), "clampMax int clamped");
		// clampMax float
		assertTrue(5.0f.clampMax(10.0f).isEqual(5.0f, 0.001f), "clampMax f no change");
		assertTrue(15.0f.clampMax(10.0f).isEqual(10.0f, 0.001f), "clampMax f clamped");
		// clampMax long
		assertEqual(5L, 5L.clampMax(10L), "clampMax long");
		// clampMax double
		assertTrue(5.0.clampMax(10.0).isEqual(5.0, 0.001), "clampMax double");
	}

	// ---- checkFloat / checkInt ----
	static void testCheckFloatInt()
	{
		// checkFloat
		assertTrue(3.14159f.checkFloat(2).isEqual(3.14f, 0.01f), "checkFloat 2digits");
		assertTrue(3.14159f.checkFloat(4).isEqual(3.1416f, 0.001f), "checkFloat 4digits");
		assertTrue(0.0f.checkFloat().isZero(), "checkFloat 0");
		// checkFloat Vector3
		Vector3 cfv = new Vector3(3.14159f, 2.71828f, 1.41421f).checkFloat(2);
		assertTrue(cfv.x.isEqual(3.14f, 0.01f), "checkFloat V3 x");
		// checkInt float
		assertTrue(5.0f.checkInt().isEqual(5.0f, 0.001f), "checkInt exact");
		assertTrue(5.0001f.checkInt(0.001f).isEqual(5.0f, 0.001f), "checkInt within precision");
		assertTrue(5.1f.checkInt().isEqual(5.1f, 0.001f), "checkInt not int");
		// checkInt Vector3
		Vector3 civ = new Vector3(5.00001f, 3.0f, 2.2f).checkInt(0.001f);
		assertTrue(civ.x.isEqual(5.0f, 0.001f) && civ.y.isEqual(3.0f, 0.001f) && civ.z.isEqual(2.2f, 0.01f), "checkInt V3");
	}

	// ---- isNaN ----
	static void testIsNaN()
	{
		assertTrue(float.NaN.isNaN(), "isNaN float");
		assertFalse(1.0f.isNaN(), "isNaN float false");
		assertTrue(new Vector2(float.NaN, 1).isNaN(), "isNaN V2 x NaN");
		assertTrue(new Vector2(1, float.NaN).isNaN(), "isNaN V2 y NaN");
		assertFalse(new Vector2(1, 2).isNaN(), "isNaN V2 false");
		assertTrue(new Vector3(float.NaN, 1, 2).isNaN(), "isNaN V3 x NaN");
		assertTrue(new Vector3(1, float.NaN, 2).isNaN(), "isNaN V3 y NaN");
		assertTrue(new Vector3(1, 2, float.NaN).isNaN(), "isNaN V3 z NaN");
		assertFalse(new Vector3(1, 2, 3).isNaN(), "isNaN V3 false");
	}

	// ---- isZero double ----
	static void testIsZeroDouble()
	{
		assertTrue(0.0.isZero(), "isZero double 0");
		assertFalse(1.0.isZero(), "isZero double 1");
		assertTrue(0.000000001.isZero(), "isZero double within default precision");
		assertFalse(0.0001.isZero(), "isZero double outside");
	}

	// ---- inRange ----
	static void testInRange()
	{
		// float
		assertTrue(5.0f.inRange(0.0f, 10.0f), "inRange f 5 in [0,10]");
		assertTrue(0.0f.inRange(0.0f, 10.0f), "inRange f 0 in [0,10]");
		assertTrue(10.0f.inRange(0.0f, 10.0f), "inRange f 10 in [0,10]");
		assertFalse((-1.0f).inRange(0.0f, 10.0f), "inRange f -1 out");
		assertFalse(11.0f.inRange(0.0f, 10.0f), "inRange f 11 out");
		// int
		assertTrue(5.inRange(0, 10), "inRange int 5");
		assertFalse((-1).inRange(0, 10), "inRange int -1 out");
		assertTrue(5.inRange(0.0f, 10.0f), "inRange int f range");
		// Vector3
		Vector3 v = new Vector3(5, 1, 5);
		assertTrue(v.inRange(new Vector3(0, 0, 0), new Vector3(10, 0, 10)), "inRange V3");
		assertFalse(v.inRange(new Vector3(0, 0, 0), new Vector3(4, 0, 4)), "inRange V3 out");
		// Vector2
		Vector2 v2 = new Vector2(5, 5);
		assertTrue(v2.inRange(new Vector2(0, 0), new Vector2(10, 10)), "inRange V2");
		assertFalse(v2.inRange(new Vector2(0, 0), new Vector2(4, 4)), "inRange V2 out");
	}

	// ---- inRangeFixed ----
	static void testInRangeFixed()
	{
		// float: inRangeFixed 不自动交换 range0/range1（固定顺序）
		assertTrue(5.0f.inRangeFixed(0.0f, 10.0f), "inRangeFixed f 5");
		assertFalse(5.0f.inRangeFixed(10.0f, 0.0f), "inRangeFixed f reversed (不自动交换)");
		assertFalse((-1.0f).inRangeFixed(0.0f, 10.0f), "inRangeFixed f out");
		// int: 同上
		assertTrue(5.inRangeFixed(0, 10), "inRangeFixed int");
		assertFalse(5.inRangeFixed(10, 0), "inRangeFixed int reversed (不自动交换)");
		assertFalse((-1).inRangeFixed(0, 10), "inRangeFixed int out");
		assertTrue(5.inRangeFixed(0.0f, 10.0f), "inRangeFixed int f range");
	}

	// ---- isGreater / isLess ----
	static void testIsGreaterLess()
	{
		// isGreater V3
		assertTrue(new Vector3(3, 4, 5).isGreater(new Vector3(2, 3, 4)), "isGreater V3 true");
		assertFalse(new Vector3(3, 1, 5).isGreater(new Vector3(2, 3, 4)), "isGreater V3 y fails");
		assertFalse(new Vector3(2, 3, 4).isGreater(new Vector3(2, 3, 4)), "isGreater V3 eq false");
		// isGreater V2
		assertTrue(new Vector2(3, 4).isGreater(new Vector2(2, 3)), "isGreater V2 true");
		assertFalse(new Vector2(3, 1).isGreater(new Vector2(2, 3)), "isGreater V2 false");
		// isLess V3
		assertTrue(new Vector3(1, 2, 3).isLess(new Vector3(4, 5, 6)), "isLess V3 true");
		assertFalse(new Vector3(1, 6, 3).isLess(new Vector3(4, 5, 6)), "isLess V3 y fails");
		// isLess V2
		assertTrue(new Vector2(1, 2).isLess(new Vector2(3, 4)), "isLess V2 true");
		assertFalse(new Vector2(1, 6).isLess(new Vector2(3, 4)), "isLess V2 false");
	}

	// ---- dot / cross ----
	static void testDotCross()
	{
		// dot V3
		assertTrue(new Vector3(1, 0, 0).dot(new Vector3(1, 0, 0)).isEqual(1.0f, 0.001f), "dot V3 same=1");
		assertTrue(new Vector3(1, 0, 0).dot(new Vector3(0, 1, 0)).isZero(), "dot V3 ortho=0");
		// dot V2
		assertTrue(new Vector2(1, 0).dot(new Vector2(1, 0)).isEqual(1.0f, 0.001f), "dot V2 same=1");
		assertTrue(new Vector2(1, 0).dot(new Vector2(0, 1)).isZero(), "dot V2 ortho=0");
		// cross V3
		Vector3 c = new Vector3(1, 0, 0).cross(new Vector3(0, 1, 0));
		assertTrue(c.x.isZero() && c.y.isZero() && c.z.isEqual(1.0f, 0.001f), "cross X x Y = Z");
	}

	// ---- setLength ----
	static void testSetLength()
	{
		Vector3 v3 = new Vector3(3, 0, 0).setLength(5.0f);
		assertTrue(v3.getLength().isEqual(5.0f, 0.001f) && v3.y.isZero() && v3.z.isZero(), "setLength V3 3->5");
		Vector3 v3zero = new Vector3(0, 0, 0).setLength(5.0f);
		assertTrue(v3zero.getLength().isZero(), "setLength V3 zero stays zero");
		Vector2 v2 = new Vector2(3, 0).setLength(5.0f);
		assertTrue(v2.getLength().isEqual(5.0f, 0.001f) && v2.y.isZero(), "setLength V2 3->5");
		Vector2 v2zero = new Vector2(0, 0).setLength(5.0f);
		assertTrue(v2zero.getLength().isZero(), "setLength V2 zero stays zero");
	}

	// ---- multi ----
	static void testMulti()
	{
		Vector2 m2 = new Vector2(2, 3).multi(new Vector2(4, 5));
		assertTrue(m2.x.isEqual(8, 0.001f) && m2.y.isEqual(15, 0.001f), "multi V2");
		Vector3 m3 = new Vector3(2, 3, 4).multi(new Vector3(5, 6, 7));
		assertTrue(m3.x.isEqual(10, 0.001f) && m3.y.isEqual(18, 0.001f) && m3.z.isEqual(28, 0.001f), "multi V3");
	}

	// ---- getLengthIgnoreY ----
	static void testGetLengthIgnoreY()
	{
		assertTrue(new Vector3(3, 99, 4).getLengthIgnoreY().isEqual(5.0f, 0.001f), "getLengthIgnoreY 3-4-5");
		assertTrue(new Vector3(0, 99, 0).getLengthIgnoreY().isZero(), "getLengthIgnoreY zero");
	}

	// ---- toRadian / toDegree ----
	static void testToRadianDegree()
	{
		// toRadian float
		assertTrue(180.0f.toRadian().isEqual(Mathf.PI, 0.001f), "toRadian 180=pi");
		assertTrue(0.0f.toRadian().isZero(), "toRadian 0");
		// toRadian V3
		Vector3 r3 = new Vector3(180, 90, 0).toRadian();
		assertTrue(r3.x.isEqual(Mathf.PI, 0.001f) && r3.y.isEqual(Mathf.PI * 0.5f, 0.001f), "toRadian V3");
		// toDegree float
		assertTrue(Mathf.PI.toDegree().isEqual(180.0f, 0.001f), "toDegree pi=180");
		assertTrue(0.0f.toDegree().isZero(), "toDegree 0");
		// toDegree V3
		Vector3 d3 = new Vector3(Mathf.PI, Mathf.PI * 0.5f, 0).toDegree();
		assertTrue(d3.x.isEqual(180.0f, 0.001f) && d3.y.isEqual(90.0f, 0.001f), "toDegree V3");
	}

	// ---- adjustAngle / adjustRadian ----
	static void testAdjustAngleRadian()
	{
		// adjustAngle180
		assertTrue(30.0f.adjustAngle180().isEqual(30.0f, 0.001f), "adjA180 30");
		assertTrue(200.0f.adjustAngle180().isEqual(-160.0f, 0.001f), "adjA180 200->-160");
		assertTrue((-200.0f).adjustAngle180().isEqual(160.0f, 0.001f), "adjA180 -200->160");
		// adjustAngle180 V3
		Vector3 a3 = new Vector3(200, -200, 30).adjustAngle180();
		assertTrue(a3.x.isEqual(-160.0f, 0.001f) && a3.y.isEqual(160.0f, 0.001f), "adjA180 V3");
		// adjustAngle360
		assertTrue(30.0f.adjustAngle360().isEqual(30.0f, 0.001f), "adjA360 30");
		assertTrue((-30.0f).adjustAngle360().isEqual(330.0f, 0.001f), "adjA360 -30->330");
		assertTrue(400.0f.adjustAngle360().isEqual(40.0f, 0.001f), "adjA360 400->40");
		// adjustAngle360 V3
		Vector3 a360 = new Vector3(-30, 400, 30).adjustAngle360();
		assertTrue(a360.x.isEqual(330.0f, 0.001f) && a360.y.isEqual(40.0f, 0.001f), "adjA360 V3");
		// adjustRadian180
		float pi = Mathf.PI;
		assertTrue((pi * 0.5f).adjustRadian180().isEqual(pi * 0.5f, 0.001f), "adjR180 pi/2");
		assertTrue((pi * 1.5f).adjustRadian180().isEqual(-pi * 0.5f, 0.001f), "adjR180 3pi/2->-pi/2");
		// adjustRadian180 V3
		Vector3 r180 = new Vector3(pi * 1.5f, pi * 0.5f, 0).adjustRadian180();
		assertTrue(r180.x.isEqual(-pi * 0.5f, 0.001f) && r180.y.isEqual(pi * 0.5f, 0.001f), "adjR180 V3");
		// adjustRadian360
		assertTrue((-pi * 0.5f).adjustRadian360().isEqual(pi * 1.5f, 0.001f), "adjR360 -pi/2->3pi/2");
		// adjustRadian360 V3
		Vector3 r360 = new Vector3(-pi * 0.5f, pi * 0.5f, 0).adjustRadian360();
		assertTrue(r360.x.isEqual(pi * 1.5f, 0.001f) && r360.y.isEqual(pi * 0.5f, 0.001f), "adjR360 V3");
	}

	// ---- getVectorFromAngle / getVector2FromAngle ----
	static void testGetVectorFromAngle()
	{
		// getVectorFromAngle (returns Vector3 on XZ plane)
		Vector3 v0 = 0.0f.getVectorFromAngle();
		assertTrue(v0.x.isEqual(0f, 0.001f) && v0.z.isEqual(1.0f, 0.001f), "getVectorFromAngle 0=fwd");
		float halfPi = Mathf.PI * 0.5f;
		Vector3 v90 = halfPi.getVectorFromAngle();
		assertTrue(v90.x.isEqual(1.0f, 0.001f) && v90.z.isZero(0.001f), "getVectorFromAngle pi/2=right");
		// getVector2FromAngle
		Vector2 v2_0 = 0.0f.getVector2FromAngle();
		assertTrue(v2_0.x.isZero(0.001f) && v2_0.y.isEqual(1.0f, 0.001f), "getVector2FromAngle 0=up");
		Vector2 v2_90 = halfPi.getVector2FromAngle();
		assertTrue(v2_90.x.isEqual(1.0f, 0.001f) && v2_90.y.isZero(0.001f), "getVector2FromAngle pi/2=right");
	}

	// ---- rotate ----
	static void testRotate()
	{
		Vector3 v = new Vector3(1, 0, 0);
		// rotate by Quaternion (90 deg around Y)
		Quaternion q = Quaternion.Euler(0, 90, 0);
		Vector3 rq = v.rotate(q);
		assertTrue(rq.x.isZero(0.01f) && rq.z.isEqual(-1.0f, 0.01f), "rotate Quat Y90");
		// rotate by radian (around Y)
		float halfPi = Mathf.PI * 0.5f;
		Vector3 rr = v.rotate(halfPi);
		assertTrue(rr.x.isZero(0.01f) && rr.z.isEqual(-1.0f, 0.01f), "rotate rad pi/2");
		// rotate by Matrix4x4
		Matrix4x4 m = Matrix4x4.Rotate(Quaternion.Euler(0, 90, 0));
		Vector3 rm = v.rotate(m);
		assertTrue(rm.x.isZero(0.01f) && rm.z.isEqual(-1.0f, 0.01f), "rotate Mat Y90");
	}

	// ---- getQuaternionYaw ----
	static void testGetQuaternionYaw()
	{
		Quaternion q = Quaternion.Euler(0, 45, 0);
		assertTrue(q.getQuaternionYaw().isEqual(45.0f, 0.01f), "qYaw 45");
		Quaternion q0 = Quaternion.identity;
		assertTrue(q0.getQuaternionYaw().isZero(0.01f), "qYaw 0");
	}

	// ---- 单位转换 ----
	static void testUnitConversion()
	{
		// KMHtoMS: 1 km/h = 0.27777 m/s
		assertTrue(1.0f.KMHtoMS().isEqual(0.27777f, 0.001f), "KMHtoMS 1");
		assertTrue(100.0f.KMHtoMS().isEqual(27.777f, 0.01f), "KMHtoMS 100");
		// MStoKMH: 1 m/s = 3.6 km/h
		assertTrue(1.0f.MStoKMH().isEqual(3.6f, 0.001f), "MStoKMH 1");
		assertTrue(10.0f.MStoKMH().isEqual(36.0f, 0.001f), "MStoKMH 10");
		// MtoKM: 1 m = 0.001 km
		assertTrue(1000.0f.MtoKM().isEqual(1.0f, 0.001f), "MtoKM 1000");
		assertTrue(0.0f.MtoKM().isZero(), "MtoKM 0");
	}

	// ---- 索引/坐标 ----
	static void testIndexPos()
	{
		// indexToX / indexToY
		assertEqual(0, 0.indexToX(4), "indexToX 0");
		assertEqual(3, 3.indexToX(4), "indexToX 3");
		assertEqual(1, 5.indexToX(4), "indexToX 5 mod 4 = 1");
		assertEqual(0, 0.indexToY(4), "indexToY 0");
		assertEqual(1, 5.indexToY(4), "indexToY 5/4=1");
		assertEqual(2, 10.indexToY(4), "indexToY 10/4=2");
		// indexToIntPos
		Vector2Int p = 5.indexToIntPos(4);
		assertEqual(new Vector2Int(1, 1), p, "indexToIntPos 5@4");
		// intPosToIndex
		assertEqual(5, new Vector2Int(1, 1).intPosToIndex(4), "intPosToIndex (1,1)@4");
		assertEqual(0, new Vector2Int(0, 0).intPosToIndex(4), "intPosToIndex (0,0)@4");
	}

	// ---- generateBatchCount ----
	static void testGenerateBatchCount()
	{
		assertEqual(1, 5.generateBatchCount(10), "batch 5/10=1");
		assertEqual(2, 15.generateBatchCount(10), "batch 15/10=2");
		assertEqual(2, 20.generateBatchCount(10), "batch 20/10=2");
		assertEqual(0, 0.generateBatchCount(10), "batch 0/10=0");
	}

	// ---- hasMask ----
	static void testHasMask()
	{
		assertTrue(0b1010.hasMask(0b0010), "hasMask true");
		assertFalse(0b1010.hasMask(0b0100), "hasMask false"); // wait: 0b1010 & 0b0100 = 0? No: 1010 & 0100 = 0000 = 0, false
		// Actually 0b1010 = 10, 0b0100 = 4, 10 & 4 = 0
		assertTrue(0b1010.hasMask(0b1000), "hasMask 0b1000 true");
		assertFalse(0b0000.hasMask(0b0001), "hasMask 0 false");
	}

	// ---- divideInt ----
	static void testDivideInt()
	{
		assertEqual(3, 10.divideInt(3), "divideInt 10/3=3");
		assertEqual(0, 10.divideInt(0), "divideInt by0=0");
		assertEqual(-1, 10.divideInt(0, -1), "divideInt by0 default -1");
		assertEqual(0, 2.divideInt(5), "divideInt 2/5=0");
	}

	// ---- getGreaterPow2 ----
	static void testGetGreaterPow2()
	{
		// "大于 value 的第一个 2^n"，1 自身是 2^0，下一个是 2
		assertEqual(2, 1.getGreaterPow2(), "getGreaterPow2 1->2");
		assertEqual(2, 2.getGreaterPow2(), "getGreaterPow2 2");
		assertEqual(4, 3.getGreaterPow2(), "getGreaterPow2 3->4");
		assertEqual(4, 4.getGreaterPow2(), "getGreaterPow2 3->4");
		assertEqual(8, 5.getGreaterPow2(), "getGreaterPow2 5->8");
		assertEqual(8, 8.getGreaterPow2(), "getGreaterPow2 8");
		assertEqual(16, 9.getGreaterPow2(), "getGreaterPow2 9->16");
	}

	// ---- replaceZ (Vector2->Vector3 / Vector3->Vector3) ----
	static void testReplaceZ()
	{
		Vector3 v3 = new Vector3(1, 2, 3);
		assertEqual(new Vector3(1, 2, 9), v3.replaceZ(9), "replaceZ V3");
		Vector2 v2 = new Vector2(1, 2);
		assertEqual(new Vector3(1, 2, 9), v2.replaceZ(9), "replaceZ V2->V3");
	}

	// ---- Helper: double -> float ----
	static float toFloat(this double value) { return (float)value; }

	// ---- getSquaredLength (V2/V3/V4) ----
	static void testSquaredLength()
	{
		assertTrue(new Vector2(3, 4).getSquaredLength().isEqual(25.0f, 0.001f), "sqLen V2 3-4");
		assertTrue(new Vector2(0, 0).getSquaredLength().isZero(), "sqLen V2 zero");
		assertTrue(new Vector3(1, 2, 2).getSquaredLength().isEqual(9.0f, 0.001f), "sqLen V3 1-2-2");
		assertTrue(new Vector4(1, 2, 2, 4).getSquaredLength().isEqual(25.0f, 0.001f), "sqLen V4 1-2-2-4");
	}

	// ---- lengthLess (strict <) ----
	static void testLengthLess()
	{
		// V2 vs float
		assertTrue(new Vector2(1, 1).lengthLess(2.0f), "lenLess V2 f true");
		assertFalse(new Vector2(3, 3).lengthLess(2.0f), "lenLess V2 f false");
		// V2 vs V2
		assertTrue(new Vector2(1, 1).lengthLess(new Vector2(2, 2)), "lenLess V2 V2 true");
		assertFalse(new Vector2(2, 2).lengthLess(new Vector2(1, 1)), "lenLess V2 V2 false");
		// V3 vs float
		assertTrue(new Vector3(1, 1, 1).lengthLess(2.0f), "lenLess V3 f true");
		assertFalse(new Vector3(3, 3, 3).lengthLess(2.0f), "lenLess V3 f false");
		// V3 vs V3
		assertTrue(new Vector3(1, 1, 1).lengthLess(new Vector3(2, 2, 2)), "lenLess V3 V3 true");
		assertFalse(new Vector3(2, 2, 2).lengthLess(new Vector3(1, 1, 1)), "lenLess V3 V3 false");
		// V4 vs float
		assertTrue(new Vector4(1, 1, 1, 1).lengthLess(3.0f), "lenLess V4 f true");
		assertFalse(new Vector4(3, 3, 3, 3).lengthLess(3.0f), "lenLess V4 f false");
	}

	// ---- lengthGreater (strict >) ----
	static void testLengthGreater()
	{
		// V2 vs float
		assertTrue(new Vector2(3, 3).lengthGreater(2.0f), "lenGreater V2 f true");
		assertFalse(new Vector2(1, 1).lengthGreater(2.0f), "lenGreater V2 f false");
		// V2 vs V2
		assertTrue(new Vector2(2, 2).lengthGreater(new Vector2(1, 1)), "lenGreater V2 V2 true");
		assertFalse(new Vector2(1, 1).lengthGreater(new Vector2(2, 2)), "lenGreater V2 V2 false");
		// V3 vs float
		assertTrue(new Vector3(3, 3, 3).lengthGreater(2.0f), "lenGreater V3 f true");
		assertFalse(new Vector3(1, 1, 1).lengthGreater(2.0f), "lenGreater V3 f false");
		// V3 vs V3
		assertTrue(new Vector3(2, 2, 2).lengthGreater(new Vector3(1, 1, 1)), "lenGreater V3 V3 true");
		assertFalse(new Vector3(1, 1, 1).lengthGreater(new Vector3(2, 2, 2)), "lenGreater V3 V3 false");
	}

	// ---- clamp (双边界) ----
	static void testClamp()
	{
		// float clamp
		assertTrue(5.0f.clamp(0.0f, 10.0f).isEqual(5.0f, 0.001f), "clamp f in range");
		assertTrue((-5.0f).clamp(0.0f, 10.0f).isEqual(0.0f, 0.001f), "clamp f below");
		assertTrue(15.0f.clamp(0.0f, 10.0f).isEqual(10.0f, 0.001f), "clamp f above");
		assertTrue(5.0f.clamp(10.0f, 0.0f).isEqual(10.0f, 0.001f), "clamp f reversed min/max");
		// int clamp
		assertEqual(5, 5.clamp(0, 10), "clamp int in range");
		assertEqual(0, (-5).clamp(0, 10), "clamp int below");
		assertEqual(10, 15.clamp(0, 10), "clamp int above");
		// long clamp
		assertEqual(5L, 5L.clamp(0L, 10L), "clamp long in range");
		assertEqual(0L, (-5L).clamp(0L, 10L), "clamp long below");
		assertEqual(10L, 15L.clamp(0L, 10L), "clamp long above");
	}

	// ---- inverse ----
	static void testInverse()
	{
		// float inverse
		assertTrue(2.0f.inverse().isEqual(0.5f, 0.001f), "inverse f 2");
		assertTrue(1.0f.inverse().isEqual(1.0f, 0.001f), "inverse f 1");
		assertTrue(0.5f.inverse().isEqual(2.0f, 0.001f), "inverse f 0.5");
		// int inverse (returns float)
		assertTrue(4.inverse().isEqual(0.25f, 0.001f), "inverse int 4");
		assertTrue(1.inverse().isEqual(1.0f, 0.001f), "inverse int 1");
		// double inverse
		assertTrue(2.0.inverse().isEqual(0.5, 0.00000001), "inverse double 2");
		assertTrue(0.5.inverse().isEqual(2.0, 0.00000001), "inverse double 0.5");
	}

	// ---- divide (8重载) ----
	static void testDivide()
	{
		// float / float
		assertTrue(10.0f.divide(3.0f).isEqual(3.33333f, 0.001f), "divide f/f");
		assertTrue(10.0f.divide(0.0f).isZero(), "divide f/0 default");
		assertTrue(10.0f.divide(0.0f, -1.0f).isEqual(-1.0f, 0.001f), "divide f/0 custom default");
		// int / int -> float
		assertTrue(10.divide(3).isEqual(3.33333f, 0.001f), "divide i/i");
		assertTrue(10.divide(0).isZero(), "divide i/0 default");
		// int / float -> float
		assertTrue(10.divide(3.0f).isEqual(3.33333f, 0.001f), "divide i/f");
		assertTrue(10.divide(0.0f).isZero(), "divide i/0f default");
		// long / float -> float
		assertTrue(10L.divide(3.0f).isEqual(3.33333f, 0.001f), "divide l/f");
		assertTrue(10L.divide(0.0f).isZero(), "divide l/0f default");
		// long / long -> float
		assertTrue(10L.divide(3L).isEqual(3.33333f, 0.001f), "divide l/l");
		assertTrue(10L.divide(0L).isZero(), "divide l/0 default");
		// double / double -> double
		assertTrue(10.0.divide(3.0).isEqual(3.33333, 0.0001), "divide d/d");
		assertTrue(10.0.divide(0.0).isZero(0.0001), "divide d/0 default");
		// Vector2 / Vector2
		Vector2 dv2 = new Vector2(10, 20).divide(new Vector2(2, 4));
		assertTrue(dv2.x.isEqual(5, 0.001f) && dv2.y.isEqual(5, 0.001f), "divide V2/V2");
		// Vector3 / Vector3
		Vector3 dv3 = new Vector3(10, 20, 30).divide(new Vector3(2, 4, 5));
		assertTrue(dv3.x.isEqual(5, 0.001f) && dv3.y.isEqual(5, 0.001f) && dv3.z.isEqual(6, 0.001f), "divide V3/V3");
		// Vector2 / float
		Vector2 dv2f = new Vector2(10, 20).divide(2.0f);
		assertTrue(dv2f.x.isEqual(5, 0.001f) && dv2f.y.isEqual(10, 0.001f), "divide V2/f");
		// Vector3 / float
		Vector3 dv3f = new Vector3(10, 20, 30).divide(2.0f);
		assertTrue(dv3f.x.isEqual(5, 0.001f) && dv3f.y.isEqual(10, 0.001f) && dv3f.z.isEqual(15, 0.001f), "divide V3/f");
	}

	// ---- inversePow10 (int pow) ----
	static void testInversePow10()
	{
		float inv0 = 0.inversePow10();
		assertTrue(inv0.isEqual(1.0f, 0.001f), "inversePow10 0=1");
		float inv1 = 1.inversePow10();
		assertTrue(inv1.isEqual(0.1f, 0.001f), "inversePow10 1=0.1");
		float inv2 = 2.inversePow10();
		assertTrue(inv2.isEqual(0.01f, 0.001f), "inversePow10 2=0.01");
		float inv3 = 3.inversePow10();
		assertTrue(inv3.isEqual(0.001f, 0.0001f), "inversePow10 3=0.001");
	}
}
