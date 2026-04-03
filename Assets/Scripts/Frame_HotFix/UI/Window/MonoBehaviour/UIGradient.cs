using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static MathUtility;

// 用于实现渐变效果,可用于文字,图片等
public class UIGradient : BaseMeshEffect
{
    public GRADIENT_DIRECTION mGradientDirection;   // 渐变方向
    public GRADIENT_BLEND mGradientBlendMode;       // 颜色混合模式
    [Range(-1, 1), Tooltip("[渐变偏移量]当偏移量为正值时，渐变效果会向后移动。当偏移量为负值时，渐变效果会向前移动。当偏移量为零时，渐变效果按照默认设置应用")] 
    public float mOffset;                           // 渐变偏移量
    public Gradient mGradient;                      // 渐变颜色
    public UIGradient()
    {
        mGradientDirection = GRADIENT_DIRECTION.VERTICAL;
        mGradientBlendMode = GRADIENT_BLEND.OVERRIDE;
        mGradient = new()
        {
            colorKeys = new GradientColorKey[]
            {
                new(Color.black, 0),
                new(Color.white, 1)
            }
        };
    }
    // 重写ModifyMesh方法以修改UI网格的顶点颜色
    public override void ModifyMesh(VertexHelper vh)
    {
        int vertCount = vh.currentVertCount;
		if (!IsActive() || vertCount == 0)
        {
            return;
        }
        using var a = new ListScope<UIVertex>(out var vertices);
		vh.GetUIVertexStream(vertices);
		bool isHorizontal = mGradientDirection == GRADIENT_DIRECTION.HORIZONTAL;
		float inverseLength = calculateInverseLength(vertices, isHorizontal, out float min, out float max);
		UIVertex vertex = new();
		for (int i = 0; i < vertCount; ++i)
		{
			vh.PopulateUIVertex(ref vertex, i);
			float normalizedPosition = (isHorizontal ? (vertex.position.x - min) : (vertex.position.y - min)) * inverseLength - mOffset;
			vertex.color = blendColors(vertex.color, mGradient.Evaluate(normalizedPosition));
			vh.SetUIVertex(vertex, i);
		}
    }
	//------------------------------------------------------------------------------------------------------------------------------
	// 计算顶点的最小和最大位置以及逆长度
	protected float calculateInverseLength(List<UIVertex> vertices, bool isHorizontal, out float min, out float max)
    {
        min = isHorizontal ? vertices[0].position.x : vertices[0].position.y;
        max = min;
        foreach (UIVertex vertex in vertices)
        {
            float position = isHorizontal ? vertex.position.x : vertex.position.y;
            clampMin(ref max, position);
            clampMax(ref min, position);
        }
        return divide(1.0f, max - min);
    }
	// 根据混合模式混合颜色
	protected Color blendColors(Color originalColor, Color newColor)
    {
        switch (mGradientBlendMode)
        {
            case GRADIENT_BLEND.OVERRIDE:   return newColor;
            case GRADIENT_BLEND.ADD:        return originalColor + newColor;
            case GRADIENT_BLEND.MULTIPLY:   return originalColor * newColor;
		}
        return originalColor;
	}
}