using UnityEngine;
using UnityEditor;
using static UnityUtility;

public class MenuUGUIGenerator
{
	[MenuItem("GameObject/将节点添加到成员变量 %W", false, 30)]
	static void makeGameObjectAsMember()
	{
		if (Selection.gameObjects.isEmpty())
		{
			Debug.LogError("请先选择一个节点");
			return;
		}
		foreach (GameObject go in Selection.gameObjects)
		{
			//从父节点中向上遍历找到第一个UGUIGenerator或者UGUISubGenerator组件
			UGUIGeneratorBase generator = getComponentInParent<UGUISubGenerator>(go);
			if (generator == null)
			{
				generator = go.GetComponentInParent<UGUIGenerator>();
			}
			if (generator == null)
			{
				Debug.LogError("所有父节点中没有找到UGUIGenerator或者UGUISubGenerator组件");
				return;
			}
			generator.addMember(go);
		}
	}
}