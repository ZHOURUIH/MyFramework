using UnityEngine;
using static MathUtility;

// ImageXBR4图片处理效果,用于将像素块的图像放大后再细节化
public class ImageXBR4
{
	public const float LUMINANCE_WEIGHT = 1.0f;
	public const float EQUAL_COLOR_TOLERANCE = 30.0f / 255.0f;
	public const float STEEP_DIRECTION_THRESHOLD = 1.2f;
	public const float DOMINANT_DIRECTION_THRESHOLD = 0.6f;
	public const float one_third = 1.0f / 3.0f;
	public const float two_third = 2.0f / 3.0f;
	public const int BLEND_NONE = 0;
	public const int BLEND_NORMAL = 1;
	public const int BLEND_DOMINANT = 2;
	// scale默认为4,所以输出的图片的宽高是原来的4倍
	public static Texture2D convertImage(Texture2D tex, int scale = 4)
	{
		int width = tex.width * scale;
		int height = tex.height * scale;
		scaleTexture(tex, scale, out Color[] pixels);
		Color[] newPixel = new Color[pixels.Length];
		for(int i = 0; i < height; ++i)
		{
			for(int j = 0; j < width; ++j)
			{
				newPixel[j + i * width] = fragment(pixels, new((float)j / width, (float)i / height), width, height, scale);
			}
		}
		Texture2D newTex = new(tex.width * scale, tex.height * scale, TextureFormat.RGBA32, false);
		newTex.SetPixels(newPixel);
		newTex.Apply(false);
		return newTex;
	}
	//------------------------------------------------------------------------------------------------------------------------------
	protected static Vector4 fragment(Color[] pixels, Vector2 texCoord, int width, int height, int scale)
	{
		Vector4[] src = new Vector4[25];
		Vector2 ps = new(1.0f / (width / scale), 1.0f / (height / scale));
		float dx = ps.x;
		float dy = ps.y;
		Vector4 t1 = new Vector4(texCoord.x, texCoord.x, texCoord.x, texCoord.y) + new Vector4(-dx, 0, dx, -2.0f * dy); // A1 B1 C1
		Vector4 t2 = new Vector4(texCoord.x, texCoord.x, texCoord.x, texCoord.y) + new Vector4(-dx, 0, dx, -dy); // A B C
		Vector4 t3 = new Vector4(texCoord.x, texCoord.x, texCoord.x, texCoord.y) + new Vector4(-dx, 0, dx, 0); // D E F
		Vector4 t4 = new Vector4(texCoord.x, texCoord.x, texCoord.x, texCoord.y) + new Vector4(-dx, 0, dx, dy); // G H I
		Vector4 t5 = new Vector4(texCoord.x, texCoord.x, texCoord.x, texCoord.y) + new Vector4(-dx, 0, dx, 2.0f * dy); // G5 H5 I5
		Vector4 t6 = new Vector4(texCoord.x, texCoord.y, texCoord.y, texCoord.y) + new Vector4(-2.0f * dx, -dy, 0, dy); // A0 D0 G0
		Vector4 t7 = new Vector4(texCoord.x, texCoord.y, texCoord.y, texCoord.y) + new Vector4(2.0f * dx, -dy, 0, dy); // C4 F4 I4

		src[21] = tex2D(pixels, width, height, new(t1.x, t1.w));
		src[22] = tex2D(pixels, width, height, new(t1.y, t1.w));
		src[23] = tex2D(pixels, width, height, new(t1.z, t1.w));
		src[6] = tex2D(pixels, width, height, new(t2.x, t2.w));
		src[7] = tex2D(pixels, width, height, new(t2.y, t2.w));
		src[8] = tex2D(pixels, width, height, new(t2.z, t2.w));
		src[5] = tex2D(pixels, width, height, new(t3.x, t3.w));
		src[0] = tex2D(pixels, width, height, new(t3.y, t3.w));
		src[1] = tex2D(pixels, width, height, new(t3.z, t3.w));
		src[4] = tex2D(pixels, width, height, new(t4.x, t4.w));
		src[3] = tex2D(pixels, width, height, new(t4.y, t4.w));
		src[2] = tex2D(pixels, width, height, new(t4.z, t4.w));
		src[15] = tex2D(pixels, width, height, new(t5.x, t5.w));
		src[14] = tex2D(pixels, width, height, new(t5.y, t5.w));
		src[13] = tex2D(pixels, width, height, new(t5.z, t5.w));
		src[19] = tex2D(pixels, width, height, new(t6.x, t6.y));
		src[18] = tex2D(pixels, width, height, new(t6.x, t6.z));
		src[17] = tex2D(pixels, width, height, new(t6.x, t6.w));
		src[9] = tex2D(pixels, width, height, new(t7.x, t7.y));
		src[10] = tex2D(pixels, width, height, new(t7.x, t7.z));
		src[11] = tex2D(pixels, width, height, new(t7.x, t7.w));

		float[] v = new float[9];
		v[0] = reduce(src[0]);
		v[1] = reduce(src[1]);
		v[2] = reduce(src[2]);
		v[3] = reduce(src[3]);
		v[4] = reduce(src[4]);
		v[5] = reduce(src[5]);
		v[6] = reduce(src[6]);
		v[7] = reduce(src[7]);
		v[8] = reduce(src[8]);

		Vector4 blendResult = new();

		if (!((v[0] == v[1] && v[3] == v[2]) || (v[0] == v[3] && v[1] == v[2])))
		{
			float dist_03_01 = distYCbCr(src[4], src[0]) + distYCbCr(src[0], src[8]) + distYCbCr(src[14], src[2]) + distYCbCr(src[2], src[10]) + (4.0f * distYCbCr(src[3], src[1]));
			float dist_00_02 = distYCbCr(src[5], src[3]) + distYCbCr(src[3], src[13]) + distYCbCr(src[7], src[1]) + distYCbCr(src[1], src[11]) + (4.0f * distYCbCr(src[0], src[2]));
			bool dominantGradient = (DOMINANT_DIRECTION_THRESHOLD * dist_03_01) < dist_00_02;
			blendResult[2] = ((dist_03_01 < dist_00_02) && (v[0] != v[1]) && (v[0] != v[3])) ? ((dominantGradient) ? BLEND_DOMINANT : BLEND_NORMAL) : BLEND_NONE;
		}

		if (!((v[5] == v[0] && v[4] == v[3]) || (v[5] == v[4] && v[0] == v[3])))
		{
			float dist_04_00 = distYCbCr(src[17], src[5]) + distYCbCr(src[5], src[7]) + distYCbCr(src[15], src[3]) + distYCbCr(src[3], src[1]) + (4.0f * distYCbCr(src[4], src[0]));
			float dist_05_03 = distYCbCr(src[18], src[4]) + distYCbCr(src[4], src[14]) + distYCbCr(src[6], src[0]) + distYCbCr(src[0], src[2]) + (4.0f * distYCbCr(src[5], src[3]));
			bool dominantGradient = (DOMINANT_DIRECTION_THRESHOLD * dist_05_03) < dist_04_00;
			blendResult[3] = ((dist_04_00 > dist_05_03) && (v[0] != v[5]) && (v[0] != v[3])) ? ((dominantGradient) ? BLEND_DOMINANT : BLEND_NORMAL) : BLEND_NONE;
		}

		if (!((v[7] == v[8] && v[0] == v[1]) || (v[7] == v[0] && v[8] == v[1])))
		{
			float dist_00_08 = distYCbCr(src[5], src[7]) + distYCbCr(src[7], src[23]) + distYCbCr(src[3], src[1]) + distYCbCr(src[1], src[9]) + (4.0f * distYCbCr(src[0], src[8]));
			float dist_07_01 = distYCbCr(src[6], src[0]) + distYCbCr(src[0], src[2]) + distYCbCr(src[22], src[8]) + distYCbCr(src[8], src[10]) + (4.0f * distYCbCr(src[7], src[1]));
			bool dominantGradient = (DOMINANT_DIRECTION_THRESHOLD * dist_07_01) < dist_00_08;
			blendResult[1] = ((dist_00_08 > dist_07_01) && (v[0] != v[7]) && (v[0] != v[1])) ? ((dominantGradient) ? BLEND_DOMINANT : BLEND_NORMAL) : BLEND_NONE;
		}

		if (!((v[6] == v[7] && v[5] == v[0]) || (v[6] == v[5] && v[7] == v[0])))
		{
			float dist_05_07 = distYCbCr(src[18], src[6]) + distYCbCr(src[6], src[22]) + distYCbCr(src[4], src[0]) + distYCbCr(src[0], src[8]) + (4.0f * distYCbCr(src[5], src[7]));
			float dist_06_00 = distYCbCr(src[19], src[5]) + distYCbCr(src[5], src[3]) + distYCbCr(src[21], src[7]) + distYCbCr(src[7], src[1]) + (4.0f * distYCbCr(src[6], src[0]));
			bool dominantGradient = (DOMINANT_DIRECTION_THRESHOLD * dist_05_07) < dist_06_00;
			blendResult[0] = ((dist_05_07 < dist_06_00) && (v[0] != v[5]) && (v[0] != v[7])) ? ((dominantGradient) ? BLEND_DOMINANT : BLEND_NORMAL) : BLEND_NONE;
		}

		Vector4[] dst = new Vector4[16];
		dst[0] = src[0];
		dst[1] = src[0];
		dst[2] = src[0];
		dst[3] = src[0];
		dst[4] = src[0];
		dst[5] = src[0];
		dst[6] = src[0];
		dst[7] = src[0];
		dst[8] = src[0];
		dst[9] = src[0];
		dst[10] = src[0];
		dst[11] = src[0];
		dst[12] = src[0];
		dst[13] = src[0];
		dst[14] = src[0];
		dst[15] = src[0];

		// Scale pixel
		if (IsBlendingNeeded(blendResult))
		{
			float dist_01_04 = distYCbCr(src[1], src[4]);
			float dist_03_08 = distYCbCr(src[3], src[8]);
			bool haveShallowLine = (STEEP_DIRECTION_THRESHOLD * dist_01_04 <= dist_03_08) && (v[0] != v[4]) && (v[5] != v[4]);
			bool haveSteepLine = (STEEP_DIRECTION_THRESHOLD * dist_03_08 <= dist_01_04) && (v[0] != v[8]) && (v[7] != v[8]);
			bool needBlend = (blendResult[2] != BLEND_NONE);
			bool doLineBlend = (blendResult[2] >= BLEND_DOMINANT ||
				!((blendResult[1] != BLEND_NONE && !isPixelEqual(src[0], src[4])) ||
				(blendResult[3] != BLEND_NONE && !isPixelEqual(src[0], src[8])) ||
					(isPixelEqual(src[4], src[3]) && isPixelEqual(src[3], src[2]) && isPixelEqual(src[2], src[1]) && isPixelEqual(src[1], src[8]) && !isPixelEqual(src[0], src[2]))));

			Vector4 blendPix = (distYCbCr(src[0], src[1]) <= distYCbCr(src[0], src[3])) ? src[1] : src[3];
			dst[2] = lerp(dst[2], blendPix, (needBlend && doLineBlend) ? (haveShallowLine ? (haveSteepLine ? 1.0f / 3.0f : 0.25f) : haveSteepLine ? 0.25f : 0.0f) : 0.0f);
			dst[9] = lerp(dst[9], blendPix, (needBlend && doLineBlend && haveSteepLine) ? 0.25f : 0.0f);
			dst[10] = lerp(dst[10], blendPix, (needBlend && doLineBlend && haveSteepLine) ? 0.75f : 0.0f);
			dst[11] = lerp(dst[11], blendPix, (needBlend) ? (doLineBlend ? (haveSteepLine ? 1.0f : (haveShallowLine ? 0.75f : 0.5f)) : 0.08677704501f) : 0.0f);
			dst[12] = lerp(dst[12], blendPix, (needBlend) ? (doLineBlend ? 1.0f : 0.6848532563f) : 0.0f);
			dst[13] = lerp(dst[13], blendPix, (needBlend) ? (doLineBlend ? (haveShallowLine ? 1.0f : (haveSteepLine ? 0.75f : 0.5f)) : 0.08677704501f) : 0.0f);
			dst[14] = lerp(dst[14], blendPix, (needBlend && doLineBlend && haveShallowLine) ? 0.75f : 0.0f);
			dst[15] = lerp(dst[15], blendPix, (needBlend && doLineBlend && haveShallowLine) ? 0.25f : 0.0f);

			dist_01_04 = distYCbCr(src[7], src[2]);
			dist_03_08 = distYCbCr(src[1], src[6]);
			haveShallowLine = (STEEP_DIRECTION_THRESHOLD * dist_01_04 <= dist_03_08) && (v[0] != v[2]) && (v[3] != v[2]);
			haveSteepLine = (STEEP_DIRECTION_THRESHOLD * dist_03_08 <= dist_01_04) && (v[0] != v[6]) && (v[5] != v[6]);
			needBlend = (blendResult[1] != BLEND_NONE);
			doLineBlend = (blendResult[1] >= BLEND_DOMINANT ||
				!((blendResult[0] != BLEND_NONE && !isPixelEqual(src[0], src[2])) ||
				(blendResult[2] != BLEND_NONE && !isPixelEqual(src[0], src[6])) ||
					(isPixelEqual(src[2], src[1]) && isPixelEqual(src[1], src[8]) && isPixelEqual(src[8], src[7]) && isPixelEqual(src[7], src[6]) && !isPixelEqual(src[0], src[8]))));

			blendPix = (distYCbCr(src[0], src[7]) <= distYCbCr(src[0], src[1])) ? src[7] : src[1];
			dst[1] = lerp(dst[1], blendPix, (needBlend && doLineBlend) ? (haveShallowLine ? (haveSteepLine ? 1.0f / 3.0f : 0.25f) : (haveSteepLine ? 0.25f : 0.0f)) : 0.0f);
			dst[6] = lerp(dst[6], blendPix, (needBlend && doLineBlend && haveSteepLine) ? 0.25f : 0.0f);
			dst[7] = lerp(dst[7], blendPix, (needBlend && doLineBlend && haveSteepLine) ? 0.75f : 0.0f);
			dst[8] = lerp(dst[8], blendPix, (needBlend) ? (doLineBlend ? (haveSteepLine ? 1.0f : (haveShallowLine ? 0.75f : 0.5f)) : 0.08677704501f) : 0.0f);
			dst[9] = lerp(dst[9], blendPix, (needBlend) ? (doLineBlend ? 1.0f : 0.6848532563f) : 0.0f);
			dst[10] = lerp(dst[10], blendPix, (needBlend) ? (doLineBlend ? (haveShallowLine ? 1.0f : (haveSteepLine ? 0.75f : 0.5f)) : 0.08677704501f) : 0.0f);
			dst[11] = lerp(dst[11], blendPix, (needBlend && doLineBlend && haveShallowLine) ? 0.75f : 0.0f);
			dst[12] = lerp(dst[12], blendPix, (needBlend && doLineBlend && haveShallowLine) ? 0.25f : 0.0f);

			dist_01_04 = distYCbCr(src[5], src[8]);
			dist_03_08 = distYCbCr(src[7], src[4]);
			haveShallowLine = (STEEP_DIRECTION_THRESHOLD * dist_01_04 <= dist_03_08) && (v[0] != v[8]) && (v[1] != v[8]);
			haveSteepLine = (STEEP_DIRECTION_THRESHOLD * dist_03_08 <= dist_01_04) && (v[0] != v[4]) && (v[3] != v[4]);
			needBlend = (blendResult[0] != BLEND_NONE);
			doLineBlend = (blendResult[0] >= BLEND_DOMINANT ||
				!((blendResult[3] != BLEND_NONE && !isPixelEqual(src[0], src[8])) ||
				(blendResult[1] != BLEND_NONE && !isPixelEqual(src[0], src[4])) ||
					(isPixelEqual(src[8], src[7]) && isPixelEqual(src[7], src[6]) && isPixelEqual(src[6], src[5]) && isPixelEqual(src[5], src[4]) && !isPixelEqual(src[0], src[6]))));

			blendPix = (distYCbCr(src[0], src[5]) <= distYCbCr(src[0], src[7])) ? src[5] : src[7];
			dst[0] = lerp(dst[0], blendPix, (needBlend && doLineBlend) ? (haveShallowLine ? (haveSteepLine ? 1.0f / 3.0f : 0.25f) : (haveSteepLine ? 0.25f : 0.0f)) : 0.0f);
			dst[15] = lerp(dst[15], blendPix, (needBlend && doLineBlend && haveSteepLine) ? 0.25f : 0.0f);
			dst[4] = lerp(dst[4], blendPix, (needBlend && doLineBlend && haveSteepLine) ? 0.75f : 0.0f);
			dst[5] = lerp(dst[5], blendPix, (needBlend) ? (doLineBlend ? (haveSteepLine ? 1.0f : (haveShallowLine ? 0.75f : 0.5f)) : 0.08677704501f) : 0.0f);
			dst[6] = lerp(dst[6], blendPix, (needBlend) ? (doLineBlend ? 1.0f : 0.6848532563f) : 0.0f);
			dst[7] = lerp(dst[7], blendPix, (needBlend) ? (doLineBlend ? (haveShallowLine ? 1.0f : (haveSteepLine ? 0.75f : 0.5f)) : 0.08677704501f) : 0.0f);
			dst[8] = lerp(dst[8], blendPix, (needBlend && doLineBlend && haveShallowLine) ? 0.75f : 0.0f);
			dst[9] = lerp(dst[9], blendPix, (needBlend && doLineBlend && haveShallowLine) ? 0.25f : 0.0f);

			dist_01_04 = distYCbCr(src[3], src[6]);
			dist_03_08 = distYCbCr(src[5], src[2]);
			haveShallowLine = (STEEP_DIRECTION_THRESHOLD * dist_01_04 <= dist_03_08) && (v[0] != v[6]) && (v[7] != v[6]);
			haveSteepLine = (STEEP_DIRECTION_THRESHOLD * dist_03_08 <= dist_01_04) && (v[0] != v[2]) && (v[1] != v[2]);
			needBlend = (blendResult[3] != BLEND_NONE);
			doLineBlend = (blendResult[3] >= BLEND_DOMINANT ||
				!((blendResult[2] != BLEND_NONE && !isPixelEqual(src[0], src[6])) ||
				(blendResult[0] != BLEND_NONE && !isPixelEqual(src[0], src[2])) ||
					(isPixelEqual(src[6], src[5]) && isPixelEqual(src[5], src[4]) && isPixelEqual(src[4], src[3]) && isPixelEqual(src[3], src[2]) && !isPixelEqual(src[0], src[4]))));

			blendPix = (distYCbCr(src[0], src[3]) <= distYCbCr(src[0], src[5])) ? src[3] : src[5];
			dst[3] = lerp(dst[3], blendPix, (needBlend && doLineBlend) ? (haveShallowLine ? (haveSteepLine ? 1.0f / 3.0f : 0.25f) : (haveSteepLine ? 0.25f : 0.0f)) : 0.0f);
			dst[12] = lerp(dst[12], blendPix, (needBlend && doLineBlend && haveSteepLine) ? 0.25f : 0.0f);
			dst[13] = lerp(dst[13], blendPix, (needBlend && doLineBlend && haveSteepLine) ? 0.75f : 0.0f);
			dst[14] = lerp(dst[14], blendPix, needBlend ? (doLineBlend ? (haveSteepLine ? 1.0f : (haveShallowLine ? 0.75f : 0.5f)) : 0.08677704501f) : 0.0f);
			dst[15] = lerp(dst[15], blendPix, needBlend ? (doLineBlend ? 1.0f : 0.6848532563f) : 0.0f);
			dst[4] = lerp(dst[4], blendPix, needBlend ? (doLineBlend ? (haveShallowLine ? 1.0f : (haveSteepLine ? 0.75f : 0.5f)) : 0.08677704501f) : 0.0f);
			dst[5] = lerp(dst[5], blendPix, (needBlend && doLineBlend && haveShallowLine) ? 0.75f : 0.0f);
			dst[6] = lerp(dst[6], blendPix, (needBlend && doLineBlend && haveShallowLine) ? 0.25f : 0.0f);
		}

		Vector2 f = new(frac(texCoord.x * divide(width, scale)), frac(texCoord.y * divide(height, scale)));
		Vector4 res = lerp(
			lerp(lerp(lerp(dst[6], dst[7], step(0.25f, f.x)), lerp(dst[8], dst[9], step(0.75f, f.x)), step(0.5f, f.x)), lerp(lerp(dst[5], dst[0], step(0.25f, f.x)), lerp(dst[1], dst[10], step(0.75f, f.x)), step(0.5f, f.x)), step(0.25f, f.y)),
			lerp(lerp(lerp(dst[4], dst[3], step(0.25f, f.x)), lerp(dst[2], dst[11], step(0.75f, f.x)), step(0.5f, f.x)), lerp(lerp(dst[15], dst[14], step(0.25f, f.x)), lerp(dst[13], dst[12], step(0.75f, f.x)), step(0.5f, f.x)), step(0.75f, f.y)),
			step(0.5f, f.y));

		return res;
	}
	protected static float reduce(Vector3 color)
	{
		return (int)dot(color, new(65536.0f, 256.0f, 1.0f));
	}
	protected static float distYCbCr(Vector3 pixA, Vector3 pixB)
	{
		Vector3 w = new(0.2627f, 0.6780f, 0.0593f);
		float scaleB = 0.5f / (1.0f - w.z);
		float scaleR = 0.5f / (1.0f - w.x);
		Vector3 diff = pixA - pixB;
		float Y = dot(diff, w);
		float Cb = scaleB * (diff.z - Y);
		float Cr = scaleR * (diff.x - Y);
		return sqrt(LUMINANCE_WEIGHT * Y * LUMINANCE_WEIGHT * Y + Cb * Cb + Cr * Cr);
	}
	protected static bool isPixelEqual(Vector3 pixA, Vector3 pixB)
	{
		return distYCbCr(pixA, pixB) < EQUAL_COLOR_TOLERANCE;
	}
	protected static bool IsBlendingNeeded(Vector4 blend)
	{
		return blend != new Vector4(BLEND_NONE, BLEND_NONE, BLEND_NONE, BLEND_NONE);
	}
	protected static Color tex2D(Color[] pixels, int width, int height, Vector2 texCoord)
	{
		saturate(ref texCoord.x);
		saturate(ref texCoord.y);
		int x = (int)(texCoord.x * width);
		clampMax(ref x, width - 1);
		int y = (int)(texCoord.y * height);
		clampMax(ref y, height - 1);
		return pixels[x + y * width];
	}
	protected static void scaleTexture(Texture2D tex, int scale, out Color[] scaledPixels)
	{
		int width = tex.width;
		int height = tex.height;
		int scaledWidth = width * scale;
		int scaledHeight = height * scale;
		scaledPixels = new Color[scaledWidth * scaledHeight];
		Color[] pixels = tex.GetPixels();
		for (int i = 0; i < scaledHeight; ++i)
		{
			int originY = (int)divide(i, scaledHeight * height);
			for (int j = 0; j < scaledWidth; ++j)
			{
				int originX = (int)divide(j, scaledWidth * width);
				scaledPixels[j + i * scaledWidth] = pixels[originX + originY * width];
			}
		}
	}
}