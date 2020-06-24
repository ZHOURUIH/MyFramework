#if UNITY_5_4_OR_NEWER || (UNITY_5 && !UNITY_5_0)
	#define UNITY_HELPATTRIB
#endif

using UnityEngine;

//-----------------------------------------------------------------------------
// Copyright 2015-2018 RenderHeads Ltd.  All rights reserverd.
//-----------------------------------------------------------------------------


namespace RenderHeads.Media.AVProVideo
{
	/// <summary>
	/// Draws video over the whole background using the special "background" tag on the shader.
	/// Useful for augmented reality.
	/// NOTE: This doesn't work with the camera clear mode set to 'skybox'
	/// </summary>
	[AddComponentMenu("AVPro Video/Display Background", 200)]
#if UNITY_HELPATTRIB
	[HelpURL("http://renderheads.com/product/avpro-video/")]
#endif
	[ExecuteInEditMode]
	public class DisplayBackground : MonoBehaviour
	{
		public IMediaProducer _source;

		public Texture2D _texture;
		public Material _material;
		
		//-------------------------------------------------------------------------

		void OnRenderObject()
		{
			if (_material == null || _texture == null)
				return;

			Vector4 uv = new Vector4(0f, 0f, 1f, 1f);
			_material.SetPass(0);
			GL.PushMatrix();
			GL.LoadOrtho();
			GL.Begin(GL.QUADS);
			
			GL.TexCoord2(uv.x, uv.y);
			GL.Vertex3(0.0f, 0.0f, 0.1f);
			
			GL.TexCoord2(uv.z, uv.y);
			GL.Vertex3(1.0f, 0.0f, 0.1f);
			
			GL.TexCoord2(uv.z, uv.w);		
			GL.Vertex3(1.0f, 1.0f, 0.1f);
			
			GL.TexCoord2(uv.x, uv.w);
			GL.Vertex3(0.0f, 1.0f, 0.1f);
			
			GL.End();
			GL.PopMatrix();
		}
	}
}