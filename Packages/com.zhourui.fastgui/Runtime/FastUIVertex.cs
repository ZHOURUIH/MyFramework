using UnityEngine.Rendering;

// FastUI顶点布局公共定义。
// 纯Image Canvas继续使用Position/Color/UV三个独立Stream；只有Canvas中存在FastText时才启用TMP扩展UV布局。
public static class FastUIVertex
{
	public const int POSITION_STREAM = 0;
	public const int COLOR_STREAM = 1;
	public const int UV_STREAM = 2;
	public const int TMP_UV2_STREAM = 3;
	public static readonly VertexAttributeDescriptor[] IMAGE_VERTEX_LAYOUT =
	{
		new(VertexAttribute.Position, VertexAttributeFormat.Float32, 3, POSITION_STREAM),
		new(VertexAttribute.Color, VertexAttributeFormat.UNorm8, 4, COLOR_STREAM),
		new(VertexAttribute.TexCoord0, VertexAttributeFormat.Float32, 2, UV_STREAM),
	};
	public static readonly VertexAttributeDescriptor[] TMP_VERTEX_LAYOUT =
	{
		new(VertexAttribute.Position, VertexAttributeFormat.Float32, 3, POSITION_STREAM),
		new(VertexAttribute.Color, VertexAttributeFormat.UNorm8, 4, COLOR_STREAM),
		new(VertexAttribute.TexCoord0, VertexAttributeFormat.Float32, 4, UV_STREAM),
		new(VertexAttribute.TexCoord1, VertexAttributeFormat.Float32, 2, TMP_UV2_STREAM),
	};
}
