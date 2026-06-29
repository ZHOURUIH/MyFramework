using System.Collections.Generic;
using UnityEngine;

// 因为有些泛型类型在热更dll中始终会报错,原因未知,可能是补充元数据失败,所以暂时只能在AOT中实例化此泛型类型
public class AOTGenericInstantiate
{
	public static HashSet<char> mList0 = new();
	public static HashSet<Vector2Int> mList1 = new();
	public static Dictionary<char, Sprite> mList2 = new();
}