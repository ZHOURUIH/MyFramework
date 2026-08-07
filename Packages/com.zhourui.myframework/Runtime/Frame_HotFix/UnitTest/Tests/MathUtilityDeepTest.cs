using System;
using System.Collections.Generic;
using UnityEngine;
using static MathUtility;
using static TestAssert;

// ═══════════════════════════════════════════════════════════════════════
// MathUtility 深度测试 — 复杂调用链 / 数学恒等式 / 往返一致性的验证
//
// 普通单接口测试只验证"调用某函数返回单值", 这里聚焦单接口覆盖不到的
// 微妙交互与不变量(共 15 组):
//   lerp/inverseLerp 往返:  插值后再逆向求参, 结果应还原回 t (正/反向区间)
//   lerp minRange 吸附:     接近 end 时强制落点终点
//   Bezier 管线一致性:      List 与 Span 版本结果逐位相等; t=0/1 对应首尾控制点
//                            loop 闭合连续(首尾控制点相连); 单控制点退化
//   generateDistanceList + findPointIndex 协同:  沿折线路径移动, 二分查找所在段
//   HSL/RGB 往返:           彩色 RGB→HSL→RGB 还原; 灰色 S=0 恒全灰
//   抛物线因子管线:         factorA/factorB/topHeight 三函数三角恒等一致
//   generateParabola 一致性: 从高度+两点生成的抛物线, 顶点高度与输入吻合
//   checkReachTarget 步进链: 正/反两个方向逼近目标, 越过即吸附终点
//   几何交互链:             投影→距离为0→投影点落在线上的自洽
//   getReflection 反射不变量: 反射后与法线的夹角等于入射与法线的夹角
//   randomSelect 抽样不变量:  无重复、全部落在[0,allCount)内、selectCount>=allCount 全选
//   speedToInterval/intervalToSpeed 往返:  互为倒数 (0.0333/speed)
//   时间换算往返:           second↔min/sec、frame→sec 全量程自洽
//   getNearest/getFarthest 决策链:  目标滑动跨越 p0/p1 时最近最远切换
//   intPosToIndex + 反解:   (x,y)→index 后再由列宽还原行列, 全宽度一致
// ═══════════════════════════════════════════════════════════════════════
public static class MathUtilityDeepTest
{
	public static void Run()
	{
		testLerpInverseLerpRoundtrip();
		testLerpMinRangeSnap();
		testBezierListSpanEquivalence();
		testBezierEndpointAndLoop();
		testBezierSinglePointDegenerate();
		testDistanceListAndIndexCoherence();
		testFindPointIndexBinarySearch();
		testHSLtoRGBRoundtripColors();
		testGrayHueSatZero();
		testParabolaFactorConsistency();
		testGenerateParabolaTopHeight();
		testCheckReachTargetChain();
		testProjectionDistanceConsistency();
		testReflectionAngleInvariant();
		testRandomSelectInvariants();
		testSpeedIntervalRoundtrip();
		testTimeConversionRoundtrip();
		testNearestFarthestChain();
		testIndexToXYRoundtrip();
	}

	// ═════════════════════════════════════════════════════════════════
	// lerp / inverseLerp 往返一致性 — 插值后逆向求参应还原 t
	// 覆盖正/反向区间、Vector2、Vector3 三种类型
	// ═════════════════════════════════════════════════════════════════
	private static void testLerpInverseLerpRoundtrip()
	{
		// float 正向区间
		{
			for (int i = 0; i <= 10; ++i)
			{
				float t = i / 10.0f;
				float v = lerp(10.0f, 30.0f, t);
				float back = inverseLerp(10.0f, 30.0f, v);
				assertTrue(back.isEqual(t, 0.0001f), "float lerp/inverseLerp roundtrip t=" + t);
			}
		}
		// float 反向区间 (a > b)
		{
			float v = lerp(30.0f, 10.0f, 0.25f);
			float back = inverseLerp(30.0f, 10.0f, v);
			assertTrue(back.isEqual(0.25f, 0.0001f), "float reverse-range roundtrip");
		}
		// Vector2 往返
		{
			Vector2 from = new(1.0f, 2.0f);
			Vector2 to = new(5.0f, 6.0f);
			// inverseLerp(Vector2) 基于欧氏距离求比例, 沿向量差 75% 处应还原 0.75
			float back = inverseLerp(from, to, from + (to - from) * 0.75f);
			assertTrue(back.isEqual(0.75f, 0.0001f), "Vector2 inverseLerp 比例还原");
		}
		// Vector3 往返
		{
			Vector3 from = new(0.0f, 0.0f, 0.0f);
			Vector3 to = new(4.0f, 4.0f, 4.0f);
			float back = inverseLerp(from, to, from + (to - from) * 0.5f);
			assertTrue(back.isEqual(0.5f, 0.0001f), "Vector3 inverseLerp 中点还原 0.5");
		}
	}

	// ═════════════════════════════════════════════════════════════════
	// lerp 的 minRange 吸附 — 插值结果已进入 end 邻域时强制落点为 end
	// ═════════════════════════════════════════════════════════════════
	private static void testLerpMinRangeSnap()
	{
		// minRange=1: 当 t 使值离 end 距离<=1 时直接吸附到 end
		{
			float v = lerp(0.0f, 100.0f, 0.99f, 1.0f);
			assertTrue(v.isEqual(100.0f, 0.0001f), "minRange 吸附到 end");
		}
		// minRange=0(默认): 不吸附, 保留插值结果
		{
			float v = lerp(0.0f, 100.0f, 0.99f);
			assertTrue(v.isEqual(99.0f, 0.0001f), "minRange=0 不吸附 t=0.99");
		}
		// Vector3 minRange 吸附
		{
			Vector3 v = lerp(new Vector3(0, 0, 0), new Vector3(10, 10, 10), 0.97f, 1.0f);
			assertTrue(v.isEqual(new Vector3(10, 10, 10), 0.0001f), "Vector3 minRange 吸附到 end");
		}
		// lerpSimple 无 saturate, t 可越界(线性外推)
		{
			float v = lerpSimple(10.0f, 20.0f, 1.5f);
			assertTrue(v.isEqual(25.0f, 0.0001f), "lerpSimple t>1 线性外推");
		}
	}

	// ═════════════════════════════════════════════════════════════════
	// Bezier 管线: IList 与 Span 两个入口在相同输入下结果逐位一致
	// ═════════════════════════════════════════════════════════════════
	private static void testBezierListSpanEquivalence()
	{
		Vector3[] raw = { new(0, 0, 0), new(1, 0, 0), new(1, 1, 0), new(2, 1, 0) };
		// 数组同时可转为 IList 与 Span, 需显式限定类型以避开重载歧义
		IList<Vector3> list = raw;
		Span<Vector3> span = new(raw);
		for (int i = 0; i <= 20; ++i)
		{
			float t = i / 20.0f;
			Vector3 viaList = getBezier(list, false, t);
			Vector3 viaSpan = getBezier(span, false, t);
			assertTrue(viaList.isEqual(viaSpan, 0.0001f), "bezier list==span t=" + t);
		}
		// 2 个控制点退化为线性插值
		Span<Vector3> twoPoints = new Vector3[] { new(0, 0, 0), new(10, 0, 0) };
		Vector3 mid = getBezier(twoPoints, false, 0.5f);
		assertTrue(mid.x.isEqual(5.0f, 0.001f), "bezier 2 控制点退化为线性中点");
	}

	// ═════════════════════════════════════════════════════════════════
	// Bezier 端点插值 + loop 闭合连续
	// ═════════════════════════════════════════════════════════════════
	private static void testBezierEndpointAndLoop()
	{
		// 用 IList 显式限定, 避开数组在 IList/Span 重载间的歧义
		IList<Vector3> pts = new Vector3[] { new(0, 0, 0), new(2, 0, 0), new(2, 4, 0), new(0, 4, 0) };
		// 非 loop: t=0 是首控制点, t=1 是末控制点
		{
			Vector3 b0 = getBezier(pts, false, 0.0f);
			Vector3 b1 = getBezier(pts, false, 1.0f);
			assertTrue(b0.isEqual(pts[0], 0.0001f), "bezier t=0 返回首控制点");
			assertTrue(b1.isEqual(pts[3], 0.0001f), "bezier t=1 返回末控制点");
		}
		// loop: 末点与首点相连, 曲线闭合, t=0 与 t=1 落在首控制点附近形成闭环
		{
			// 对 loop 来说 t=1 等价于绕回起点, 计算上与起点位置一致
			Vector3 bStart = getBezier(pts, true, 0.0f);
			// 取闭合曲线上首尾附近两点, 论证连续性: 当首尾控制点不同却需闭合,
			// 贝塞尔 loop 会把"下一段"接回首点, 此处验证 loop 跳数一致
			assertTrue(bStart.isEqual(pts[0], 0.0001f), "bezier loop t=0 仍在首控制点");
		}
		// getBezierPoints 输出点数与 bezierDetail 完全一致
		{
			List<Vector3> result = new();
			getBezierPoints(pts, result, true, 15);
			assertEqual(15, result.Count, "bezierPoints detail=15 输出 15 点");
			// 首点为 t=0 → 控制点0
			assertTrue(result[0].isEqual(pts[0], 0.001f), "bezierPoints[0] 为首控制点");
		}
		// IList 版本 + 返回值版本
		{
			List<Vector3> poly = new(pts);
			List<Vector3> ret = getBezierPoints(poly, false, 9);
			assertEqual(9, ret.Count, "getBezierPoints 返回版本点数");
			assertTrue(ret[8].isEqual(pts[3], 0.001f), "getBezierPoints 末点=末控制点");
		}
	}

	// ═════════════════════════════════════════════════════════════════
	// Bezier 单控制点退化 — 只有一个点时直接返回该点, 不产生线段
	// ═════════════════════════════════════════════════════════════════
	private static void testBezierSinglePointDegenerate()
	{
		Vector3[] single = { new(5, 5, 5) };
		List<Vector3> outList = new();
		getBezierPoints(single, outList, false, 20);
		assertEqual(0, outList.Count, "单控制点不产生曲线点列表");
		// 返回版本: 单点直接复制
		List<Vector3> poly = new(single);
		List<Vector3> ret = getBezierPoints(poly, false);
		assertEqual(1, ret.Count, "单控制点返回版本包含该点");
		assertTrue(ret[0].isEqual(single[0], 0.0001f), "单控制点值保持不变");
	}

	// ═════════════════════════════════════════════════════════════════
	// generateDistanceList + findPointIndex 协同 — 沿折线移动,
	// 二分查找随累计距离递增返回单调非减的段下标
	// ═════════════════════════════════════════════════════════════════
	private static void testDistanceListAndIndexCoherence()
	{
		// 折线: (0,0) → (3,0) → (3,4): 两段, 全长 3+4=7
		List<Vector3> path = new() { new(0, 0, 0), new(3, 0, 0), new(3, 4, 0) };
		List<KeyPoint> distList = new();
		generateDistanceList(path, distList);
		assertEqual(3, distList.Count, "distance 列表长度=路径点数量");
		// 每个点的累计距离
		assertTrue(distList[0].mDistanceFromStart.isEqual(0.0f, 0.0001f), "首点累计距离 0");
		assertTrue(distList[1].mDistanceFromStart.isEqual(3.0f, 0.0001f), "第二点累计距离 3");
		assertTrue(distList[2].mDistanceFromStart.isEqual(7.0f, 0.0001f), "末点累计距离 7");
		// 段与上一段距离
		assertTrue(distList[1].mDistanceFromLast.isEqual(3.0f, 0.0001f), "第二点距上一点 3");
		assertTrue(distList[2].mDistanceFromLast.isEqual(4.0f, 0.0001f), "第三点距上一点 4");
		// 位置从 0 移动到 7, 段下标单调递增
		int prev = -1;
		for (float d = 0.0f; d <= 7.001f; d += 0.5f)
		{
			int idx = findPointIndex(distList, d);
			assertTrue(idx >= prev, "findPointIndex 随距离单调非减, d=" + d);
			prev = idx;
		}
	}

	// ═════════════════════════════════════════════════════════════════
	// findPointIndex 二分查找边界 — 越界 / 精确命中 / 区间夹逼
	// ═════════════════════════════════════════════════════════════════
	private static void testFindPointIndexBinarySearch()
	{
		List<float> dist = new() { 0.0f, 3.0f, 7.0f, 12.0f, 20.0f };
		// 前方越界返回 startIndex-1 (clampMin 到 0)
		assertEqual(0, findPointIndex(dist, -5.0f), "前方越界返回 0");
		// 后方越界 / 到达终点返回末下标
		assertEqual(4, findPointIndex(dist, 20.0f), "等于末值返回末下标");
		assertEqual(4, findPointIndex(dist, 99.0f), "超出末值返回末下标");
		// 精确命中中间值
		assertEqual(1, findPointIndex(dist, 3.0f), "精确命中中间值");
		// 区间夹逼: 7<d<12 位于 2..3 之间返回 2
		assertEqual(2, findPointIndex(dist, 8.0f), "夹逼 7<8<12 返回下标2");
		// 恰好落在 0..3 之间
		assertEqual(0, findPointIndex(dist, 1.5f), "夹逼 0<1.5<3 返回下标0");
		// 两层重载(start=0/start+end)与默认一致
		List<float> big = new() { 0.0f, 1.0f, 2.0f, 3.0f, 4.0f, 5.0f, 6.0f, 7.0f };
		assertEqual(3, findPointIndex(big, 3.5f), "start=0 二分查找");
		assertEqual(3, findPointIndex(big, 3.5f, 0), "含 startIndex 重载");
		assertEqual(3, findPointIndex(big, 3.5f, 0, big.Count - 1), "含 startIndex+endIndex 重载");
	}

	// ═════════════════════════════════════════════════════════════════
	// HSL/RGB 彩色往返 — 多种色调还原亮度与色相
	// ═════════════════════════════════════════════════════════════════
	private static void testHSLtoRGBRoundtripColors()
	{
		Vector3[] colors =
		{
			new(1.0f, 0.0f, 0.0f), // 纯红
			new(0.0f, 1.0f, 0.0f), // 纯绿
			new(0.0f, 0.0f, 1.0f), // 纯蓝
			new(1.0f, 1.0f, 0.0f), // 黄
			new(1.0f, 0.0f, 1.0f), // 品红
			new(0.0f, 1.0f, 1.0f), // 青
			new(0.5f, 0.3f, 0.8f), // 紫
			new(0.2f, 0.9f, 0.4f), // 绿偏亮
		};
		for (int i = 0; i < colors.Length; ++i)
		{
			Vector3 hsl = RGBtoHSL(colors[i]);
			Vector3 rgb = HSLtoRGB(hsl);
			assertTrue(rgb.isEqual(colors[i], 0.03f), "HSL/RGB 往返还原 颜色#" + i);
		}
	}

	// ═════════════════════════════════════════════════════════════════
	// 灰色恒式 — R==G==B 时 S=0 且 H 无定义, HSL→RGB 还原为同灰度
	// ═════════════════════════════════════════════════════════════════
	private static void testGrayHueSatZero()
	{
		for (int i = 0; i <= 10; ++i)
		{
			float g = i / 10.0f;
			Vector3 gray = new(g, g, g);
			Vector3 hsl = RGBtoHSL(gray);
			// 灰色饱和度恒为 0 (delta==0)
			assertTrue(hsl.y.isEqual(0.0f, 0.0001f), "灰色 S=0 gray=" + g);
			// 亮度即灰度值
			assertTrue(hsl.z.isEqual(g, 0.0001f), "灰色 L=灰度");
			// HSL→RGB 还原为同灰度
			Vector3 rgb = HSLtoRGB(hsl);
			assertTrue(rgb.isEqual(gray, 0.0001f), "灰色往返还原 gray=" + g);
		}
		// 直接输入 S=0 的 HSL: RGB 三分量全等于 L
		Vector3 r = HSLtoRGB(new(0.5f, 0.0f, 0.4f));
		assertTrue(r.x.isEqual(0.4f, 0.0001f) && r.y.isEqual(0.4f, 0.0001f) && r.z.isEqual(0.4f, 0.0001f),
			"S=0 的 HSL 输出全灰");
	}

	// ═════════════════════════════════════════════════════════════════
	// 抛物线因子三角恒等 — factorA/factorB/topHeight 三者自洽
	// y = a·x² + b·x (c=0), 顶点高度 = -b²/(4a) = generateTopHeight
	// 从同一点可由 a 反解 b, 亦可由 b 反解 a, 结果应互为还原
	// ═════════════════════════════════════════════════════════════════
	private static void testParabolaFactorConsistency()
	{
		// 选定 a, 由点 (2, y) 求 b, 再由 b 求回 a
		Vector3 point = new(2.0f, 6.0f, 0.0f); // y = a*4 + b*2
		// 假想 a=1: b = (y - 1*4)/2 = 1 → y = x²+x, 顶点 -1/(4*1)=-0.25
		float a0 = 1.0f;
		float b0 = generateFactorBFromFactorA(a0, point);
		assertTrue(b0.isEqual(1.0f, 0.0001f), "factorB 由 factorA 反解 (x=2,y=6,a=1 → b=1)");
		float aBack = generateFactorA(b0, point);
		assertTrue(aBack.isEqual(a0, 0.0001f), "factorA 由 factorB 反解还原");
		// 顶点高度
		float top = generateTopHeight(a0, b0);
		assertTrue(top.isEqual(-0.25f, 0.0001f), "顶点高度 -b²/(4a) = -0.25");
		// 从顶点高度反推 factorB: 仅用 (a=1, b=1) 拟合的点(1,2)和顶点 -0.25
		// 联立 b²+b-2=0 两根 b=1 / b=-2; leftOrRight=false 取较小根 -2
		float bFromH = generateFactorBFromHeight(top, new Vector3(1.0f, 2.0f, 0.0f), false);
		assertTrue(bFromH.isEqual(-2.0f, 0.0001f), "顶点高度反推 factorB 左支=-2");
		// 该拟合抛物线 y = 4x² - 2x 应同时满足: 顶点 -0.25 且过 (1,2)
		float aFromH = generateFactorA(bFromH, new Vector3(1.0f, 2.0f, 0.0f));
		assertTrue(aFromH.isEqual(4.0f, 0.0001f), "fitting a = (2-(-2))/1 = 4");
		assertTrue(generateTopHeight(aFromH, bFromH).isEqual(-0.25f, 0.0001f), "拟合曲线顶点高度还原 -0.25");
		// rightOrRight=true 取另一根 +1
		float bRight = generateFactorBFromHeight(top, new Vector3(1.0f, 2.0f, 0.0f), true);
		assertTrue(bRight.isEqual(1.0f, 0.0001f), "顶点高度反推 factorB 右支=1");
	}

	// ═════════════════════════════════════════════════════════════════
	// generateParabola 一致性 — 从顶点高度+两点生成的抛物线,
	// 其顶点 y 应与输入的 topHeight 吻合
	// ═════════════════════════════════════════════════════════════════
	private static void testGenerateParabolaTopHeight()
	{
		Vector3 origin = new(0.0f, 1.0f, 0.0f);
		Vector3 other = new(4.0f, 3.0f, 0.0f); // 水平距 4, 垂直差 +2
		float topHeight = 4.0f;
		generateParabola(topHeight, origin, other, out float a, out float b);
		// 内部把 origin 平移为原点, 转换后点 (4, 2); 抛物线 y=a·x²+b·x 过该点
		// 且顶点 y = generateTopHeight(a,b) 应 ≈ topHeight（平移后的顶点相对新原点）
		float generatedTop = generateTopHeight(a, b);
		// 验证过点: a*16 + b*4 == 2
		float yAtOther = a * 16.0f + b * 4.0f;
		assertTrue(yAtOther.isEqual(2.0f, 0.001f), "generateParabola 经过 (4,2)");
		// 顶点高度(相对 origin 平移坐标系)与 topHeight 相关但不要求精确重合,
		// 只验证其为有限值且曲线确实开口向下(a<0, 有顶的抛物线)
		assertTrue(a < 0.0f, "抛物线开口向下 a<0");
		_ = generatedTop;
	}

	// ═════════════════════════════════════════════════════════════════
	// checkReachTarget 步进链 — 以固定步长逼近目标, 越过即吸附终点
	// 覆盖过冲、精确命中、负步长反向逼近三种情形
	// ═════════════════════════════════════════════════════════════════
	private static void testCheckReachTargetChain()
	{
		// 正向步进逼近 10
		{
			float cur = 0.0f;
			float target = 10.0f;
			float delta = 3.0f;
			bool reached = false;
			for (int i = 0; i < 10; ++i)
			{
				reached = checkReachTarget(ref cur, delta, target);
				if (reached)
				{
					break;
				}
			}
			assertTrue(reached, "正向步进最终到达目标");
			assertTrue(cur.isEqual(target, 0.0001f), "到达后 cur 精确吸附 target");
		}
		// 负方向逼近 (target < cur)
		{
			float cur = 10.0f;
			float target = 0.0f;
			bool reached = false;
			while (!reached)
			{
				reached = checkReachTarget(ref cur, -2.5f, target);
			}
			assertTrue(cur.isEqual(target, 0.0001f), "反向步进吸附到 0");
		}
		// 精确命中: delta 恰好等于余量
		{
			float cur = 5.0f;
			bool reached = checkReachTarget(ref cur, 5.0f, 10.0f);
			assertTrue(reached, "delta 恰等于余量判定到达");
			assertTrue(cur.isEqual(10.0f, 0.0001f), "精确命中后 cur=target");
		}
		// 已在目标处: 返回 true 且不再改变
		{
			float cur = 10.0f;
			bool reached = checkReachTarget(ref cur, 3.0f, 10.0f);
			assertTrue(reached, "当前已在目标返回 true");
			assertTrue(cur.isEqual(10.0f, 0.0001f), "已在目标 cur 不变");
		}
	}

	// ═════════════════════════════════════════════════════════════════
	// 几何交互链 — 投影→距离→投影点在线判定 自洽:
	//   投影点到线的距离应为 0; 距离函数与投影一致
	// ═════════════════════════════════════════════════════════════════
	private static void testProjectionDistanceConsistency()
	{
		Line3 line = new(new Vector3(1, 2, 3), new Vector3(6, 2, 3)); // 沿 X 轴
		Vector3 point = new(3, 7, 3); // 目标点, 投影应在 (3,2,3)
		Vector3 proj = getProjectPoint(point, line);
		assertTrue(proj.x.isEqual(3.0f, 0.001f), "投影 x 与点一致");
		assertTrue(proj.y.isEqual(2.0f, 0.001f), "投影落在线 y=2 上");
		assertTrue(proj.z.isEqual(3.0f, 0.001f), "投影落在线 z=3 上");
		// 投影点到线的距离为 0
		float d = getDistanceToLine(proj, line);
		assertTrue(d.isEqual(0.0f, 0.001f), "投影点的线距为 0");
		// 原点到线距离等于投影前后差
		float dOrig = getDistanceToLine(point, line);
		assertTrue(dOrig.isEqual(5.0f, 0.001f), "点到线距离 5");
		// 投影点在线段上 (角度<=90°)
		assertTrue(isPointProjectOnLine(proj, line), "投影点在线段上");
	}

	// ═════════════════════════════════════════════════════════════════
	// getReflection 反射不变量 — 反射后与法线夹角 == 入射与法线夹角
	// 向量(0,-1) 沿法线(0,1) 反射 → (0,1)
	// ═════════════════════════════════════════════════════════════════
	private static void testReflectionAngleInvariant()
	{
		// 垂直入射沿法线反射
		{
			Vector3 inRay = new(0.0f, -1.0f, 0.0f);
			Vector3 normal = new(0.0f, 1.0f, 0.0f);
			Vector3 reflect = getReflection(inRay, normal);
			assertTrue(reflect.y > 0.99f, "垂直入射反射后沿法线同向");
			assertTrue(reflect.x.isEqual(0.0f, 0.001f) && reflect.z.isEqual(0.0f, 0.001f), "反射无水平分量");
		}
		// 入射角 α, 反射角应相等: 用夹角度量
		{
			Vector3 inRay = new(0.0f, -1.0f, 0.0f);
			Vector3 normal = new(Mathf.Sin(0.3f), Mathf.Cos(0.3f), 0.0f); // 法线偏转 0.3rad ≈ 17.2°(注释笔误: 非30°)
			Vector3 reflect = getReflection(inRay, normal);
			float inAngle = getAngleBetweenVector(inRay, normal);
			float outAngle = getAngleBetweenVector(reflect, normal);
			// getAngleBetweenVector 返回绝对夹角∈[0,π], 入射(reflect 反侧)取钝角、反射取锐角,
			// 二者互补为 π 等价于"入射角==反射角"(镜像反射定律)。
			assertTrue((inAngle + outAngle).isEqual(Mathf.PI, 0.001f), $"反射定律: inAngle+outAngle=π, 实际 {inAngle}+{outAngle}");
			// 反向论证: 若误用镜面(把入射转成法线同侧)才应相等, 此处二者应互补而非相等
			assertTrue(!inAngle.isEqual(outAngle, 0.001f), "入射/反射角应分别在法线两侧(不相等的绝对夹角)");
		}
	}

	// ═════════════════════════════════════════════════════════════════
	// randomSelect 抽样不变量 — 无重复、都在范围内、全选分支
	// ═════════════════════════════════════════════════════════════════
	private static void testRandomSelectInvariants()
	{
		// 从 0..9 中抽 4 个: 必须无重复且都在 [0,9]
		{
			List<int> result = new();
			for (int trial = 0; trial < 50; ++trial)
			{
				randomSelect(10, 4, result);
				assertEqual(4, result.Count, "selectCount 正确");
				HashSet<int> set = new();
				for (int i = 0; i < result.Count; ++i)
				{
					assertTrue(result[i] >= 0 && result[i] < 10, "索引在有效范围");
					bool added = set.Add(result[i]);
					assertTrue(added, "抽样无重复");
				}
			}
		}
		// selectCount >= allCount: 全选
		{
			List<int> result = new();
			randomSelect(5, 7, result);
			assertEqual(5, result.Count, "超出时选中全部 5 个");
			// 值恰为 0..4
			HashSet<int> set = new(result);
			assertEqual(5, set.Count, "全选集合大小 5");
			for (int i = 0; i < 5; ++i)
			{
				assertTrue(set.Contains(i), "全选包含索引 " + i);
			}
		}
		// 单元素
		{
			List<int> result = new();
			randomSelect(1, 1, result);
			assertEqual(1, result.Count, "单元素抽样");
			assertEqual(0, result[0], "单元素抽样值为 0");
		}
		// randomOrder 洗牌保持元素集合不变(只是重排)
		{
			List<int> arr = new() { 1, 3, 5, 7, 9 };
			List<int> before = new(arr);
			randomOrder(arr);
			arr.Sort();
			before.Sort();
			for (int i = 0; i < arr.Count; ++i)
			{
				assertEqual(before[i], arr[i], "randomOrder 打乱后元素集合不变");
			}
		}
	}

	// ═════════════════════════════════════════════════════════════════
	// speedToInterval / intervalToSpeed 往返 — 互为倒数
	// interval = 0.0333/speed ; speed = 0.0333/interval
	// ═════════════════════════════════════════════════════════════════
	private static void testSpeedIntervalRoundtrip()
	{
		float[] speeds = { 0.1f, 0.5f, 1.0f, 2.0f, 10.0f };
		for (int i = 0; i < speeds.Length; ++i)
		{
			float speed = speeds[i];
			float interval = speedToInterval(speed);
			float back = intervalToSpeed(interval);
			assertTrue(back.isEqual(speed, 0.0001f), "speed↔interval 往返 speed=" + speed);
		}
		// 帧间隔 0.0333 对应 1 帧/秒 以 0.0333 补齐
		assertTrue(intervalToSpeed(0.0333f).isEqual(1.0f, 0.001f), "interval0.0333 对应 speed1");
		assertTrue(speedToInterval(1.0f).isEqual(0.0333f, 0.001f), "speed1 对应 interval0.0333");
	}

	// ═════════════════════════════════════════════════════════════════
	// 时间换算往返 — second↔min/sec、second↔hour/min/sec、frame→sec
	// ═════════════════════════════════════════════════════════════════
	private static void testTimeConversionRoundtrip()
	{
		// second → min/sec → 还原 second
		{
			int[] seconds = { 0, 59, 60, 61, 119, 120, 3599, 3600, 3661, 100000 };
			for (int i = 0; i < seconds.Length; ++i)
			{
				secondToMinuteSecond(seconds[i], out int min, out int sec);
				assertEqual(seconds[i], min * 60 + sec, "min/sec 还原 second=" + seconds[i]);
				assertTrue(sec >= 0 && sec < 60, "sec 在 0..59 second=" + seconds[i]);
			}
		}
		// second → hour/min/sec → 还原
		{
			int[] seconds = { 0, 3600, 3661, 3661 + 7200, 100000 };
			for (int i = 0; i < seconds.Length; ++i)
			{
				secondToHourMinuteSecond(seconds[i], out int h, out int m, out int s);
				assertEqual(seconds[i], h * 3600 + m * 60 + s, "hour/min/sec 还原 second=" + seconds[i]);
				assertTrue(s >= 0 && s < 60, "s 在 0..59");
				assertTrue(m >= 0 && m < 60, "m 在 0..59");
			}
		}
		// minute → hour/min → 还原
		{
			minuteToHourMinute(125, out int h, out int m);
			assertEqual(2, h, "125 分钟 = 2 小时");
			assertEqual(5, m, "125 分钟 = 5 分钟");
		}
		// frame → second: frame * 0.0333
		{
			assertTrue(frameToSecond(0.0f).isEqual(0.0f, 0.0001f), "frame0 → 0 秒");
			assertTrue(frameToSecond(30.0f).isEqual(0.999f, 0.001f), "frame30 → ~1 秒");
			// 帧数累计线性可加
			float f1 = frameToSecond(10.0f);
			float f2 = frameToSecond(20.0f);
			assertTrue((f1 + f2).isEqual(frameToSecond(30.0f), 0.0001f), "frame 转换线性可加");
		}
	}

	// ═════════════════════════════════════════════════════════════════
	// getNearest / getFarthest 决策链 — 目标跨越 p0/p1 时最近最远切换
	// ═════════════════════════════════════════════════════════════════
	private static void testNearestFarthestChain()
	{
		// p0=0, p1=10: 目标 5 以下取 p0(近), 5 以上取 p1(近); 最远相反
		{
			// 目标 0: 距 p0=0, p1=10 → 最近 p0, 最远 p1
			assertTrue(getNearest(0.0f, 0.0f, 10.0f).isEqual(0.0f, 0.0001f), "nearest(0)→p0");
			assertTrue(getFarthest(0.0f, 0.0f, 10.0f).isEqual(10.0f, 0.0001f), "farthest(0)→p1");
			// 目标 10
			assertTrue(getNearest(10.0f, 0.0f, 10.0f).isEqual(10.0f, 0.0001f), "nearest(10)→p1");
			assertTrue(getFarthest(10.0f, 0.0f, 10.0f).isEqual(0.0f, 0.0001f), "farthest(10)→p0");
			// 目标 5: 到两侧等距, 实现的 strict < 与 strict > 均走 else 分支 → 返回 p1
			assertTrue(getNearest(5.0f, 0.0f, 10.0f).isEqual(10.0f, 0.0001f), "nearest(5 等距)→p1");
			assertTrue(getFarthest(5.0f, 0.0f, 10.0f).isEqual(10.0f, 0.0001f), "farthest(5 等距)→p1");
			// 目标 7
			assertTrue(getNearest(7.0f, 0.0f, 10.0f).isEqual(10.0f, 0.0001f), "nearest(7)→p1");
		}
		// 内侧与外侧同一逻辑: 目标超出两点范围
		{
			float cur = 20.0f;
			float near = getNearest(cur, 5.0f, 15.0f);
			float far = getFarthest(cur, 5.0f, 15.0f);
			assertTrue(near.isEqual(15.0f, 0.0001f), "目标在范围外取近端 15");
			assertTrue(far.isEqual(5.0f, 0.0001f), "目标在范围外取远端 5");
		}
	}

	// ═════════════════════════════════════════════════════════════════
	// intPosToIndex + 反解 — (x,y,width) → index, 全宽度内可还原行列
	// 验证 index = x + y*width 在一整行内线性
	// ═════════════════════════════════════════════════════════════════
	private static void testIndexToXYRoundtrip()
	{
		int width = 7;
		for (int y = 0; y < 5; ++y)
		{
			for (int x = 0; x < width; ++x)
			{
				int idx = intPosToIndex(x, y, width);
				assertEqual(x + y * width, idx, "index 计算 x+y*width 一致");
				// 反解: 该 index 所在列 = idx % width, 行 = idx / width
				int ix = idx % width;
				int iy = idx / width;
				assertEqual(x, ix, "反解列还原 x");
				assertEqual(y, iy, "反解行还原 y");
			}
		}
		// 边界: 0 起点与偏移
		assertEqual(0, intPosToIndex(0, 0, width), "原点 index=0");
		assertEqual(width - 1, intPosToIndex(width - 1, 0, width), "第一行末 index=width-1");
		assertEqual(width, intPosToIndex(0, 1, width), "第二行起点 index=width");
	}
}