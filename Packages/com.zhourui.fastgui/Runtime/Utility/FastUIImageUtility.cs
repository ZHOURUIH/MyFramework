using UnityEngine;
using UnityEngine.Sprites;

// FastImage的纯几何计算工具。
// 所有Image类型最终只生成Position/UV，不依赖FastUI专用Shader，也不向Material注入隐藏协议。
// Sliced/Tiled通过真实多Quad几何实现；Filled Radial通过矩形与角度扇区求交后生成退化Quad三角形。
// Radial使用固定最大三角单元数量，fillAmount变化只改Position/UV，不改变Index拓扑。
public static class FastUIImageUtility
{
	private const float EPSILON = 0.0001f;
	private const float DEFAULT_REFERENCE_PIXELS_PER_UNIT = 100.0f;
	private const int MAX_TILED_AXIS_COUNT = 256;
	private const int MAX_TILED_QUAD_COUNT = 4096;
	// Radial最多只需要起点、终点和4个矩形角点。固定Scratch数组避免为这几个临时点引入List或ECS结构变化。
	private static readonly FastUIRadialPoint[] sRadialPointBuffer = new FastUIRadialPoint[8];
	private static int sRadialPointCount;
	private struct FastUIRadialPoint
	{
		public Vector2 mPosition;
		public float mProgress;
		public FastUIRadialPoint(Vector2 position, float progress)
		{
			mPosition = position;
			mProgress = progress;
		}
	}
	public static Rect getOuterUVRect(Sprite sprite)
	{
		if (sprite == null)
		{
			return new Rect(0.0f, 0.0f, 1.0f, 1.0f);
		}
		Vector4 outer = DataUtility.GetOuterUV(sprite);
		return new Rect(outer.x, outer.y, outer.z - outer.x, outer.w - outer.y);
	}
	public static Rect getInnerUVRect(Sprite sprite)
	{
		if (sprite == null)
		{
			return new Rect(0.0f, 0.0f, 1.0f, 1.0f);
		}
		Vector4 inner = DataUtility.GetInnerUV(sprite);
		return new Rect(inner.x, inner.y, inner.z - inner.x, inner.w - inner.y);
	}
	public static Vector2 getNativeSize(Sprite sprite)
	{
		if (sprite == null)
		{
			return Vector2.zero;
		}
		// 与UGUI Image.SetNativeSize一致：NativeSize使用Sprite本身的PPU，不受Sliced/Tiled的PixelsPerUnitMultiplier影响。
		return sprite.rect.size / getBasePixelsPerUnit(sprite);
	}
	public static int clampFillOrigin(FastUIImageFillMethod method, int origin)
	{
		int maxOrigin = method == FastUIImageFillMethod.Horizontal || method == FastUIImageFillMethod.Vertical ? 1 : 3;
		return Mathf.Clamp(origin, 0, maxOrigin);
	}
	// Sliced Fill only has linear semantics. Radial methods are intentionally
	// treated as Horizontal so switching from Filled -> Sliced never produces
	// undefined geometry from an old serialized radial setting.
	public static FastUIImageFillMethod getSlicedFillMethod(FastUIImageFillMethod method)
	{
		return method == FastUIImageFillMethod.Vertical
			? FastUIImageFillMethod.Vertical
			: FastUIImageFillMethod.Horizontal;
	}
	public static int clampSlicedFillOrigin(int origin)
	{
		return Mathf.Clamp(origin, 0, 1);
	}
	// Sliced Fill is implemented by shrinking the logical target Rect first and
	// then rebuilding the 9-slice inside that Rect. This preserves both end borders
	// (for example rounded progress-bar caps) instead of merely clipping the full
	// width sliced mesh and losing the far cap until FillAmount reaches 1.
	public static Rect getSlicedFillRect(Rect rect, FastUIImageFillMethod method, float amount, int origin)
	{
		amount = Mathf.Clamp01(amount);
		method = getSlicedFillMethod(method);
		origin = clampSlicedFillOrigin(origin);
		if (amount >= 1.0f - EPSILON)
		{
			return rect;
		}
		if (method == FastUIImageFillMethod.Vertical)
		{
			float height = rect.height * amount;
			if (origin == 1)
			{
				return Rect.MinMaxRect(rect.xMin, rect.yMax - height, rect.xMax, rect.yMax);
			}
			return Rect.MinMaxRect(rect.xMin, rect.yMin, rect.xMax, rect.yMin + height);
		}
		float width = rect.width * amount;
		if (origin == 1)
		{
			return Rect.MinMaxRect(rect.xMax - width, rect.yMin, rect.xMax, rect.yMax);
		}
		return Rect.MinMaxRect(rect.xMin, rect.yMin, rect.xMin + width, rect.yMax);
	}
	public static void buildSimple(FastUIGeometryBuilder builder, Rect rect, Sprite sprite)
	{
		builder.AddQuad(rect, getOuterUVRect(sprite));
	}
	public static bool tryGetSlicedGrid(Rect rect, Sprite sprite, float pixelsPerUnitMultiplier, out Vector4 x, out Vector4 y, out Vector4 u, out Vector4 v)
	{
		if (sprite == null || sprite.border == Vector4.zero)
		{
			x = default;
			y = default;
			u = default;
			v = default;
			return false;
		}
		Vector4 border = getAdjustedBorder(rect, sprite, pixelsPerUnitMultiplier);
		Vector4 outer = DataUtility.GetOuterUV(sprite);
		Vector4 inner = DataUtility.GetInnerUV(sprite);
		x = new Vector4(rect.xMin, rect.xMin + border.x, rect.xMax - border.z, rect.xMax);
		y = new Vector4(rect.yMin, rect.yMin + border.y, rect.yMax - border.w, rect.yMax);
		u = new Vector4(outer.x, inner.x, inner.z, outer.z);
		v = new Vector4(outer.y, inner.y, inner.w, outer.w);
		return true;
	}
	// 九宫格直接生成真实8/9个Quad。Border先换算到Canvas单位，再按目标Rect尺寸进行比例压缩，避免左右或上下Border互相穿过。
	public static void buildSliced(FastUIGeometryBuilder builder, Rect rect, Sprite sprite, bool fillCenter, float pixelsPerUnitMultiplier)
	{
		if (sprite == null || sprite.border == Vector4.zero)
		{
			buildSimple(builder, rect, sprite);
			return;
		}
		Vector4 border = getAdjustedBorder(rect, sprite, pixelsPerUnitMultiplier);
		Vector4 outer = DataUtility.GetOuterUV(sprite);
		Vector4 inner = DataUtility.GetInnerUV(sprite);
		Vector4 x = new(rect.xMin, rect.xMin + border.x, rect.xMax - border.z, rect.xMax);
		Vector4 y = new(rect.yMin, rect.yMin + border.y, rect.yMax - border.w, rect.yMax);
		Vector4 u = new(outer.x, inner.x, inner.z, outer.z);
		Vector4 v = new(outer.y, inner.y, inner.w, outer.w);
		for (int yi = 0; yi < 3; ++yi)
		{
			for (int xi = 0; xi < 3; ++xi)
			{
				if (!fillCenter && xi == 1 && yi == 1)
				{
					continue;
				}
				addRect(builder, x[xi], y[yi], x[xi + 1], y[yi + 1], u[xi], v[yi], u[xi + 1], v[yi + 1]);
			}
		}
	}
	public static void buildSliced(
		FastUIGeometryBuilder builder,
		Rect rect,
		Sprite sprite,
		bool fillCenter,
		float pixelsPerUnitMultiplier,
		FastUIImageFillMethod fillMethod,
		float fillAmount,
		int fillOrigin)
	{
		Rect fillRect = getSlicedFillRect(rect, fillMethod, fillAmount, fillOrigin);
		if (fillRect.width <= EPSILON || fillRect.height <= EPSILON)
		{
			return;
		}
		buildSliced(builder, fillRect, sprite, fillCenter, pixelsPerUnitMultiplier);
	}
	// Tiled保留Sprite Border：四角保持原尺寸，四条边只沿长轴平铺，中心区域双轴平铺。
	// 每个轴最多256段，同时限制单个平铺区域最多4096个Quad，防止错误尺寸/PPU把主线程和Mesh容量撑爆。
	public static void buildTiled(FastUIGeometryBuilder builder, Rect rect, Sprite sprite, bool fillCenter, float pixelsPerUnitMultiplier)
	{
		if (sprite == null)
		{
			buildSimple(builder, rect, null);
			return;
		}
		float pixelsPerUnit = getPixelsPerUnit(sprite, pixelsPerUnitMultiplier);
		Vector4 border = getAdjustedBorder(rect, sprite, pixelsPerUnitMultiplier);
		Vector4 outer = DataUtility.GetOuterUV(sprite);
		Vector4 inner = DataUtility.GetInnerUV(sprite);
		float x0 = rect.xMin;
		float x1 = rect.xMin + border.x;
		float x2 = rect.xMax - border.z;
		float x3 = rect.xMax;
		float y0 = rect.yMin;
		float y1 = rect.yMin + border.y;
		float y2 = rect.yMax - border.w;
		float y3 = rect.yMax;
		float centerPixelWidth = Mathf.Max(sprite.rect.width - sprite.border.x - sprite.border.z, 0.0f);
		float centerPixelHeight = Mathf.Max(sprite.rect.height - sprite.border.y - sprite.border.w, 0.0f);
		// Border已经占满Sprite某个轴时不存在可平铺中心像素，退化为单段拉伸，避免EPSILON Tile产生无意义的大量Quad。
		float tileWidth = centerPixelWidth > EPSILON ? centerPixelWidth / pixelsPerUnit : Mathf.Max(x2 - x1, EPSILON);
		float tileHeight = centerPixelHeight > EPSILON ? centerPixelHeight / pixelsPerUnit : Mathf.Max(y2 - y1, EPSILON);
		bool hasBorder = sprite.border != Vector4.zero;
		if (!hasBorder)
		{
			tileRect(builder, rect, new Rect(outer.x, outer.y, outer.z - outer.x, outer.w - outer.y), tileWidth, tileHeight, true, true);
			return;
		}
		addRect(builder, x0, y0, x1, y1, outer.x, outer.y, inner.x, inner.y);
		addRect(builder, x2, y0, x3, y1, inner.z, outer.y, outer.z, inner.y);
		addRect(builder, x0, y2, x1, y3, outer.x, inner.w, inner.x, outer.w);
		addRect(builder, x2, y2, x3, y3, inner.z, inner.w, outer.z, outer.w);
		Rect centerUV = new(inner.x, inner.y, inner.z - inner.x, inner.w - inner.y);
		if (x2 > x1 + EPSILON)
		{
			tileRect(builder, new Rect(x1, y0, x2 - x1, y1 - y0), new Rect(inner.x, outer.y, inner.z - inner.x, inner.y - outer.y), tileWidth, y1 - y0, true, false);
			tileRect(builder, new Rect(x1, y2, x2 - x1, y3 - y2), new Rect(inner.x, inner.w, inner.z - inner.x, outer.w - inner.w), tileWidth, y3 - y2, true, false);
		}
		if (y2 > y1 + EPSILON)
		{
			tileRect(builder, new Rect(x0, y1, x1 - x0, y2 - y1), new Rect(outer.x, inner.y, inner.x - outer.x, inner.w - inner.y), x1 - x0, tileHeight, false, true);
			tileRect(builder, new Rect(x2, y1, x3 - x2, y2 - y1), new Rect(inner.z, inner.y, outer.z - inner.z, inner.w - inner.y), x3 - x2, tileHeight, false, true);
		}
		if (fillCenter && x2 > x1 + EPSILON && y2 > y1 + EPSILON)
		{
			tileRect(builder, new Rect(x1, y1, x2 - x1, y2 - y1), centerUV, tileWidth, tileHeight, true, true);
		}
	}
	public static void buildFilled(
		FastUIGeometryBuilder builder,
		Rect rect,
		Sprite sprite,
		FastUIImageFillMethod method,
		float amount,
		bool clockwise,
		int origin)
	{
		amount = Mathf.Clamp01(amount);
		Rect uvRect = getOuterUVRect(sprite);
		origin = clampFillOrigin(method, origin);
		if (method == FastUIImageFillMethod.Horizontal)
		{
			buildLinearFilled(builder, rect, uvRect, true, amount, origin == 1);
			return;
		}
		if (method == FastUIImageFillMethod.Vertical)
		{
			buildLinearFilled(builder, rect, uvRect, false, amount, origin == 1);
			return;
		}
		buildRadialFilled(builder, rect, uvRect, method, amount, clockwise, origin);
	}
	private static void buildLinearFilled(FastUIGeometryBuilder builder, Rect rect, Rect uvRect, bool horizontal, float amount, bool reverse)
	{
		Rect position = rect;
		Rect uv = uvRect;
		if (horizontal)
		{
			float width = rect.width * amount;
			float uvWidth = uvRect.width * amount;
			if (reverse)
			{
				position.xMin = rect.xMax - width;
				uv.xMin = uvRect.xMax - uvWidth;
			}
			else
			{
				position.xMax = rect.xMin + width;
				uv.xMax = uvRect.xMin + uvWidth;
			}
		}
		else
		{
			float height = rect.height * amount;
			float uvHeight = uvRect.height * amount;
			if (reverse)
			{
				position.yMin = rect.yMax - height;
				uv.yMin = uvRect.yMax - uvHeight;
			}
			else
			{
				position.yMax = rect.yMin + height;
				uv.yMax = uvRect.yMin + uvHeight;
			}
		}
		builder.AddQuad(position, uv);
	}
	private static void buildRadialFilled(
		FastUIGeometryBuilder builder,
		Rect rect,
		Rect uvRect,
		FastUIImageFillMethod method,
		float amount,
		bool clockwise,
		int origin)
	{
		getRadialDefinition(rect, method, origin, clockwise, out Vector2 pivot, out float startAngle, out float fullSweep);
		float direction = clockwise ? -1.0f : 1.0f;
		float sweep = fullSweep * amount;
		float endAngle = startAngle + direction * sweep;
		sRadialPointCount = 0;
		Vector2 startPoint = rayRectIntersection(rect, pivot, startAngle);
		Vector2 endPoint = rayRectIntersection(rect, pivot, endAngle);
		addRadialPoint(startPoint, 0.0f);
		for (int i = 0; i < 4; ++i)
		{
			Vector2 corner = getRectCorner(rect, i);
			if ((corner - pivot).sqrMagnitude <= EPSILON * EPSILON)
			{
				continue;
			}
			float angle = Mathf.Atan2(corner.y - pivot.y, corner.x - pivot.x) * Mathf.Rad2Deg;
			float progress = angularProgress(startAngle, angle, clockwise);
			if (progress > EPSILON && progress < sweep - EPSILON)
			{
				addRadialPoint(corner, progress);
			}
		}
		addRadialPoint(endPoint, sweep);
		sortRadialPoints();
		Vector2 pivotUV = mapUV(rect, uvRect, pivot);
		int triangleCount = 0;
		for (int i = 0; i + 1 < sRadialPointCount; ++i)
		{
			Vector2 p1 = sRadialPointBuffer[i].mPosition;
			Vector2 p2 = sRadialPointBuffer[i + 1].mPosition;
			if ((p1 - p2).sqrMagnitude <= EPSILON * EPSILON)
			{
				continue;
			}
			Vector2 firstPoint = clockwise ? p1 : p2;
			Vector2 secondPoint = clockwise ? p2 : p1;
			builder.AddTriangle(
				new(pivot.x, pivot.y, 0.0f),
				new(firstPoint.x, firstPoint.y, 0.0f),
				new(secondPoint.x, secondPoint.y, 0.0f),
				pivotUV,
				mapUV(rect, uvRect, firstPoint),
				mapUV(rect, uvRect, secondPoint)
			);
			++triangleCount;
		}
		int fixedTriangleCount = getFixedRadialTriangleCount(method);
		while (triangleCount < fixedTriangleCount)
		{
			Vector3 degenerate = new(pivot.x, pivot.y, 0.0f);
			builder.AddTriangle(degenerate, degenerate, degenerate, pivotUV, pivotUV, pivotUV);
			++triangleCount;
		}
	}
	private static void sortRadialPoints()
	{
		// Radial最多只有起点、终点和4个矩形角点。直接插入排序避免List.Sort的Comparer/委托路径。
		for (int i = 1; i < sRadialPointCount; ++i)
		{
			FastUIRadialPoint value = sRadialPointBuffer[i];
			int write = i - 1;
			while (write >= 0 && sRadialPointBuffer[write].mProgress > value.mProgress)
			{
				sRadialPointBuffer[write + 1] = sRadialPointBuffer[write];
				--write;
			}
			sRadialPointBuffer[write + 1] = value;
		}
	}
	private static int getFixedRadialTriangleCount(FastUIImageFillMethod method)
	{
		if (method == FastUIImageFillMethod.Radial90)
		{
			return 2;
		}
		if (method == FastUIImageFillMethod.Radial180)
		{
			return 3;
		}
		return 5;
	}
	private static void getRadialDefinition(
		Rect rect,
		FastUIImageFillMethod method,
		int origin,
		bool clockwise,
		out Vector2 pivot,
		out float startAngle,
		out float fullSweep)
	{
		if (method == FastUIImageFillMethod.Radial90)
		{
			fullSweep = 90.0f;
			switch (origin)
			{
				case 1:
					pivot = new Vector2(rect.xMin, rect.yMax);
					startAngle = clockwise ? 0.0f : -90.0f;
					return;
				case 2:
					pivot = new Vector2(rect.xMax, rect.yMax);
					startAngle = clockwise ? -90.0f : 180.0f;
					return;
				case 3:
					pivot = new Vector2(rect.xMax, rect.yMin);
					startAngle = clockwise ? 180.0f : 90.0f;
					return;
				default:
					pivot = new Vector2(rect.xMin, rect.yMin);
					startAngle = clockwise ? 90.0f : 0.0f;
					return;
			}
		}
		if (method == FastUIImageFillMethod.Radial180)
		{
			fullSweep = 180.0f;
			switch (origin)
			{
				case 1:
					pivot = new Vector2(rect.xMin, rect.center.y);
					startAngle = clockwise ? 90.0f : -90.0f;
					return;
				case 2:
					pivot = new Vector2(rect.center.x, rect.yMax);
					startAngle = clockwise ? 0.0f : 180.0f;
					return;
				case 3:
					pivot = new Vector2(rect.xMax, rect.center.y);
					startAngle = clockwise ? -90.0f : 90.0f;
					return;
				default:
					pivot = new Vector2(rect.center.x, rect.yMin);
					startAngle = clockwise ? 180.0f : 0.0f;
					return;
			}
		}
		fullSweep = 360.0f;
		pivot = rect.center;
		switch (origin)
		{
			case 1:
				startAngle = 0.0f;
				break;
			case 2:
				startAngle = 90.0f;
				break;
			case 3:
				startAngle = 180.0f;
				break;
			default:
				startAngle = -90.0f;
				break;
		}
	}
	private static float angularProgress(float startAngle, float angle, bool clockwise)
	{
		return clockwise ? Mathf.Repeat(startAngle - angle, 360.0f) : Mathf.Repeat(angle - startAngle, 360.0f);
	}
	private static Vector2 rayRectIntersection(Rect rect, Vector2 origin, float angleDegrees)
	{
		float radians = angleDegrees * Mathf.Deg2Rad;
		Vector2 direction = new(Mathf.Cos(radians), Mathf.Sin(radians));
		float bestT = float.PositiveInfinity;
		if (Mathf.Abs(direction.x) > EPSILON)
		{
			float tx0 = (rect.xMin - origin.x) / direction.x;
			float tx1 = (rect.xMax - origin.x) / direction.x;
			if (tx0 > EPSILON)
			{
				float y = origin.y + direction.y * tx0;
				if (y >= rect.yMin - EPSILON && y <= rect.yMax + EPSILON)
				{
					bestT = Mathf.Min(bestT, tx0);
				}
			}
			if (tx1 > EPSILON)
			{
				float y = origin.y + direction.y * tx1;
				if (y >= rect.yMin - EPSILON && y <= rect.yMax + EPSILON)
				{
					bestT = Mathf.Min(bestT, tx1);
				}
			}
		}
		if (Mathf.Abs(direction.y) > EPSILON)
		{
			float ty0 = (rect.yMin - origin.y) / direction.y;
			float ty1 = (rect.yMax - origin.y) / direction.y;
			if (ty0 > EPSILON)
			{
				float x = origin.x + direction.x * ty0;
				if (x >= rect.xMin - EPSILON && x <= rect.xMax + EPSILON)
				{
					bestT = Mathf.Min(bestT, ty0);
				}
			}
			if (ty1 > EPSILON)
			{
				float x = origin.x + direction.x * ty1;
				if (x >= rect.xMin - EPSILON && x <= rect.xMax + EPSILON)
				{
					bestT = Mathf.Min(bestT, ty1);
				}
			}
		}
		if (float.IsInfinity(bestT))
		{
			return origin;
		}
		return origin + direction * bestT;
	}
	private static Vector2 getRectCorner(Rect rect, int index)
	{
		switch (index)
		{
			case 1:
				return new Vector2(rect.xMin, rect.yMax);
			case 2:
				return new Vector2(rect.xMax, rect.yMax);
			case 3:
				return new Vector2(rect.xMax, rect.yMin);
			default:
				return new Vector2(rect.xMin, rect.yMin);
		}
	}
	private static void addRadialPoint(Vector2 point, float progress)
	{
		for (int i = 0; i < sRadialPointCount; ++i)
		{
			FastUIRadialPoint existing = sRadialPointBuffer[i];
			if ((existing.mPosition - point).sqrMagnitude <= EPSILON * EPSILON &&
				Mathf.Abs(existing.mProgress - progress) <= EPSILON)
			{
				return;
			}
		}
		if (sRadialPointCount >= sRadialPointBuffer.Length)
		{
			return;
		}
		sRadialPointBuffer[sRadialPointCount++] = new FastUIRadialPoint(point, progress);
	}
	private static Vector2 mapUV(Rect positionRect, Rect uvRect, Vector2 point)
	{
		float tx = Mathf.Abs(positionRect.width) > EPSILON ? (point.x - positionRect.xMin) / positionRect.width : 0.0f;
		float ty = Mathf.Abs(positionRect.height) > EPSILON ? (point.y - positionRect.yMin) / positionRect.height : 0.0f;
		return new Vector2(Mathf.Lerp(uvRect.xMin, uvRect.xMax, tx), Mathf.Lerp(uvRect.yMin, uvRect.yMax, ty));
	}
	private static Vector4 getAdjustedBorder(Rect rect, Sprite sprite, float pixelsPerUnitMultiplier)
	{
		float pixelsPerUnit = getPixelsPerUnit(sprite, pixelsPerUnitMultiplier);
		Vector4 border = sprite.border / pixelsPerUnit;
		float horizontal = border.x + border.z;
		float vertical = border.y + border.w;
		float rectWidth = Mathf.Abs(rect.width);
		float rectHeight = Mathf.Abs(rect.height);
		if (horizontal > rectWidth && horizontal > EPSILON)
		{
			float scale = rectWidth / horizontal;
			border.x *= scale;
			border.z *= scale;
		}
		if (vertical > rectHeight && vertical > EPSILON)
		{
			float scale = rectHeight / vertical;
			border.y *= scale;
			border.w *= scale;
		}
		return border;
	}
	private static float getPixelsPerUnit(Sprite sprite, float pixelsPerUnitMultiplier)
	{
		float multiplier = Mathf.Max(pixelsPerUnitMultiplier, 0.01f);
		// FastCanvas使用与UGUI默认Canvas一致的100 referencePixelsPerUnit语义。
		// 普通PPU=100的Sprite因此是1 UI单位/像素；Multiplier越大，Border和Tile在UI空间越小，与UGUI Image一致。
		return Mathf.Max(getBasePixelsPerUnit(sprite) * multiplier, EPSILON);
	}
	private static float getBasePixelsPerUnit(Sprite sprite)
	{
		return Mathf.Max(sprite.pixelsPerUnit / DEFAULT_REFERENCE_PIXELS_PER_UNIT, EPSILON);
	}
	private static void tileRect(
		FastUIGeometryBuilder builder,
		Rect positionRect,
		Rect uvRect,
		float tileWidth,
		float tileHeight,
		bool tileX,
		bool tileY)
	{
		if (positionRect.width <= EPSILON || positionRect.height <= EPSILON)
		{
			return;
		}
		int requestedXCount = tileX ? Mathf.Max(Mathf.CeilToInt(positionRect.width / Mathf.Max(tileWidth, EPSILON)), 1) : 1;
		int requestedYCount = tileY ? Mathf.Max(Mathf.CeilToInt(positionRect.height / Mathf.Max(tileHeight, EPSILON)), 1) : 1;
		int xCount = Mathf.Min(requestedXCount, MAX_TILED_AXIS_COUNT);
		int yCount = Mathf.Min(requestedYCount, MAX_TILED_AXIS_COUNT);
		// 极端小Tile可能让中心区域产生数万Quad。这里保留图案覆盖范围，但主动放大Tile尺寸控制单元素几何上限。
		// 4096 Quad对应16384顶点，避免一个错误PPU或异常Rect直接把整个Canvas的Mesh容量推高几个数量级。
		if ((long)xCount * yCount > MAX_TILED_QUAD_COUNT)
		{
			float scale = Mathf.Sqrt(MAX_TILED_QUAD_COUNT / ((float)xCount * yCount));
			xCount = Mathf.Max(Mathf.FloorToInt(xCount * scale), 1);
			yCount = Mathf.Max(Mathf.FloorToInt(yCount * scale), 1);
			while ((long)xCount * yCount > MAX_TILED_QUAD_COUNT)
			{
				if (xCount >= yCount && xCount > 1)
				{
					--xCount;
				}
				else if (yCount > 1)
				{
					--yCount;
				}
				else
				{
					break;
				}
			}
		}
		float stepX = tileX && requestedXCount > xCount ? positionRect.width / xCount : tileX ? tileWidth : positionRect.width;
		float stepY = tileY && requestedYCount > yCount ? positionRect.height / yCount : tileY ? tileHeight : positionRect.height;
		for (int y = 0; y < yCount; ++y)
		{
			float yMin = positionRect.yMin + y * stepY;
			float yMax = Mathf.Min(yMin + stepY, positionRect.yMax);
			float vRatio = tileY ? (yMax - yMin) / stepY : 1.0f;
			for (int x = 0; x < xCount; ++x)
			{
				float xMin = positionRect.xMin + x * stepX;
				float xMax = Mathf.Min(xMin + stepX, positionRect.xMax);
				float uRatio = tileX ? (xMax - xMin) / stepX : 1.0f;
				addRect(
					builder,
					xMin,
					yMin,
					xMax,
					yMax,
					uvRect.xMin,
					uvRect.yMin,
					Mathf.Lerp(uvRect.xMin, uvRect.xMax, uRatio),
					Mathf.Lerp(uvRect.yMin, uvRect.yMax, vRatio)
				);
			}
		}
	}
	private static void addRect(FastUIGeometryBuilder builder, float xMin, float yMin, float xMax, float yMax, float uMin, float vMin, float uMax, float vMax)
	{
		if (xMax <= xMin + EPSILON || yMax <= yMin + EPSILON)
		{
			return;
		}
		builder.AddQuad(
			new Vector3(xMin, yMin, 0.0f),
			new Vector3(xMin, yMax, 0.0f),
			new Vector3(xMax, yMax, 0.0f),
			new Vector3(xMax, yMin, 0.0f),
			new Vector2(uMin, vMin),
			new Vector2(uMin, vMax),
			new Vector2(uMax, vMax),
			new Vector2(uMax, vMin)
		);
	}
}
