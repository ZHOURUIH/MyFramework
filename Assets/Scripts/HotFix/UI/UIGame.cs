using System.Collections.Generic;
using Obfuz;
using UnityEngine;
using static StringUtility;
using static MathUtility;
using static FrameBaseHotFix;

// auto generate member start
// generate from:Assets/GameResources/UI/UIPrefab/UIGame.prefab
// 游戏界面
[ObfuzIgnore(ObfuzScope.TypeName)]
public class UIGame : LayoutScript
{
	protected myUGUIObject mAvatar;
	protected myUGUIText mSpeed;
	protected myUGUIDamageNumber mDamageNumber;
	// auto generate member end
	public UIGame()
	{
		// auto generate constructor start
		// auto generate constructor end
	}
	public override void assignWindow()
	{
		// auto generate assignWindow start
		newObject(out myUGUIObject background, "Background", false);
		newObject(out mAvatar, background, "Avatar");
		newObject(out mSpeed, background, "Speed");
		newObject(out mDamageNumber, background, "DamageNumber");
		// auto generate assignWindow end
	}
	public override void init()
	{
		base.init();
		mDamageNumber.setInterval(-10);
	}
	public override void onGameState()
	{
		base.onGameState();
	}
	public override void update(float elapsedTime)
	{
		base.update(elapsedTime);
		for (int i = 0; i < 30; ++i)
		{
			showNumber(new Vector3(randomFloat(400, -400), randomFloat(400, -400)), randomInt(10, 10000));
		}
	}
	public void setAvatarPosition(Vector3 pos)
	{
		mAvatar.setPosition(pos);
	}
	public void setSpeed(float speed)
	{
		mSpeed.setText("速度:" + FToS(speed, 0));
	}
	// 显示伤害数字,position是数字的世界坐标
	public void showNumber(Vector3 position, int number)
	{
		Vector3 randomOffset = new(randomFloat(-20.0f, 20.0f), randomFloat(-5.0f, 5.0f));
		mDamageNumber.addDamage(position + randomOffset, new(0.3f, 0.3f, 0.3f), number, getTranslate("pg01"), getScale("pg01"));
	}
	protected Dictionary<float, Vector3> getTranslate(string pathName) { return mPathKeyframeManager.getTranslatePath(pathName); }
	protected Dictionary<float, Vector3> getScale(string pathName) { return mPathKeyframeManager.getScalePath(pathName); }
}
