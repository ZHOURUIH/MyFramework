//-----------------------------------------------------------------------------
// Copyright 2015-2017 RenderHeads Ltd.  All rights reserverd.
//-----------------------------------------------------------------------------

#if defined (SHADERLAB_GLSL)
	#define INLINE 
	#define HALF2 vec2
	#define HALF3 vec3
	#define HALF4 vec4
	#define FLOAT2 vec2
	#define FLOAT3 vec3
	#define FLOAT4 vec4
	#define FLOAT3X3 mat3
#else
	#define INLINE inline
	#define HALF2 half2
	#define HALF3 half3
	#define HALF4 half4
	#define FLOAT2 float2
	#define FLOAT3 float3
	#define FLOAT4 float4
	#define FLOAT3X3 float3x3
#endif

// Specify this so Unity doesn't automatically update our shaders.
#define UNITY_SHADER_NO_UPGRADE 1

// We use this method so that when Unity automatically updates the shader from the old
// mul(UNITY_MATRIX_MVP.. to UnityObjectToClipPos that it only changes in one place.
INLINE FLOAT4 XFormObjectToClip(FLOAT4 vertex)
{
#if defined(SHADERLAB_GLSL)
	return gl_ModelViewProjectionMatrix * vertex;
#else
#if (UNITY_VERSION >= 560)
	return UnityObjectToClipPos(vertex);
#else
	return mul(UNITY_MATRIX_MVP, vertex);
#endif
#endif
}

INLINE bool IsStereoEyeLeft(FLOAT3 worldNosePosition, FLOAT3 worldCameraRight)
{
#if defined(FORCEEYE_LEFT)
	return true;
#elif defined(FORCEEYE_RIGHT)
	return false;
#elif defined(UNITY_SINGLE_PASS_STEREO) || defined (UNITY_STEREO_INSTANCING_ENABLED)
	// Unity 5.4 has this new variable
	return (unity_StereoEyeIndex == 0);
#elif defined (UNITY_DECLARE_MULTIVIEW)
	// OVR_multiview extension
	return (UNITY_VIEWID == 0);
#else

//#if (UNITY_VERSION > 540) && defined(GOOGLEVR) && !defined(SHADERLAB_GLSL)
	// Daydream support uses the skew component of the projection matrix
	// (But unity_CameraProjection doesn't seem to be declared when using GLSL)
	// NOTE: we've had to remove this minor optimisationg as it was causing too many isues.  
	//       eg. Unity 5.4.1 in GLSL mode complained UNITY_VERSION and unity_CameraProjection aren't defined
	//return (unity_CameraProjection[0][2] > 0.0);
//#else
	// worldNosePosition is the camera positon passed in from Unity via script
	// We need to determine whether _WorldSpaceCameraPos (Unity shader variable) is to the left or to the right of _cameraPosition
	float dRight = distance(worldNosePosition + worldCameraRight, _WorldSpaceCameraPos);
	float dLeft = distance(worldNosePosition - worldCameraRight, _WorldSpaceCameraPos);
	return (dRight > dLeft);
//#endif

#endif
}

#if defined(STEREO_TOP_BOTTOM) || defined(STEREO_LEFT_RIGHT)
FLOAT4 GetStereoScaleOffset(bool isLeftEye, bool isYFlipped)
{
	FLOAT2 scale = FLOAT2(1.0, 1.0);
	FLOAT2 offset = FLOAT2(0.0, 0.0);

	// Top-Bottom
#if defined(STEREO_TOP_BOTTOM)

	scale.y = 0.5;
	offset.y = 0.0;

	if (!isLeftEye)
	{
		offset.y = 0.5;
	}

#if !defined(SHADERLAB_GLSL) 
#if !defined(UNITY_UV_STARTS_AT_TOP)	// UNITY_UV_STARTS_AT_TOP is for directx
	if (!isYFlipped)
	{
		// Currently this only runs for Android and Windows using DirectShow
		offset.y = 0.5 - offset.y;
	}
#endif
#endif

	// Left-Right 
#elif defined(STEREO_LEFT_RIGHT)

	scale.x = 0.5;
	offset.x = 0.0;
	if (!isLeftEye)
	{
		offset.x = 0.5;
	}

#endif

	return FLOAT4(scale, offset);
}
#endif

#if defined(STEREO_DEBUG)
INLINE FLOAT4 GetStereoDebugTint(bool isLeftEye)
{
	FLOAT4 tint = FLOAT4(1.0, 1.0, 1.0, 1.0);

#if defined(STEREO_TOP_BOTTOM) || defined(STEREO_LEFT_RIGHT) || defined(STEREO_CUSTOM_UV)
	FLOAT4 leftEyeColor = FLOAT4(0.0, 1.0, 0.0, 1.0);		// green
	FLOAT4 rightEyeColor = FLOAT4(1.0, 0.0, 0.0, 1.0);		// red

	if (isLeftEye)
	{
		tint = leftEyeColor;
	}
	else
	{
		tint = rightEyeColor;
	}
#endif

#if defined(UNITY_UV_STARTS_AT_TOP)
	tint.b = 0.5;
#endif
/*#if defined(UNITY_SINGLE_PASS_STEREO) || defined(UNITY_STEREO_INSTANCING_ENABLED) || defined(UNITY_DECLARE_MULTIVIEW)
	tint.b = 1.0;
#endif*/

	return tint;
}
#endif

FLOAT2 ScaleZoomToFit(float targetWidth, float targetHeight, float sourceWidth, float sourceHeight)
{
#if defined(ALPHAPACK_TOP_BOTTOM)
	sourceHeight *= 0.5;
#elif defined(ALPHAPACK_LEFT_RIGHT)
	sourceWidth *= 0.5;
#endif
	float targetAspect = targetHeight / targetWidth;
	float sourceAspect = sourceHeight / sourceWidth;
	FLOAT2 scale = FLOAT2(1.0, sourceAspect / targetAspect);
	if (targetAspect < sourceAspect)
	{
		scale = FLOAT2(targetAspect / sourceAspect, 1.0);
	}
	return scale;
}

FLOAT4 OffsetAlphaPackingUV(FLOAT2 texelSize, FLOAT2 uv, bool flipVertical)
{
	FLOAT4 result = uv.xyxy;

	// We don't want bilinear interpolation to cause bleeding
	// when reading the pixels at the edge of the packed areas.
	// So we shift the UV's by a fraction of a pixel so the edges don't get sampled.

#if defined(ALPHAPACK_TOP_BOTTOM)
	float offset = texelSize.y * 1.5;
	result.y = lerp(0.0 + offset, 0.5 - offset, uv.y);
	result.w = result.y + 0.5;

	if (flipVertical)
	{
		// Flip vertically (and offset to put back in 0..1 range)
		result.yw = 1.0 - result.yw;
		result.yw = result.wy;
	}
	else
	{
#if !defined(UNITY_UV_STARTS_AT_TOP)
		// For opengl we flip
		result.yw = result.wy;
#endif
	}

#elif defined(ALPHAPACK_LEFT_RIGHT)
	float offset = texelSize.x * 1.5;
	result.x = lerp(0.0 + offset, 0.5 - offset, uv.x);
	result.z = result.x + 0.5;

	if (flipVertical)
	{
		// Flip vertically (and offset to put back in 0..1 range)
		result.yw = 1.0 - result.yw;
	}

#else

	if (flipVertical)
	{
		// Flip vertically (and offset to put back in 0..1 range)
		result.yw = 1.0 - result.yw;
	}

#endif

	return result;
}


// http://entropymine.com/imageworsener/srgbformula/
INLINE HALF3 GammaToLinear(HALF3 col)
{
// Forced cheap version
#if defined(CHEAP_GAMMATOLINEAR)
#if defined (SHADERLAB_GLSL)
	return pow(col, vec3(2.2, 2.2, 2.2));
#else
	// Approximate version from http://chilliant.blogspot.com.au/2012/08/srgb-approximations-for-hlsl.html?m=1
	return col * (col * (col * 0.305306011h + 0.682171111h) + 0.012522878h);
#endif
#else

#if SHADER_TARGET < 30
// Cheaper approximation
#if defined (SHADERLAB_GLSL)
	return col * (col * (col * 0.305306011 + 0.682171111) + 0.012522878);
#else
	return col * (col * (col * 0.305306011h + 0.682171111h) + 0.012522878h);
#endif
#else
// Accurate version
	if (col.r <= 0.04045)
		col.r = col.r / 12.92;
	else
		col.r = pow((col.r + 0.055) / 1.055, 2.4);

	if (col.g <= 0.04045)
		col.g = col.g / 12.92;
	else
		col.g = pow((col.g + 0.055) / 1.055, 2.4);

	if (col.b <= 0.04045)
		col.b = col.b / 12.92;
	else
		col.b = pow((col.b + 0.055) / 1.055, 2.4);
#endif
#endif
	return col;
}

INLINE HALF3 LinearToGamma(HALF3 col)
{
// Forced cheap version
#if defined(CHEAP_GAMMATOLINEAR)
#if defined (SHADERLAB_GLSL)
	return pow(col, vec3(1.0 / 2.2, 1.0 / 2.2, 1.0 / 2.2));
#else
	// Approximate version from http://chilliant.blogspot.com.au/2012/08/srgb-approximations-for-hlsl.html?m=1
	return max(1.055h * pow(col, 0.416666667h) - 0.055h, 0.0h);
#endif
#else

#if SHADER_TARGET < 30
// Cheaper approximation
#if defined (SHADERLAB_GLSL)
	return max(1.055 * pow(col, vec3(0.416666667, 0.416666667, 0.416666667)) - 0.055, 0.0);
#else
	return max(1.055h * pow(col, 0.416666667h) - 0.055h, 0.0h);
#endif
#else
// Accurate version
	if (col.r <= 0.0031308)
		col.r = col.r * 12.92;
	else
		col.r = 1.055 * pow(col.r, 0.4166667) - 0.055;

	if (col.g <= 0.0031308)
		col.g = col.g * 12.92;
	else
		col.g = 1.055 * pow(col.g, 0.4166667) - 0.055;

	if (col.b <= 0.0031308)
		col.b = col.b * 12.92;
	else
		col.b = 1.055 * pow(col.b, 0.4166667) - 0.055;
#endif
#endif
	return col;
}

INLINE FLOAT3 Convert420YpCbCr8ToRGB(FLOAT3 ypcbcr)
{
#if 1
	// Full range [0...255]
	FLOAT3X3 m = FLOAT3X3(
		1.0,  0.0,      1.402,
		1.0, -0.34414, -0.71414,
		1.0,  1.77200,  0.0
	);
	FLOAT3 o = FLOAT3(0.0, -0.5, -0.5);
#else
	// Video range [16...235]
	FLOAT3X3 m = FLOAT3X3(
		1.1643,  0.0,      1.5958,
		1.1643, -0.39173, -0.81290,
		1.1643,  2.017,    0.0
	);
	FLOAT3 o = FLOAT3(-0.0625, -0.5, -0.5);
#endif

#if defined(SHADERLAB_GLSL)
	return m * (ypcbcr + o);
#else
	return mul(m, ypcbcr + o);
#endif
}
